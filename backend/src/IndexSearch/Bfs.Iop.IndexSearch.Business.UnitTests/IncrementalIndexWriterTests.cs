using AwesomeAssertions;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.IndexSearch.Business.Sources;
using Bfs.Iop.IndexSearch.Contracts.Indexing;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bfs.Iop.IndexSearch.Business.UnitTests;

[TestFixture]
internal sealed class IncrementalIndexWriterTests
{
    private static readonly Guid _dataset = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid _concept = Guid.Parse("22222222-2222-2222-2222-222222222222");

    [Test]
    public async Task A_dataset_is_written_with_the_structure_flag_resolved()
    {
        // The document is written whole and the flag does not come from the dataset row, so without
        // resolving it here the write would erase it and empty the Structures facet for that dataset.
        var writer = new RecordingCatalogWriter();

        await Create(writer, structures: new HashSet<Guid> { _dataset })
            .UpsertCatalogResourceAsync(SearchResourceType.Dataset, _dataset);

        writer.Written.Should().ContainSingle()
            .Which.HasStructure.Should().BeTrue();
    }

    [Test]
    public async Task A_dataset_with_no_structure_is_written_with_the_flag_false()
    {
        var writer = new RecordingCatalogWriter();

        await Create(writer, structures: new HashSet<Guid>())
            .UpsertCatalogResourceAsync(SearchResourceType.Dataset, _dataset);

        writer.Written.Should().ContainSingle()
            .Which.HasStructure.Should().BeFalse();
    }

    [Test]
    public async Task A_dataset_is_still_written_when_the_structure_cannot_be_read()
    {
        // The document mostly carries the publication level that search authorization filters on.
        // Refusing to write it because the structure store is down would leave a resource that was
        // just unpublished still visible to anonymous callers - far worse than a wrong facet.
        var writer = new RecordingCatalogWriter();

        await Create(writer, structures: null)
            .UpsertCatalogResourceAsync(SearchResourceType.Dataset, _dataset);

        writer.Written.Should().ContainSingle();
    }

    [Test]
    public async Task A_structure_that_cannot_be_read_keeps_the_flag_the_index_already_holds()
    {
        var writer = new RecordingCatalogWriter();
        var reader = new StubCatalogIndexReader(indexed: true);

        await Create(writer, structures: null, indexReader: reader)
            .UpsertCatalogResourceAsync(SearchResourceType.Dataset, _dataset);

        reader.Reads.Should().Be(1);
        writer.Written.Should().ContainSingle().Which.HasStructure.Should().BeTrue();
    }

    [Test]
    public async Task A_structure_that_cannot_be_read_for_a_dataset_the_index_does_not_hold_stays_unset()
    {
        var writer = new RecordingCatalogWriter();

        await Create(writer, structures: null, indexReader: new StubCatalogIndexReader(indexed: null))
            .UpsertCatalogResourceAsync(SearchResourceType.Dataset, _dataset);

        writer.Written.Should().ContainSingle().Which.HasStructure.Should().BeNull();
    }

    [Test]
    public async Task A_structure_that_resolves_is_never_read_back_from_the_index()
    {
        var writer = new RecordingCatalogWriter();
        var reader = new StubCatalogIndexReader(indexed: false);

        await Create(writer, new HashSet<Guid> { _dataset }, indexReader: reader)
            .UpsertCatalogResourceAsync(SearchResourceType.Dataset, _dataset);

        reader.Reads.Should().Be(0, "the source answered, so there is nothing to carry forward");
        writer.Written.Should().ContainSingle().Which.HasStructure.Should().BeTrue();
    }

    // TaskCanceledException is the one that matters: HttpClient reports its own timeout that way even
    // though nobody cancelled anything, so a catch written against OperationCanceledException lets the
    // commonest failure of all through.
    [TestCaseSource(nameof(ReadFailures))]
    public async Task A_dataset_is_still_written_when_the_index_cannot_be_read_either(Exception failure)
    {
        var writer = new RecordingCatalogWriter();

        await Create(writer, structures: null, indexReader: new StubCatalogIndexReader(throws: failure))
            .UpsertCatalogResourceAsync(SearchResourceType.Dataset, _dataset);

        writer.Written.Should().ContainSingle().Which.HasStructure.Should().BeNull();
    }

