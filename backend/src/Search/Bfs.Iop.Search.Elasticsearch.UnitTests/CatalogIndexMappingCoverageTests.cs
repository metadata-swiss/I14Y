using System.Reflection;
using Bfs.Iop.Core.Abstractions.Models.Indexing;
using System.Text.Json;
using AwesomeAssertions;
using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Search.Elasticsearch.UnitTests;

/// <summary>
/// Guards the drift that Elasticsearch will not complain about: a field written into the document but
/// never declared in the index mapping.
/// <para>
/// Elasticsearch accepts such a document happily. The field is then indexed by dynamic mapping or not
/// at all, so it is silently unsearchable or un-aggregatable — no error, no log, just results that
/// quietly do not match. This is the same failure shape as the publisher-casing defect: the write
/// side and the query side disagreeing, with nothing failing loudly.
/// </para>
/// </summary>
[TestFixture(TestOf = typeof(CatalogIndexMapping))]
public class CatalogIndexMappingCoverageTests
{
    /// <summary>The field names the index actually declares, read from the real create-index body.</summary>
    private static HashSet<string> MappedFields()
    {
        using var doc = JsonDocument.Parse(CatalogIndexMapping.BuildCreateIndexJson());

        return [.. doc.RootElement
            .GetProperty("mappings")
            .GetProperty("properties")
            .EnumerateObject()
            .Select(x => x.Name)];
    }

    /// <summary>
    /// Every field-name constant must be declared. <c>EsCatalogFields</c> is the single vocabulary
    /// both the document factory and the query builder speak, so adding a constant and using it
    /// without mapping it is the realistic version of this mistake.
    /// </summary>
    [Test]
    public void EveryDeclaredFieldNameConstantIsMapped()
    {
        var constants = typeof(EsCatalogFields)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(x => x.IsLiteral && x.FieldType == typeof(string))
            .Select(x => (Name: x.Name, Value: (string)x.GetRawConstantValue()!))
            .ToList();

        constants.Should().NotBeEmpty("the reflection filter must actually find the constants");

        var mapped = MappedFields();

        var unmapped = constants.Where(x => !mapped.Contains(x.Value)).Select(x => x.Name).ToList();

        unmapped.Should().BeEmpty(
            "every field the system names must be declared in the index mapping, or it is silently " +
            "unsearchable");
    }

    /// <summary>
    /// The other direction, driven from the factory rather than the constants — this is what would
    /// catch a raw string literal used as a document key, which the constants check cannot see.
    /// <para>
    /// Covers one resource type. A field emitted only on a path not exercised here would still slip
    /// through, which is why the constants test above is the primary guard rather than this one.
    /// </para>
    /// </summary>
    [Test]
    public void EveryFieldTheFactoryEmitsIsMapped()
    {
        var system = new SystemInfoModel { CreatedAt = DateTimeOffset.UnixEpoch };

        var document = CatalogDocumentFactory.Build(new CatalogIndexEntry
        {
            Id = Guid.NewGuid(),
            Type = SearchResourceType.Concept,
            Identifier = "concept-1",
            Name = new MultiLanguageModel { De = "Bevoelkerung" },
            Title = new MultiLanguageModel { De = "Bevoelkerung" },
            Description = new MultiLanguageModel { De = "Beschreibung" },
            Version = "1.0.0",
            PublisherId = Guid.NewGuid(),
            PublisherIdentifier = "CH_BFS",
            PublicationLevel = PublicationLevel.Public,
            RegistrationStatus = RegistrationStatus.Recorded,
            CreatedAt = DateTimeOffset.UnixEpoch,
            ConceptType = ConceptType.CodeList,
        }).Document;

        var mapped = MappedFields();

        document.Keys.Should().OnlyContain(
            key => mapped.Contains(key),
            "a document key with no mapping is accepted by Elasticsearch and then silently not searchable");
    }
}
