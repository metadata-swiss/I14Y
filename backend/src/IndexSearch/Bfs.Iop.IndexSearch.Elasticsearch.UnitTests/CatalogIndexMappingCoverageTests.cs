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
}