    [Test]
    public async Task A_single_write_keeps_the_reuse_count_the_index_already_holds()
    {
        var writer = new RecordingCatalogWriter();
        var reader = new StubCatalogIndexReader(indexedReuse: 12);

        await Create(writer, structures: null, indexReader: reader)
            .UpsertCatalogResourceAsync(SearchResourceType.Concept, _concept);

        // The write replaces the whole document. Nothing about editing a concept changes how many
        // resources reuse it, so dropping the count would sink it among equally relevant results.
        reader.ReuseReads.Should().Be(1);
        writer.Written.Should().ContainSingle().Which.ReuseCount.Should().Be(12);
    }

    [Test]
    public async Task A_resource_the_index_does_not_hold_yet_is_written_without_a_reuse_count()
    {
        var writer = new RecordingCatalogWriter();

        await Create(writer, structures: null, indexReader: new StubCatalogIndexReader(indexedReuse: null))
            .UpsertCatalogResourceAsync(SearchResourceType.Concept, _concept);

        // Zero would claim nothing reuses it, which is a different thing from not knowing yet.
        writer.Written.Should().ContainSingle().Which.ReuseCount.Should().BeNull();
    }

    [TestCaseSource(nameof(ReadFailures))]
    public async Task A_resource_is_still_written_when_the_reuse_count_cannot_be_read(Exception failure)
    {
        var writer = new RecordingCatalogWriter();

        await Create(writer, structures: null, indexReader: new StubCatalogIndexReader(throws: failure))
            .UpsertCatalogResourceAsync(SearchResourceType.Concept, _concept);

        writer.Written.Should().ContainSingle().Which.ReuseCount.Should().BeNull();
    }

    private static IEnumerable<TestCaseData> ReadFailures()
    {
        yield return new TestCaseData(new HttpRequestException("the shard was relocating"))
            .SetName("the node answered with an error");

        yield return new TestCaseData(new TaskCanceledException("timed out", new TimeoutException()))
            .SetName("the read timed out while a rebuild saturated the cluster");
    }

    [Test]
    public async Task A_shutdown_is_not_swallowed_as_a_failed_read()
    {
        // The mirror of the above: a token that really was cancelled must stop the write rather than
        // index a document whose flag nobody even tried to read.
        using var shuttingDown = new CancellationTokenSource();
        await shuttingDown.CancelAsync();

        var write = async () => await Create(
                new RecordingCatalogWriter(),
                structures: null,
                indexReader: new StubCatalogIndexReader(throws: new TaskCanceledException()))
            .UpsertCatalogResourceAsync(SearchResourceType.Dataset, _dataset, shuttingDown.Token);

        await write.Should().ThrowAsync<OperationCanceledException>();
    }

    [Test]
    public async Task A_type_that_is_not_a_dataset_is_never_asked_about_structures()
    {
        var writer = new RecordingCatalogWriter();
        var structures = new CountingStructureSource();

        await Create(writer, structures)
            .UpsertCatalogResourceAsync(SearchResourceType.Concept, _concept);

        structures.Probes.Should().Be(0);
        writer.Written.Should().ContainSingle().Which.HasStructure.Should().BeNull();
    }

    [Test]
    public async Task A_resource_that_no_longer_exists_is_deleted_instead_of_written()
    {
        // A delete can overtake its own notification. Treating the missing row as an error would leave
        // the deleted resource searchable until the next rebuild.
        var writer = new RecordingCatalogWriter();

        await Create(writer, catalog: new StubCatalogSource(missing: true))
            .UpsertCatalogResourceAsync(SearchResourceType.Dataset, _dataset);

        writer.Written.Should().BeEmpty();
        writer.Deleted.Should().ContainSingle().Which.Should().Be(_dataset);
    }

    [Test]
    public async Task Replacing_a_code_list_writes_before_it_removes_anything()
    {
        // Emptying the concept first means a failure in between, or a search in between, finds a code
        // list with no entries at all and nothing puts them back before the next rebuild. Writing
        // first makes the worst case an entry that lingers one replacement too long.
        var writer = new RecordingCodeListWriter();
        var stale = Guid.Parse("33333333-3333-3333-3333-333333333333");

        await Create(codeLists: writer, codeListReader: new StubCodeListIndexReader(stale))
            .ReplaceCodeListAsync(_concept);

        writer.Calls.Should().Equal("write", "delete-by-id");
    }

