using AwesomeAssertions;
using Bfs.Iop.Core.Mappings;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using NSubstitute;

using CatalogSearchHit = Bfs.Iop.IndexSearch.Contracts.Search.CatalogSearchHit;

namespace Bfs.Iop.Core.UnitTests.Mappings;

[TestFixture(TestOf = typeof(CatalogSearchHitMappingExtensions))]
internal sealed class CatalogSearchHitMappingExtensionsTests
{
    private static readonly AgentModel Tenant = new()
    {
        Id = Guid.NewGuid(),
        Identifier = "CH_BFS",
        Name = new MultiLanguageModel { De = "Bundesamt für Statistik" },
        PrefLabel = new MultiLanguageModel(),
        System = new SystemInfoModel(),
    };

    private static CatalogSearchHit Hit() => new()
    {
        Id = Guid.NewGuid(),
        Type = SearchResourceType.Dataset,
        Identifiers = ["DS-1"],
        TenantId = Tenant.Id,
        Title = new MultiLanguageModel { De = "Titel" },
        Description = new MultiLanguageModel { De = "Beschreibung" },
    };

    private static readonly IVocabulariesService Vocabularies = Substitute.For<IVocabulariesService>();

    [Test]
    public void The_result_carries_the_tenant_and_the_indexed_publisher()
    {
        var hit = Hit() with
        {
            PublisherIdentifier = "CH_ESTV",
            PublisherName = new MultiLanguageModel { De = "Steuerverwaltung" },
        };

        var result = hit.MapToSearchResultModel(Tenant, Vocabularies);

        result.Tenant.Should().BeSameAs(Tenant);
        result.Publisher.Identifier.Should().Be("CH_ESTV");
        result.Publisher.Name!.De.Should().Be("Steuerverwaltung");
    }

    [Test]
    public void A_publisher_with_only_a_name_is_returned_without_an_identifier()
    {
        var hit = Hit() with { PublisherName = new MultiLanguageModel { De = "Externe Stelle" } };

        var result = hit.MapToSearchResultModel(Tenant, Vocabularies);

        result.Publisher.Identifier.Should().BeNull();
        result.Publisher.Name!.De.Should().Be("Externe Stelle");
    }

    [Test]
    public void A_hit_without_publisher_details_still_has_a_publisher()
    {
        var publisher = Hit().MapToSearchResultModel(Tenant, Vocabularies).Publisher;

        publisher.Should().NotBeNull();
        publisher.Identifier.Should().BeNull();
        publisher.Name.Should().BeNull();
    }
}
