using System.Runtime.CompilerServices;
using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Business.Sources;
using Bfs.Iop.IndexSearch.Contracts.Indexing;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bfs.Iop.IndexSearch.Business.UnitTests;

[TestFixture]
internal sealed class CodeListIndexRebuilderTests
{
    [Test]
    public async Task A_batch_that_cannot_be_written_does_not_abandon_the_rest()
    {
        // The same guarantee as the catalog rebuilder, in its own copy of the loop: code lists are the
        // largest thing indexed, so giving up on the first bad batch costs the most here.
        var writer = new RecordingWriter { FailOnCall = 2 };

        var report = await new CodeListIndexRebuilder(new StubSource(), writer, NullLogger<CodeListIndexRebuilder>.Instance)
            .RebuildAsync(batchSize: 1);

        report.BatchesFailed.Should().Be(1);
        report.DocumentsWritten.Should().Be(2);
        writer.Written.Select(x => x.Code).Should().Equal("01", "03");
    }

    private sealed class StubSource : ICodeListDocumentSource
    {
        public async IAsyncEnumerable<IReadOnlyList<CodeListIndexDocument>> ReadAllAsync(
            int batchSize,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            CodeListIndexDocument[] documents =
            [
                new() { Id = Guid.NewGuid(), ConceptId = Guid.NewGuid(), Code = "01" },
                new() { Id = Guid.NewGuid(), ConceptId = Guid.NewGuid(), Code = "02" },
                new() { Id = Guid.NewGuid(), ConceptId = Guid.NewGuid(), Code = "03" },
            ];

            foreach (var chunk in documents.Chunk(batchSize))
            {
                yield return chunk;
            }

            await Task.CompletedTask;
        }
    }

    private sealed class RecordingWriter : ICodeListIndexWriter
    {
        private int _calls;

        public int? FailOnCall { get; init; }

        public List<CodeListIndexDocument> Written { get; } = [];

        public Task<int> WriteAsync(
            IReadOnlyCollection<CodeListIndexDocument> documents,
            CancellationToken cancellationToken = default)
        {
            _calls++;

            if (_calls == FailOnCall)
            {
                throw new InvalidOperationException("The index rejected the batch.");
            }

            Written.AddRange(documents);

            return Task.FromResult(documents.Count);
        }

        public Task DeleteAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }
}
