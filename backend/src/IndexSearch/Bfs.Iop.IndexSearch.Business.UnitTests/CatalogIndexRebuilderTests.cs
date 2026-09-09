using Bfs.Iop.DataAccess.Abstractions;
using System.Runtime.CompilerServices;
using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Business.Sources;
using Bfs.Iop.IndexSearch.Contracts;
using Bfs.Iop.IndexSearch.Contracts.Indexing;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bfs.Iop.IndexSearch.Business.UnitTests;

[TestFixture]
internal sealed class CatalogIndexRebuilderTests
{
    private static readonly Guid _datasetWithStructure = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid _datasetWithoutStructure = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid _concept = Guid.Parse("33333333-3333-3333-3333-333333333333");

    [Test]
    public async Task Datasets_are_flagged_from_the_structure_source_and_other_types_are_left_alone()
    {
        var writer = new RecordingWriter();

        var report = await CreateRebuilder(writer, structures: new HashSet<Guid> { _datasetWithStructure }).RebuildAsync(batchSize: 10);

        report.StructuresResolved.Should().BeTrue();
        report.DocumentsWritten.Should().Be(3);

        Flag(writer, _datasetWithStructure).Should().BeTrue();
        Flag(writer, _datasetWithoutStructure).Should().BeFalse();

        // Not a dataset, so the flag does not apply and must stay absent rather than become false.
        Flag(writer, _concept).Should().BeNull();
    }

    [Test]
    public async Task An_unreadable_structure_source_leaves_the_indexed_flag_untouched()
    {
        var writer = new RecordingWriter();

        var report = await CreateRebuilder(writer, structures: null).RebuildAsync(batchSize: 10);

        // Everything is still indexed, and nothing claims to know about structures — writing false here
        // would strip the flag from every dataset for the length of the outage.
        report.DocumentsWritten.Should().Be(3);
        report.StructuresResolved.Should().BeFalse();
        Flag(writer, _datasetWithStructure).Should().BeNull();
        Flag(writer, _datasetWithoutStructure).Should().BeNull();
    }

    [Test]
    public async Task Documents_the_index_turns_away_are_not_counted_as_written()
    {
        // A bulk write can reject documents one at a time and still succeed overall. Counting the batch
        // size would report a rebuild that lost a third of the catalogue as a complete one.
        var writer = new RecordingWriter { AcceptPerBatch = 2 };

        var report = await CreateRebuilder(writer, structures: new HashSet<Guid>()).RebuildAsync(batchSize: 3);

        report.DocumentsSent.Should().Be(3);
        report.DocumentsWritten.Should().Be(2);
        report.BatchesFailed.Should().Be(0);
    }
    [Test]
    public async Task A_batch_that_cannot_be_written_does_not_abandon_the_rest()
    {
        var writer = new RecordingWriter { FailOnCall = 1 };

        // One document per batch, so the first batch fails and the two after it must still arrive.
        var report = await CreateRebuilder(writer, structures: new HashSet<Guid>()).RebuildAsync(batchSize: 1);

        report.BatchesFailed.Should().Be(1);
        report.DocumentsWritten.Should().Be(2);
        writer.Written.Should().HaveCount(2);

        // Everything handed over is counted, the lost batch included, so that sent minus written is
        // the whole shortfall rather than only the part the index refused document by document.
        report.DocumentsSent.Should().Be(3);
    }

    private static CatalogIndexRebuilder CreateRebuilder(ICatalogIndexWriter writer, IReadOnlySet<Guid>? structures) =>
        new(new StubSource(), new StubStructureSource(structures), writer, NullLogger<CatalogIndexRebuilder>.Instance);

    private static bool? Flag(RecordingWriter writer, Guid id) =>
        writer.Written.Single(x => x.Id == id).HasStructure;

    private sealed class StubSource : ICatalogDocumentSource
    {
        public async IAsyncEnumerable<IReadOnlyList<CatalogIndexDocument>> ReadAllAsync(
            int batchSize,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            CatalogIndexDocument[] documents =
            [
                new() { Id = _datasetWithStructure, Type = SearchResourceType.Dataset },
                new() { Id = _datasetWithoutStructure, Type = SearchResourceType.Dataset },
                new() { Id = _concept, Type = SearchResourceType.Concept },
            ];

            foreach (var chunk in documents.Chunk(batchSize))
            {
                yield return chunk;
            }

            await Task.CompletedTask;
        }
    }

    private sealed class StubStructureSource(IReadOnlySet<Guid>? structures) : IDatasetStructureSource
    {
        public Task<IReadOnlySet<Guid>?> GetIdsWithStructuresAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(structures);
    }

    private sealed class RecordingWriter : ICatalogIndexWriter
    {
        private int _calls;

        public int? FailOnCall { get; init; }

        public List<CatalogIndexDocument> Written { get; } = [];

        // How many of each batch the index accepts. Null means all of them.
        public int? AcceptPerBatch { get; init; }

        public Task<int> WriteAsync(
            IReadOnlyCollection<CatalogIndexDocument> documents,
            CancellationToken cancellationToken = default)
        {
            _calls++;

            if (_calls == FailOnCall)
            {
                throw new InvalidOperationException("The index rejected the batch.");
            }

            var accepted = documents.Take(AcceptPerBatch ?? documents.Count).ToArray();
            Written.AddRange(accepted);

            return Task.FromResult(accepted.Length);
        }

        public Task DeleteAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }
}
