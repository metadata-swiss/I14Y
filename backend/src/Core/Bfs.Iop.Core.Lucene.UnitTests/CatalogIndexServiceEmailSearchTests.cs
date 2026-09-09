using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.Infrastructure.Security.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Bfs.Iop.Core.Lucene.UnitTests;

/// <summary>
///     Regression tests: searching by email address (used by the "My Data" dashboard) must match
///     only objects that actually reference that email, not every object whose free-text fields happen to share a
///     token of the address (e.g. "user" from "test.user@...").
/// </summary>
[TestFixture(TestOf = typeof(CatalogIndexService))]
public class CatalogIndexServiceEmailSearchTests
{
    private const string UserEmail = "test.user@example.org";

    private CatalogIndexService _service = null!;

    [SetUp]
    public void SetUp()
    {
        var userContextService = Substitute.For<IUserContextService>();
        // Returns InteroperabilityService -> no authorization filter is applied to the search.
        userContextService.UserHasRole(Arg.Any<string>()).Returns(true);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Lucene:UseRamDirectory"] = "true" })
            .Build();

        _service = new CatalogIndexService(
            userContextService,
            NullLoggerFactory.Instance.CreateLogger<CatalogIndexService>(),
            config);

        // Matching dataset: the searched email is its contact point.
        _service.UpdateIndex(hasStructure: false, model: BuildDataset(
            identifier: "dataset-contact",
            titleDe: "Beschäftigungsstatistik",
            contactEmail: UserEmail));

        // Noise dataset: its title shares the "user" token of the email local part, and it has a different contact
        // email. Must NOT match the email search (it would under the old tokenizing behavior).
        _service.UpdateIndex(hasStructure: false, model: BuildDataset(
            identifier: "dataset-noise",
            titleDe: "User Portal",
            contactEmail: "other.contact@sample.test"));

        // Matching dataset: the searched email is its responsible person.
        _service.UpdateIndex(hasStructure: false, model: BuildDataset(
            identifier: "dataset-responsible",
            titleDe: "Steuerregister",
            contactEmail: "contact@sample.test",
            responsiblePersonEmail: UserEmail));
    }

    [TearDown]
    public void TearDown() => _service.Dispose();

    [Test]
    public void Search_ByEmail_MatchesContactPointAndResponsiblePersonOnly()
    {
        var result = _service.Search(UserEmail, null, null, 1, 100);

        var identifiers = result.Results.Select(x => x.Identifier).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(result.TotalCount, Is.EqualTo(2));
            Assert.That(identifiers, Does.Contain("dataset-contact"));
            Assert.That(identifiers, Does.Contain("dataset-responsible"));
            // The "admin" token from the domain must not pull in the noise dataset.
            Assert.That(identifiers, Does.Not.Contain("dataset-noise"));
        });
    }

    [Test]
    public void Search_ByQuotedEmail_BehavesLikeUnquotedEmail()
    {
        var result = _service.Search($"\"{UserEmail}\"", null, null, 1, 100);

        Assert.That(result.TotalCount, Is.EqualTo(2));
    }

    [Test]
    public void Search_ByNonEmailTerm_StillMatchesFreeText()
    {
        var result = _service.Search("portal", "de", null, 1, 100);

        var identifiers = result.Results.Select(x => x.Identifier).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(identifiers, Does.Contain("dataset-noise"));
            Assert.That(identifiers, Does.Not.Contain("dataset-contact"));
        });
    }

    private static DcatDatasetModel BuildDataset(
        string identifier,
        string titleDe,
        string contactEmail,
        string? responsiblePersonEmail = null)
    {
        var name = new MultiLanguageModel { De = "Test" };

        return new DcatDatasetModel
        {
            Id = Guid.NewGuid(),
            Identifiers = [identifier],
            AccessRights = new VocabularyEntryModel { Code = "PUBLIC" },
            Title = new MultiLanguageModel { De = titleDe },
            Description = new MultiLanguageModel { De = "Beschreibung" },
            PublicationLevel = PublicationLevel.Public,
            RegistrationStatus = RegistrationStatus.Recorded,
            ContactPoints = [new VCardModel { HasEmail = contactEmail }],
            ResponsiblePerson = responsiblePersonEmail is null
                ? null
                : new IopPersonModel { GivenName = "Test", FamilyName = "Person", Email = responsiblePersonEmail },
            Publisher = new AgentModel
            {
                Id = Guid.NewGuid(),
                Identifier = "ch.test",
                Name = name,
                PrefLabel = name,
                System = new SystemInfoModel { CreatedAt = DateTimeOffset.UnixEpoch }
            },
            System = new SystemInfoModel { CreatedAt = DateTimeOffset.UnixEpoch }
        };
    }
}