    [Test]
    public async Task Only_the_entries_the_database_no_longer_lists_are_removed()
    {
        // Entries still in the concept are overwritten in place by the write; deleting them as well
        // would take them out of the index between the two calls for no reason.
        var writer = new RecordingCodeListWriter();
        var kept = Entry(_concept);
        var stale = Guid.Parse("33333333-3333-3333-3333-333333333333");

        await Create(
                codeLists: writer,
                codeListSource: new StubCodeListSource([kept]),
                codeListReader: new StubCodeListIndexReader(kept.Id, stale))
            .ReplaceCodeListAsync(_concept);

        writer.DeletedIds.Should().Equal(stale);
    }

    [Test]
    public async Task A_replacement_that_removes_nothing_costs_no_delete()
    {
        var writer = new RecordingCodeListWriter();
        var kept = Entry(_concept);

        await Create(
                codeLists: writer,
                codeListSource: new StubCodeListSource([kept]),
                codeListReader: new StubCodeListIndexReader(kept.Id))
            .ReplaceCodeListAsync(_concept);

        writer.Calls.Should().Equal("write");
    }

    [Test]
    public async Task A_concept_whose_entries_are_all_gone_is_still_cleared()
    {
        var writer = new RecordingCodeListWriter();

        await Create(codeLists: writer, codeListSource: new StubCodeListSource([])).ReplaceCodeListAsync(_concept);

        writer.Calls.Should().Equal("delete-by-concept");
        writer.DeletedConcepts.Should().ContainSingle();
    }

    [Test]
    public async Task A_rejected_write_fails_rather_than_reporting_success()
    {
        // The bulk answers 200 and rejects the item inside it. Swallowing that would let the endpoint
        // answer 204, the dispatcher would count the notification delivered, and the stale document
        // would survive until the next nightly rebuild with nothing retrying it.
        var writer = new RecordingCatalogWriter { Rejects = true };

        var upsert = async () => await Create(writer, structures: new HashSet<Guid>())
            .UpsertCatalogResourceAsync(SearchResourceType.Dataset, _dataset);

        await upsert.Should().ThrowAsync<InvalidOperationException>();
    }

    [Test]
    public async Task A_rejected_removal_fails_rather_than_reporting_success()
    {
        // Deleting a document that is already gone counts as accepted, so a shortfall is a genuine
        // rejection - and a resource left searchable after it was deleted is the worst of these.
        var writer = new RecordingCatalogWriter { Rejects = true };

        var remove = async () => await Create(writer).RemoveCatalogResourceAsync(_dataset);

        await remove.Should().ThrowAsync<InvalidOperationException>();
    }

    private static IncrementalIndexWriter Create(
        ICatalogIndexWriter? catalogWriter = null,
        IDatasetStructureSource? structures = null,
        ICatalogDocumentSource? catalog = null,
        ICodeListIndexWriter? codeLists = null,
        ICodeListDocumentSource? codeListSource = null,
        ICatalogIndexReader? indexReader = null,
        ICodeListIndexReader? codeListReader = null) =>
        new(
            catalog ?? new StubCatalogSource(),
            codeListSource ?? new StubCodeListSource([Entry(_concept)]),
            catalogWriter ?? new RecordingCatalogWriter(),
            codeLists ?? new RecordingCodeListWriter(),
            structures ?? new CountingStructureSource(),
            indexReader ?? new StubCatalogIndexReader(),
            codeListReader ?? new StubCodeListIndexReader(),
            new CodeListReplacementGate(),
            NullLogger<IncrementalIndexWriter>.Instance);

    /// <summary>An index holding exactly what the source is about to write, so nothing is stale.</summary>
    private sealed class StubCodeListIndexReader : ICodeListIndexReader
    {
        private readonly Guid[] _ids;

        public StubCodeListIndexReader(params Guid[] ids) => _ids = ids;

        public Task<IReadOnlyCollection<Guid>> ReadEntryIdsAsync(
            Guid conceptId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<Guid>>(_ids);
    }

