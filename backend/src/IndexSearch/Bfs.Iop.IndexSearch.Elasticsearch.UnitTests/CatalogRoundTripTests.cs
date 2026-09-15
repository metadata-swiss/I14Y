using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Contracts;
using Bfs.Iop.IndexSearch.Contracts.Indexing;
using Bfs.Iop.IndexSearch.Contracts.Search;
using Bfs.Iop.IndexSearch.Elasticsearch;

using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.IndexSearch.Elasticsearch.UnitTests;

// CatalogDocumentFactory and CatalogResponseReader are only correct together. Comparing either against
// hand-written JSON proves nothing about the pair, so this puts a document through a real server and
// reads it back: whatever the two disagree about shows up as a field that changed on the way round.
[TestFixture]
[Explicit("Needs a live Elasticsearch. docker compose up -d elasticsearch")]
[Category("Integration")]
public class CatalogRoundTripTests
{
    private const string Index = "i14y-catalog-roundtriptest";

    private static readonly string[] German = ["de"];

    private static readonly CatalogIndexDocument Original = new()
    {
        Id = Guid.Parse("7f1c9d2e-4b3a-4c5d-8e6f-0a1b2c3d4e5f"),
        Type = SearchResourceType.Concept,
        Identifiers = ["BFS-STAT-2024", "BFS-STAT-OLD"],
        PublisherId = Guid.Parse("11112222-3333-4444-5555-666677778888"),
        PublisherIdentifier = "CH_BFS",
        PublicationLevel = PublicationLevel.Public,
        PublicationLevelProposal = PublicationLevel.Internal,
        RegistrationStatus = RegistrationStatus.Standard,
        RegistrationStatusProposal = RegistrationStatus.PreferredStandard,
        CreatedAt = new DateTimeOffset(2024, 6, 1, 12, 0, 0, TimeSpan.Zero),
        ModifiedAt = new DateTimeOffset(2025, 6, 1, 12, 0, 0, TimeSpan.Zero),
        CreationType = CreationType.Automated,
        Title = new MultiLanguageModel { De = "Titel", Fr = "Titre" },
        Name = new MultiLanguageModel { De = "Name", It = "Nome" },
        Description = new MultiLanguageModel { De = "Beschreibung" },
        Keywords = [new MultiLanguageModel { De = "Schlagwort" }],
        Version = "1.2.0",
        DataOwner = "Bundesamt für Statistik",
        AccessRights = "PUBLIC",
        Themes = ["ENER", "SOCI"],
        Formats = ["CSV", "JSON"],
        BusinessEvents = ["BE-1"],
        LifeEvents = ["LE-1"],
        HasStructure = true,
        ConceptType = ConceptType.CodeList,
        ValidFrom = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
        ValidTo = new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero),
        ResponsiblePerson = new IndexPerson { GivenName = "Ada", FamilyName = "Lovelace", Email = "ada@example.ch" },
        ResponsibleDeputy = new IndexPerson { GivenName = "Alan", FamilyName = "Turing", Email = "alan@example.ch" },
        ContactPoints = [new IndexContactPoint { HasEmail = "kontakt@example.ch", HasTelephone = "+41 58 000 00 00" }],
        ChannelEmails = ["kanal@example.ch"],
    };

    private HttpClient _client = null!;
    private CatalogSearchHit _hit = null!;
    private CatalogFacetCounts _facets = null!;

    [OneTimeSetUp]
    public async Task RoundTripAsync()
    {
        var uri = Environment.GetEnvironmentVariable("INDEXSEARCH_TEST_ELASTICSEARCH") ?? "http://localhost:9200";

        if (new Uri(uri).Host is not ("localhost" or "127.0.0.1" or "[::1]"))
        {
            Assert.Fail("Refusing to run: this fixture deletes indices. Use a local node.");
        }

        _client = new HttpClient { BaseAddress = new Uri(uri), Timeout = TimeSpan.FromSeconds(30) };

        await _client.DeleteAsync($"/{Index}");
        await _client.PutAsync(
            $"/{Index}",
            new StringContent(CatalogIndexMapping.BuildCreateIndexJson(), Encoding.UTF8, "application/json"));

        var (id, document) = CatalogDocumentFactory.Build(Original);

        await _client.PutAsync($"/{Index}/_doc/{id}?refresh=true", JsonContent.Create(document));

        var search = await PostAsync(CatalogQueryBuilder.BuildSearchBody(
            null, German, null, SearchCaller.Anonymous, from: 0, size: 10));

        _hit = CatalogResponseReader.ReadSearch(search, page: 1, pageSize: 10).Results.Single();

        var count = await PostAsync(CatalogQueryBuilder.BuildCountBody(
            null, German, null, SearchCaller.Anonymous));

        _facets = CatalogResponseReader.ReadFacets(count);
    }

    [OneTimeTearDown]
    public void Dispose() => _client?.Dispose();

    [Test]
    public void Identity_and_classification_survive_the_round_trip()
    {
        _hit.Id.Should().Be(Original.Id);
        _hit.Type.Should().Be(Original.Type);
        _hit.Identifiers.Should().Equal(Original.Identifiers);
        _hit.PublisherId.Should().Be(Original.PublisherId);
        _hit.ConceptType.Should().Be(Original.ConceptType);
    }

    [Test]
    public void Every_enum_survives_the_round_trip()
    {
        // Written as names and parsed back by name. A value that failed to parse would silently become
        // the first enum member, which is why each is checked rather than just one.
        _hit.PublicationLevel.Should().Be(Original.PublicationLevel);
        _hit.PublicationLevelProposal.Should().Be(Original.PublicationLevelProposal);
        _hit.RegistrationStatus.Should().Be(Original.RegistrationStatus);
        _hit.RegistrationStatusProposal.Should().Be(Original.RegistrationStatusProposal);
        _hit.CreationType.Should().Be(Original.CreationType);
    }

    [Test]
    public void Every_date_survives_the_round_trip()
    {
        _hit.CreatedAt.Should().Be(Original.CreatedAt);
        _hit.ModifiedAt.Should().Be(Original.ModifiedAt);
        _hit.ValidFrom.Should().Be(Original.ValidFrom);
        _hit.ValidTo.Should().Be(Original.ValidTo);
    }

    [Test]
    public void Multilingual_text_survives_the_round_trip_in_every_language_it_had()
    {
        _hit.Title!.De.Should().Be("Titel");
        _hit.Title!.Fr.Should().Be("Titre");
        _hit.Title!.En.Should().BeNull("a language that was never set must not appear");

        _hit.Name!.De.Should().Be("Name");
        _hit.Name!.It.Should().Be("Nome");
        _hit.Description!.De.Should().Be("Beschreibung");
    }

    [Test]
    public void Vocabulary_codes_and_the_structure_flag_survive_the_round_trip()
    {
        _hit.Themes.Should().BeEquivalentTo(Original.Themes);
        _hit.Formats.Should().BeEquivalentTo(Original.Formats);
        _hit.BusinessEvents.Should().BeEquivalentTo(Original.BusinessEvents);
        _hit.LifeEvents.Should().BeEquivalentTo(Original.LifeEvents);
        _hit.AccessRights.Should().Be(Original.AccessRights);
        _hit.Version.Should().Be(Original.Version);
        _hit.HasStructure.Should().Be(Original.HasStructure);
    }

    [Test]
    public void Every_facet_the_document_belongs_to_reports_it()
    {
        _facets.TotalCount.Should().Be(1);

        _facets.Themes.Should().Contain(new KeyValuePair<string, int>("ENER", 1));
        _facets.Types.Should().Contain(new KeyValuePair<string, int>("Concept", 1));
        _facets.AccessRights.Should().Contain(new KeyValuePair<string, int>("PUBLIC", 1));
        _facets.Formats.Should().Contain(new KeyValuePair<string, int>("CSV", 1));
        _facets.RegistrationStatuses.Should().Contain(new KeyValuePair<string, int>("Standard", 1));
        _facets.ConceptTypes.Should().Contain(new KeyValuePair<string, int>("CodeList", 1));

        // The publisher facet keys on the label, so it keeps the original casing rather than the
        // lowercased copy the filters match.
        _facets.Publishers.Should().Contain(new KeyValuePair<string, int>("CH_BFS", 1));

        // A terms aggregation on a boolean keys its buckets 0 and 1, so this only reads as "True" if
        // the reader converts them.
        _facets.Structures.Should().Contain(new KeyValuePair<string, int>("True", 1));
    }

    [Test]
    public async Task A_multi_valued_field_holding_a_single_bare_string_still_reads_as_a_list()
    {
        // Our factory always writes these as arrays, so this shape never comes from us. Elasticsearch
        // accepts either for a keyword field though, so a document written by hand or by an older
        // version can hold a bare string — and reading that as "no values" would drop it silently.
        var id = Guid.Parse("0e0e0e0e-0e0e-0e0e-0e0e-0e0e0e0e0e0e");

        var raw = $$"""
            {
              "id": "{{id}}",
              "type": "Dataset",
              "publicationLevel": "Public",
              "registrationStatus": "Standard",
              "creationType": "Manual",
              "identifier": "ONLY-ONE",
              "themes": "ENER"
            }
            """;

        await _client.PutAsync(
            $"/{Index}/_doc/{id}?refresh=true",
            new StringContent(raw, Encoding.UTF8, "application/json"));

        var search = await PostAsync(CatalogQueryBuilder.BuildSearchBody(
            "ONLY-ONE", German, null, SearchCaller.Anonymous, from: 0, size: 10));

        var hit = CatalogResponseReader.ReadSearch(search, page: 1, pageSize: 10).Results.Single();

        hit.Identifiers.Should().Equal("ONLY-ONE");
        hit.Themes.Should().Equal("ENER");
    }
    private async Task<JsonElement> PostAsync(Dictionary<string, object?> body)
    {
        var response = await _client.PostAsync($"/{Index}/_search", JsonContent.Create(body));

        if (!response.IsSuccessStatusCode)
        {
            Assert.Fail($"search failed with {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
        }

        return JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement.Clone();
    }
}
