using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Elasticsearch;

namespace Bfs.Iop.Core.Elasticsearch.UnitTests;

/// <summary>
/// Tests that <see cref="CatalogDocumentFactory.FromConcept"/> writes the reuse-count and
/// derived reuse-weight fields into the indexed document.
/// </summary>
[TestFixture(TestOf = typeof(CatalogDocumentFactory))]
public class CatalogDocumentFactoryFromConceptTests
{
    [TestCase(0)]
    [TestCase(3)]
    [TestCase(42)]
    public void FromConcept_SetsReuseCountAndDerivedReuseWeight(int reuseCount)
    {
        var model = BuildConcept();

        var (_, document) = CatalogDocumentFactory.FromConcept(model, reuseCount);

        Assert.That(document[EsCatalogFields.ReuseCount], Is.EqualTo(reuseCount));
        Assert.That(document[EsCatalogFields.ReuseWeight], Is.EqualTo(CatalogDocumentFactory.ReuseCountToWeight(reuseCount)));
    }

    private static IopConceptModel BuildConcept() => new()
    {
        Id = Guid.NewGuid(),
        Identifiers = ["concept-1"],
        Version = "1",
        Name = new MultiLanguageModel { De = "Konzept" },
        Description = new MultiLanguageModel { De = "Beschreibung" },
        ResponsiblePerson = null,
        PublicationLevel = PublicationLevel.Public,
        RegistrationStatus = RegistrationStatus.Recorded,
        System = new SystemInfoModel { CreatedAt = DateTimeOffset.UtcNow },
        Publisher = new AgentModel
        {
            Id = Guid.NewGuid(),
            Identifier = "publisher-1",
            Name = new MultiLanguageModel { De = "Publisher" },
            PrefLabel = new MultiLanguageModel { De = "Publisher" },
            System = new SystemInfoModel { CreatedAt = DateTimeOffset.UtcNow },
        },
    };
}