    private static IncrementalIndexWriter Create(
        RecordingCatalogWriter writer,
        IReadOnlySet<Guid>? structures,
        ICatalogIndexReader? indexReader = null) =>
        Create(writer, new CountingStructureSource(structures), indexReader: indexReader);

    private static CodeListIndexDocument Entry(Guid conceptId) => new()
    {
        Id = Guid.NewGuid(),
        ConceptId = conceptId,
        Code = "1",
    };

    private sealed class StubCatalogSource(bool missing = false) : ICatalogDocumentSource
    {
        public IAsyncEnumerable<IReadOnlyList<CatalogIndexDocument>> ReadAllAsync(
            int batchSize,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("A single write never reads everything.");

        public Task<CatalogIndexDocument?> ReadOneAsync(
            SearchResourceType type,
            Guid id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<CatalogIndexDocument?>(
                missing ? null : new CatalogIndexDocument { Id = id, Type = type });
    }

    private sealed class StubCodeListSource(IReadOnlyList<CodeListIndexDocument> documents) : ICodeListDocumentSource
    {
        public IAsyncEnumerable<IReadOnlyList<CodeListIndexDocument>> ReadAllAsync(
            int batchSize,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("A single write never reads everything.");

        public Task<IReadOnlyList<CodeListIndexDocument>> ReadConceptAsync(
            Guid conceptId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(documents);
    }

    private sealed class StubCatalogIndexReader(
        bool? indexed = null,
        Exception? throws = null,
        int? indexedReuse = null) : ICatalogIndexReader
    {
        public int Reads { get; private set; }

        public int ReuseReads { get; private set; }

        public Task<bool?> ReadStructureFlagAsync(Guid datasetId, CancellationToken cancellationToken = default)
        {
            Reads++;

            return throws is null
                ? Task.FromResult(indexed)
                : Task.FromException<bool?>(throws);
        }

        public Task<int?> ReadReuseCountAsync(Guid id, CancellationToken cancellationToken = default)
        {
            ReuseReads++;

            return throws is null
                ? Task.FromResult(indexedReuse)
                : Task.FromException<int?>(throws);
        }
    }

    private sealed class CountingStructureSource(IReadOnlySet<Guid>? structures = null) : IDatasetStructureSource
    {
        public int Probes { get; private set; }

        public bool IsConfigured => true;

        public Task<IReadOnlySet<Guid>?> GetIdsWithStructuresAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(structures);

        public Task<bool?> HasStructureAsync(Guid datasetId, CancellationToken cancellationToken = default)
        {
            Probes++;

            return Task.FromResult<bool?>(structures?.Contains(datasetId));
        }
    }

    private sealed class RecordingCatalogWriter : ICatalogIndexWriter
    {
        /// <summary>A bulk request can answer 200 and still reject the item inside it.</summary>
        public bool Rejects { get; init; }

        public List<CatalogIndexDocument> Written { get; } = [];

        public List<Guid> Deleted { get; } = [];

        public Task<int> WriteAsync(
            IReadOnlyCollection<CatalogIndexDocument> documents,
            CancellationToken cancellationToken = default)
        {
            Written.AddRange(documents);

            return Task.FromResult(Rejects ? 0 : documents.Count);
        }

        public Task<int> DeleteAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
        {
            Deleted.AddRange(ids);

            return Task.FromResult(Rejects ? 0 : ids.Count);
        }
    }

    private sealed class RecordingCodeListWriter : ICodeListIndexWriter
    {
        public List<string> Calls { get; } = [];

        public List<Guid> DeletedConcepts { get; } = [];

        public Task<int> WriteAsync(
            IReadOnlyCollection<CodeListIndexDocument> documents,
            CancellationToken cancellationToken = default)
        {
            Calls.Add("write");

            return Task.FromResult(documents.Count);
        }

        public List<Guid> DeletedIds { get; } = [];

        public Task<int> DeleteAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
        {
            Calls.Add("delete-by-id");
            DeletedIds.AddRange(ids);

            return Task.FromResult(ids.Count);
        }

        public Task<int> DeleteByConceptAsync(Guid conceptId, CancellationToken cancellationToken = default)
        {
            Calls.Add("delete-by-concept");
            DeletedConcepts.Add(conceptId);

            return Task.FromResult(1);
        }
    }
}
