using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Models.Search.Filters;
using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.Infrastructure.Security.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Bfs.Iop.Core.Lucene.UnitTests;

/// <summary>
///     Tests for the filter-count (facet) behavior of <see cref="CatalogIndexService.SearchCount"/>.
///     Selecting a value inside a filter category must not collapse the other values of that same
///     category to zero (OR within a category), while still constraining the other categories
///     (AND across categories) — the "drill-sideways" behavior that powers the filter dropdowns.
/// </summary>
[TestFixture(TestOf = typeof(CatalogIndexService))]
public class CatalogIndexServiceSearchCountTests
{
    private const string PublisherA = "pub-a";
    private const string PublisherB = "pub-b";

    private CatalogIndexService _service = null!;

    [SetUp]
    public void SetUp()
    {
        var userContextService = Substitute.For<IUserContextService>();
        // UserHasRole(true) resolves to BusinessRole.InteroperabilityService -> no authorization filter is applied.
        userContextService.UserHasRole(Arg.Any<string>()).Returns(true);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Lucene:UseRamDirectory"] = "true" })
            .Build();

        _service = new CatalogIndexService(
            userContextService,
            NullLoggerFactory.Instance.CreateLogger<CatalogIndexService>(),
            config);

        // Two publishers, two registration statuses:
        //   Publisher A -> 2 datasets, both Recorded
        //   Publisher B -> 1 dataset, Qualified
        _service.UpdateIndex(hasStructure: false, model: BuildDataset("ds-a1", PublisherA, RegistrationStatus.Recorded));
        _service.UpdateIndex(hasStructure: false, model: BuildDataset("ds-a2", PublisherA, RegistrationStatus.Recorded));
        _service.UpdateIndex(hasStructure: false, model: BuildDataset("ds-b1", PublisherB, RegistrationStatus.Qualified));
    }

    [TearDown]
    public void TearDown() => _service.Dispose();

    [Test]
    public void SearchCount_WithPublisherFilter_KeepsOtherPublishersVisibleButConstrainsOtherCategories()
    {
        var counts = _service.SearchCount(null, null, new CatalogSearchFilter { PublisherIdentifiers = [PublisherA] }).ToList();

        var publishers = CountsFor(counts, LuceneFields.Catalog.PublisherIdentifier);
        var statuses = CountsFor(counts, LuceneFields.Catalog.RegistrationStatus);

        Assert.Multiple(() =>
        {
            // OR within the publisher category: selecting A must not hide B.
            Assert.That(publishers[PublisherA], Is.EqualTo(2));
            Assert.That(publishers[PublisherB], Is.EqualTo(1));

            // AND across categories: the status counts are still constrained to publisher A's datasets.
            Assert.That(statuses[nameof(RegistrationStatus.Recorded)], Is.EqualTo(2));
            Assert.That(statuses.ContainsKey(nameof(RegistrationStatus.Qualified)), Is.False);

            // The returned total is the true filtered result count, not the sideways publisher total.
            Assert.That(TotalOf(counts), Is.EqualTo(2));
        });
    }

    [Test]
    public void SearchCount_WithNumericCategoryFilter_KeepsOtherValuesVisible()
    {
        var counts = _service.SearchCount(null, null, new CatalogSearchFilter { RegistrationStatuses = [RegistrationStatus.Recorded] }).ToList();

        var statuses = CountsFor(counts, LuceneFields.Catalog.RegistrationStatus);
        var publishers = CountsFor(counts, LuceneFields.Catalog.PublisherIdentifier);

        Assert.Multiple(() =>
        {
            // OR within the (numeric) registration-status category: selecting Recorded must not hide Qualified.
            Assert.That(statuses[nameof(RegistrationStatus.Recorded)], Is.EqualTo(2));
            Assert.That(statuses[nameof(RegistrationStatus.Qualified)], Is.EqualTo(1));

            // AND across categories: only publisher A has Recorded datasets.
            Assert.That(publishers[PublisherA], Is.EqualTo(2));
            Assert.That(publishers.ContainsKey(PublisherB), Is.False);

            Assert.That(TotalOf(counts), Is.EqualTo(2));
        });
    }

    [Test]
    public void SearchCount_WithMultiplePublishers_ReturnsUnionTotal()
    {
        var counts = _service.SearchCount(null, null, new CatalogSearchFilter { PublisherIdentifiers = [PublisherA, PublisherB] }).ToList();

        var publishers = CountsFor(counts, LuceneFields.Catalog.PublisherIdentifier);

        Assert.Multiple(() =>
        {
            Assert.That(publishers[PublisherA], Is.EqualTo(2));
            Assert.That(publishers[PublisherB], Is.EqualTo(1));
            Assert.That(TotalOf(counts), Is.EqualTo(3));
        });
    }

    [Test]
    public void SearchCount_WithoutFilter_ReturnsAllValuesAndTotal()
    {
        var counts = _service.SearchCount(null, null, new CatalogSearchFilter()).ToList();

        var publishers = CountsFor(counts, LuceneFields.Catalog.PublisherIdentifier);

        Assert.Multiple(() =>
        {
            Assert.That(publishers[PublisherA], Is.EqualTo(2));
            Assert.That(publishers[PublisherB], Is.EqualTo(1));
            Assert.That(TotalOf(counts), Is.EqualTo(3));
        });
    }

    [Test]
    public void SearchCount_WithNoMatchingResults_ReturnsZeroTotal()
    {
        var counts = _service.SearchCount(null, null, new CatalogSearchFilter { PublisherIdentifiers = ["does-not-exist"] }).ToList();

        // No dataset matches the full filter, so the true total is zero even though the publisher
        // dimension itself still reports counts (its own filter is removed by drill-sideways).
        Assert.That(TotalOf(counts), Is.EqualTo(0));
    }

    [Test]
    public void Search_WithPublisherFilter_IsUnchanged()
    {
        // Guard: the result-list path keeps AND semantics (only publisher A's datasets match).
        var result = _service.Search(null, null, new CatalogSearchFilter { PublisherIdentifiers = [PublisherA] }, 1, 100);

        Assert.That(result.TotalCount, Is.EqualTo(2));
    }

    private static IReadOnlyDictionary<string, int> CountsFor(
        IEnumerable<Search.CatalogSearchCountResultEntry> counts,
        string dimension) =>
        counts.Single(x => x.Identifier == dimension).CountByValues;

    private static int TotalOf(IEnumerable<Search.CatalogSearchCountResultEntry> counts) =>
        counts.First().TotalDocumentsCount;

    private static DcatDatasetModel BuildDataset(string identifier, string publisherIdentifier, RegistrationStatus registrationStatus)
    {
        var name = new MultiLanguageModel { De = "Test" };

        return new DcatDatasetModel
        {
            Id = Guid.NewGuid(),
            Identifiers = [identifier],
            AccessRights = new VocabularyEntryModel { Code = "PUBLIC" },
            Title = new MultiLanguageModel { De = "Titel" },
            Description = new MultiLanguageModel { De = "Beschreibung" },
            PublicationLevel = PublicationLevel.Public,
            RegistrationStatus = registrationStatus,
            ContactPoints = [new VCardModel { HasEmail = "contact@example.org" }],
            Publisher = new AgentModel
            {
                Id = Guid.NewGuid(),
                Identifier = publisherIdentifier,
                Name = name,
                PrefLabel = name,
                System = new SystemInfoModel { CreatedAt = DateTimeOffset.UnixEpoch }
            },
            System = new SystemInfoModel { CreatedAt = DateTimeOffset.UnixEpoch }
        };
    }
}
