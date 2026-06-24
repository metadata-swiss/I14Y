using Lucene.Net.Documents;
using Lucene.Net.Facet;

namespace Bfs.Iop.Core.Lucene.Index.Extensions;

internal static class DocumentExtensions
{
    public static void AddStoredAndFacetField(this Document document, string fieldName, string fieldValue)
    {
        ArgumentNullException.ThrowIfNull(document, nameof(document));
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldName, nameof(fieldName));

        if (string.IsNullOrWhiteSpace(fieldValue))
        {
            // Null and empty values cannot be used in a FacetField
            return;
        }

        document.Add(new FacetField(fieldName, fieldValue));
        document.Add(new StoredField(fieldName, fieldValue));
    }

    public static void AddStoredAndFacetField(this Document document, string fieldName, IEnumerable<string> fieldValues)
    {
        ArgumentNullException.ThrowIfNull(document, nameof(document));
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldName, nameof(fieldName));
        ArgumentNullException.ThrowIfNull(fieldValues, nameof(fieldValues));

        foreach (var item in fieldValues)
        {
            document.AddStoredAndFacetField(fieldName, item);
        }
    }
}
