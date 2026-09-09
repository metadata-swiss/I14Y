using AwesomeAssertions;
using Bfs.Iop.Core.LinkedData.Services;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Bfs.Iop.IndexSearch.Data.UnitTests;

[TestFixture]
internal sealed class DatasetStructureSourceTests
{
    private static readonly Guid _id = Guid.Parse("44444444-4444-4444-4444-444444444444");

    [TestCase("44444444-4444-4444-4444-444444444444.ttl", TestName = "file name")]
    [TestCase("44444444-4444-4444-4444-444444444444", TestName = "bare id")]
    public async Task Both_shapes_the_two_implementations_return_are_understood(string value)
    {
        // The file-backed service returns "{guid}.ttl" and the triple store returns the bare id. A
        // caller cannot tell which it is talking to, so the port normalises both.
        var ids = await CreateSource([value]).GetIdsWithStructuresAsync();

        ids.Should().Equal([_id]);
    }

    [Test]
    public async Task A_value_that_is_not_a_dataset_id_is_skipped_rather_than_failing_the_rebuild()
    {
        var ids = await CreateSource(["not-a-guid.ttl", "44444444-4444-4444-4444-444444444444.ttl"]).GetIdsWithStructuresAsync();

        ids.Should().Equal([_id]);
    }

    [Test]
    public async Task An_unreadable_store_returns_null_rather_than_an_empty_set()
    {
        var service = Substitute.For<IDatasetModelProcessService>();
        service.GetAllDatasetIdsWithStructures(Arg.Any<CancellationToken>())
            .Returns<Task<IEnumerable<string>>>(_ => throw new InvalidOperationException("The store is unreachable."));

        var ids = await new DatasetStructureSource(service, NullLogger<DatasetStructureSource>.Instance)
            .GetIdsWithStructuresAsync();

        // Empty would read as "no dataset has a structure" and clear the flag on all of them.
        ids.Should().BeNull();
    }

    private static DatasetStructureSource CreateSource(IEnumerable<string> values)
    {
        var service = Substitute.For<IDatasetModelProcessService>();
        service.GetAllDatasetIdsWithStructures(Arg.Any<CancellationToken>()).Returns(Task.FromResult(values));

        return new DatasetStructureSource(service, NullLogger<DatasetStructureSource>.Instance);
    }
}
