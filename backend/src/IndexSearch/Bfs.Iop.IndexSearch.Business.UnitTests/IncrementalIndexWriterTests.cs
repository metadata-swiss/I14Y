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
    public async Task Replacing_a_code_list_deletes_the_whole_concept_before_writing()
    {
        // Entries removed from the concept have no document to overwrite, so writing alone would leave
        // them searchable forever. The order matters as much as the delete does.
        var writer = new RecordingCodeListWriter();

        await Create(codeLists: writer).ReplaceCodeListAsync(_concept);

        writer.Calls.Should().Equal("delete-by-concept", "write");
        writer.DeletedConcepts.Should().ContainSingle().Which.Should().Be(_concept);
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
        ICodeListDocumentSource? codeListSource = null) =>
        new(
            catalog ?? new StubCatalogSource(),
            codeListSource ?? new StubCodeListSource([Entry(_concept)]),
            catalogWriter ?? new RecordingCatalogWriter(),
            codeLists ?? new RecordingCodeListWriter(),
            structures ?? new CountingStructureSource(),
            NullLogger<IncrementalIndexWriter>.Instance);

    private static IncrementalIndexWriter Create(
        RecordingCatalogWriter writer,
        IReadOnlySet<Guid>? structures) =>
        Create(writer, new CountingStructureSource(structures));

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

        public Task<int> DeleteAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
        {
            Calls.Add("delete-by-id");

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
