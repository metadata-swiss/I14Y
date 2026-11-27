using System.Text.Json;
using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Elasticsearch;

namespace Bfs.Iop.IndexSearch.Elasticsearch.UnitTests;

[TestFixture]
public class CatalogIndexMappingCoverageTests
{
    private static HashSet<string> MappedFields()
    {
        using var doc = JsonDocument.Parse(CatalogIndexMapping.BuildCreateIndexJson());

        return [.. doc.RootElement
            .GetProperty("mappings")
            .GetProperty("properties")
            .EnumerateObject()
            .Select(x => x.Name)];
    }

    [Test]
    public void Every_declared_field_name_is_mapped()
    {
        var declared = typeof(EsCatalogFields)
            .GetFields()
            .Where(x => x.IsLiteral && x.FieldType == typeof(string))
            .Select(x => (string)x.GetRawConstantValue()!)
            .ToArray();

        MappedFields().Should().Contain(declared);
    }

    [TestCase(EsCatalogFields.Tenant)]
    [TestCase(EsCatalogFields.TenantIdentifier)]
    [TestCase(EsCatalogFields.TenantIdentifierLabel)]
    [TestCase(EsCatalogFields.PublisherName)]
    [TestCase(EsCatalogFields.PublisherKey)]
    [TestCase(EsCatalogFields.PublisherKeyLabel)]
    public void Existing_index_gets_the_field_mapped_as_on_a_new_index(string field)
    {
        using var created = JsonDocument.Parse(CatalogIndexMapping.BuildCreateIndexJson());
        using var added = JsonDocument.Parse(CatalogIndexMapping.BuildAddedFieldsMappingJson());

        var createdField = created.RootElement.GetProperty("mappings").GetProperty("properties").GetProperty(field);
        var addedField = added.RootElement.GetProperty("properties").GetProperty(field);

        addedField.GetRawText().Should().Be(createdField.GetRawText());
    }
}
