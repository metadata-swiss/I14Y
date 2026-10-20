using AwesomeAssertions;
using Bfs.Iop.Core.LinkedData.DataObjects;
using Bfs.Iop.Core.LinkedData.Services;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Bfs.Iop.IndexSearch.Data.UnitTests;

/// <summary>
///     Joins the counts the database can work out to the ones only the triple store knows.
/// </summary>
[TestFixture(TestOf = typeof(ResourceReuseSource))]
internal sealed class ResourceReuseSourceTests
{
    private static readonly Guid _concept = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid _publicDataset = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid _internalDataset = Guid.Parse("33333333-3333-3333-3333-333333333333");

    [Test]
    public async Task A_concept_adds_the_structures_conforming_to_it_to_the_count_the_database_gave()
    {
        var counts = await Create(
            fromDatabase: new Dictionary<Guid, int> { [_concept] = 2 },
            structureReferences: new Dictionary<Guid, IReadOnlyList<Guid>> { [_concept] = [_publicDataset] })
            .GetReuseCountsAsync();

        // Mapping tables and dataset structures are both reuse, and they are counted in different
        // stores, so neither may replace the other.
        counts!.GetValueOrDefault(_concept).Should().Be(3);
    }

    [Test]
    public async Task A_structure_in_a_dataset_the_public_cannot_see_is_not_counted()
    {
        var counts = await Create(
            fromDatabase: new Dictionary<Guid, int>(),
            structureReferences: new Dictionary<Guid, IReadOnlyList<Guid>>
            {
                [_concept] = [_publicDataset, _internalDataset],
            })
            .GetReuseCountsAsync();

        // The triple store records references without knowing publication levels, so the filtering
        // has to happen here or an anonymous caller could infer that the internal dataset exists.
        counts!.GetValueOrDefault(_concept).Should().Be(1);
    }

    [Test]
    public async Task Every_attribute_built_on_a_concept_counts_even_within_one_dataset()
    {
        var counts = await Create(
            fromDatabase: new Dictionary<Guid, int>(),
            structureReferences: new Dictionary<Guid, IReadOnlyList<Guid>>
            {
                // One row per referencing property, so a dataset with three attributes built on the
                // concept comes back three times.
                [_concept] = [_publicDataset, _publicDataset, _publicDataset],
            })
            .GetReuseCountsAsync();

        // The relations endpoint counts these rows the same way. Collapsing them to one dataset
        // would rank a concept the catalogue shows as reused ten times below one it shows as two.
        counts!.GetValueOrDefault(_concept).Should().Be(3);
    }

    [Test]
    public async Task Every_concept_is_asked_about_even_when_there_are_more_than_one_query_holds()
    {
        var concepts = Enumerable.Range(0, 1201).Select(_ => Guid.NewGuid()).ToArray();

        var structures = Substitute.For<IDatasetModelProcessService>();
        structures
            .GetConceptStructureReferencesBatch(Arg.Any<IEnumerable<IopConceptData>>(), Arg.Any<CancellationToken>())
            .Returns(call => Task.FromResult<IReadOnlyDictionary<Guid, IReadOnlyList<Guid>>>(
                call.Arg<IEnumerable<IopConceptData>>()
                    .ToDictionary(x => x.Id, _ => (IReadOnlyList<Guid>)new[] { _publicDataset })));

        var counts = await new ResourceReuseSource(
            Provider(new Dictionary<Guid, int>(), concepts),
            structures,
            NullLogger<ResourceReuseSource>.Instance)
            .GetReuseCountsAsync();

        // Chunking is what keeps the query string bounded; dropping the last partial chunk would
        // silently under-count whichever concepts happened to land in it.
        counts!.Should().HaveCount(concepts.Length);
    }

    [Test]
    public async Task A_database_that_cannot_be_read_yields_nothing_rather_than_half_the_counts()
    {
        var provider = Substitute.For<ISearchIndexProviderService>();
        provider.GetPublicReuseCounts(Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyDictionary<Guid, int>>>(_ => throw new InvalidOperationException("no connection"));

        var counts = await new ResourceReuseSource(
            provider,
            Substitute.For<IDatasetModelProcessService>(),
            NullLogger<ResourceReuseSource>.Instance)
            .GetReuseCountsAsync();

        counts.Should().BeNull();
    }

    [Test]
    public async Task A_triple_store_that_cannot_be_read_yields_nothing_rather_than_the_database_half()
    {
        var structures = Substitute.For<IDatasetModelProcessService>();
        structures
            .GetConceptStructureReferencesBatch(Arg.Any<IEnumerable<IopConceptData>>(), Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyDictionary<Guid, IReadOnlyList<Guid>>>>(_ => throw new InvalidOperationException("unreachable"));

        var counts = await new ResourceReuseSource(
            Provider(new Dictionary<Guid, int> { [_concept] = 2 }, [_concept]),
            structures,
            NullLogger<ResourceReuseSource>.Instance)
            .GetReuseCountsAsync();

        // Returning the database half alone would rank a widely reused concept below one that is
        // only in a mapping table, which is the opposite of what the field is for.
        counts.Should().BeNull();
    }

    private static ResourceReuseSource Create(
        IReadOnlyDictionary<Guid, int> fromDatabase,
        IReadOnlyDictionary<Guid, IReadOnlyList<Guid>> structureReferences)
    {
        var structures = Substitute.For<IDatasetModelProcessService>();
        structures
            .GetConceptStructureReferencesBatch(Arg.Any<IEnumerable<IopConceptData>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(structureReferences));

        return new ResourceReuseSource(
            Provider(fromDatabase, [.. structureReferences.Keys]),
            structures,
            NullLogger<ResourceReuseSource>.Instance);
    }

    private static ISearchIndexProviderService Provider(
        IReadOnlyDictionary<Guid, int> counts,
        IReadOnlyList<Guid> conceptIds)
    {
        var provider = Substitute.For<ISearchIndexProviderService>();

        provider.GetPublicReuseCounts(Arg.Any<CancellationToken>()).Returns(counts);

        provider.GetConceptIdentities(Arg.Any<CancellationToken>()).Returns(
            [.. conceptIds.Select(x => new ConceptIdentityModel(x, $"c-{x:N}", "1.0.0"))]);

        provider.GetPublicDatasetIds(Arg.Any<CancellationToken>())
            .Returns<IReadOnlySet<Guid>>(new HashSet<Guid> { _publicDataset });

        return provider;
    }
}
