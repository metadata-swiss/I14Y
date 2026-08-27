using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Models.Indexing;
using NSubstitute;

namespace Bfs.Iop.Core.IndexForwarding.UnitTests;

/// <summary>
/// Each write overload projects its model with the right SearchResourceType, which is what the
/// document factory keys the resource kind on.
/// <para>
/// Get one wrong — a copy-paste slip between two near-identical overloads — and a dataset is indexed
/// as a concept. Nothing fails: the enqueue succeeds, the POST succeeds, and the dataset simply never
/// appears under its own type filter. Nothing surfaces until a user reports missing results.
/// </para>
/// <para>
/// The overloads differ only by parameter type, so the compiler will not catch a mismatched Type
/// either. That is what makes this worth asserting rather than reading. The queue target used to
/// carry this distinction; it now travels on the entry, which is why these assert the entry.
/// </para>
/// </summary>
[TestFixture]
public sealed class ForwardingIndexWriterTargetTests
{
    private static readonly SystemInfoModel System = new() { CreatedAt = DateTimeOffset.UnixEpoch };
    private static readonly MultiLanguageModel Text = new() { De = "text" };

    private static AgentModel Publisher() => new()
    {
        Id = Guid.NewGuid(),
        Identifier = "CH_BFS",
        Name = Text,
        PrefLabel = Text,
        System = System,
    };

    private IIndexSearchDispatcher _dispatcher = null!;
    private ForwardingIndexWriter _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _dispatcher = Substitute.For<IIndexSearchDispatcher>();
        _sut = new ForwardingIndexWriter(_dispatcher, NullLogger<ForwardingIndexWriter>.Instance);
    }

    private void ShouldHaveForwarded(SearchResourceType expected) =>
        _dispatcher.Received(1).Enqueue(Arg.Is<IndexForwardItem>(
            x => x.Target == IndexForwardTarget.Catalog && x.Model != null && ((CatalogIndexEntry)x.Model).Type == expected));

    [Test]
    public async Task SingleDataset_GoesToDatasets()
    {
        await _sut.UpdateIndexAsync(new DcatDatasetModel { System = System, Publisher = Publisher() }, hasStructure: true);

        ShouldHaveForwarded(SearchResourceType.Dataset);
    }

    // DatasetCollection_GoesToDatasets stood here, covering
    // UpdateIndexAsync(IEnumerable<DcatDatasetModel>, IEnumerable<string>). Core never called that
    // overload — only the full rebuild inside the IndexSearch service does — and Core's forwarder
    // silently discarded its second argument. It is not part of ICatalogIndexWriter, so the test has
    // no subject. The engine still implements it, exercised through ICatalogIndexService.

    [Test]
    public async Task PublicServices_GoToPublicServices()
    {
        await _sut.UpdateIndexAsync([new PublicServiceModel
        {
            Name = Text, Description = Text, Publisher = Publisher(), System = System,
        }]);

        ShouldHaveForwarded(SearchResourceType.PublicService);
    }

    [Test]
    public async Task DataServices_GoToDataServices()
    {
        await _sut.UpdateIndexAsync([new DataServiceModel { System = System, Publisher = Publisher() }]);

        ShouldHaveForwarded(SearchResourceType.DataService);
    }

    [Test]
    public async Task Concepts_GoToConcepts()
    {
        await _sut.UpdateIndexAsync([new IopConceptModel
        {
            Name = Text, Description = Text, Version = "1.0.0",
            Publisher = Publisher(), ResponsiblePerson = null, System = System,
        }]);

        ShouldHaveForwarded(SearchResourceType.Concept);
    }

    [Test]
    public async Task MappingTables_GoToMappingTables()
    {
        await _sut.UpdateIndexAsync([new MappingTableModel
        {
            Name = Text, Description = Text, Version = "1.0.0",
            Publisher = Publisher(), ResponsiblePerson = null, System = System,
            Source = new MappingTableUriModel { Uri = "urn:source" },
            Target = new MappingTableUriModel { Uri = "urn:target" },
        }]);

        ShouldHaveForwarded(SearchResourceType.MappingTable);
    }

    /// <summary>
    /// Every overload must land on a DIFFERENT target. Asserting each one individually would still
    /// pass if two overloads shared a target by mistake, so pin the set as a whole.
    /// </summary>
    [Test]
    public async Task TheFiveResourceTypesUseFiveDistinctTargets()
    {
        var seen = new List<SearchResourceType>();
        _dispatcher.When(x => x.Enqueue(Arg.Any<IndexForwardItem>()))
            .Do(call => seen.Add(((CatalogIndexEntry)call.Arg<IndexForwardItem>().Model!).Type));

        await _sut.UpdateIndexAsync(new DcatDatasetModel { System = System, Publisher = Publisher() });
        await _sut.UpdateIndexAsync([new PublicServiceModel
        { Name = Text, Description = Text, Publisher = Publisher(), System = System }]);
        await _sut.UpdateIndexAsync([new DataServiceModel { System = System, Publisher = Publisher() }]);
        await _sut.UpdateIndexAsync([new IopConceptModel
        { Name = Text, Description = Text, Version = "1", Publisher = Publisher(), ResponsiblePerson = null, System = System }]);
        await _sut.UpdateIndexAsync([new MappingTableModel
        { Name = Text, Description = Text, Version = "1", Publisher = Publisher(), ResponsiblePerson = null, System = System,
          Source = new MappingTableUriModel { Uri = "urn:s" }, Target = new MappingTableUriModel { Uri = "urn:t" } }]);

        seen.Should().OnlyHaveUniqueItems().And.HaveCount(5);
    }
}
