using System.Data;
using System.Text;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Models.LinkedData;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Update;
using VDS.RDF.Writing.Formatting;

namespace Bfs.Iop.Core.LinkedData.Helpers;

internal static class ShaclSparqlQueryHelper
{
    internal const string AllowedValuesColumn = "allowedValues";
    internal const string ClassClosed = "classClosed";
    internal const string ClassIdentifier = "classIdentifier";
    internal const string ClassUriColumn = "class";
    internal const string ContentColumn = "content";
    internal const string DestinationColumn = "destination";
    internal const string MaxCountColumn = "maxCount";
    internal const string MaxLengthColumn = "maxLength";
    internal const string MinCountColumn = "minCount";
    internal const string MinLengthColumn = "minLength";
    internal const string OrderColumn = "order";
    internal const string PositionColumnX = "positionX";
    internal const string PositionColumnY = "positionY";
    internal const string PrefixI14y = "i14y";
    internal const string PropertyConformsToColumn = "propertyConformsTo";
    internal const string PropertyDataTypeColumn = "propertyDatatype";
    internal const string PropertyIdentifier = "propertyIdentifier";
    internal const string PropertyPath = "path";
    internal const string PropertyPatternColumn = "propertyPattern";
    internal const string PropertyUriColumn = "propertyUri";
    internal const string Separator = ", ";
    internal const string TargetClassColumn = "targetClass";
    internal const string UnitColumn = "Unit";
    internal const string PropertyCountColumn = "propertyCount";
    internal const string ConceptUriColumn = "conceptUri";
    internal const string DatasetIdColumn = "datasetId";
    private const string CoordXDefinition = "i14y_schema:coord_x";
    private const string CoordYDefinition = "i14y_schema:coord_y";  
    private const string StoredDefaultGraph = "urn:x-arq:DefaultGraph";

    private static readonly IReadOnlyDictionary<string, string> _sparqlQueryPrefixes = new Dictionary<string, string>
{
    { "sh", "http://www.w3.org/ns/shacl#" },
    { "rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#" },
    { "rdfs", "http://www.w3.org/2000/01/rdf-schema#" },
    { "xsd", "http://www.w3.org/2001/XMLSchema#" },
    { "qudt", "http://qudt.org/schema/qudt/" },
    { "unit", "http://qudt.org/vocab/unit/" },
    { "dcterms", "http://purl.org/dc/terms/" },
    { "i14y_schema", "http://i14y-schema/extension#" },
    { "owl", "http://www.w3.org/2002/07/owl#" },
    { "dcat", "http://www.w3.org/ns/dcat#" },
    { "schema","http://schema.org/#" }
};

    internal static string ConstructStructureQuery(Guid datasetId, bool protectedTraversal)
    {
        var whereClause = protectedTraversal
            ? BuildRecursiveStructureSelectionWhereClause(datasetId)
            : BuildSimpleStructureSelectionWhereClause(datasetId);

        return $@"
{GetAggregatedPrefixes()}
CONSTRUCT {{
    ?s ?p ?o .
}}
{whereClause}";
    }

    internal static string DeleteStructureQuery(Guid datasetId, bool protectedTraversal)
    {
        var whereClause = protectedTraversal
            ? BuildRecursiveStructureSelectionWhereClause(datasetId)
            : BuildSimpleStructureSelectionWhereClause(datasetId);

        return $@"
{GetAggregatedPrefixes()}
DELETE {{
    GRAPH <{StoredDefaultGraph}> {{
        ?s ?p ?o .
    }}
}}
{whereClause}";
    }

    internal static Uri ExpandPrefixedName(string prefixedName, string defaultPrefix)
    {
        string prefix = defaultPrefix;
        string localName = prefixedName;
        var parts = prefixedName.Split(':');

        if (parts.Length == 2)
        {
            prefix = parts[0];
            localName = parts[1];
        }

        if (!_sparqlQueryPrefixes.TryGetValue(prefix, out var namespaceUri))
        {
            namespaceUri = _sparqlQueryPrefixes[defaultPrefix];
        }

        return new Uri(namespaceUri + localName);
    }

    internal static List<ISparqlResult> GetClassAndPropertyFromGraphQuery(Graph graph)
    {
        // The whole property block is wrapped in a single outer OPTIONAL so that
        // classes without any sh:property are still returned (as one row per class
        // with the property-related columns unbound). This matches SHACL semantics:
        // a NodeShape without properties is a valid, meaningful shape.
        string queryStr = $"SELECT ?{ClassUriColumn} ?{PropertyPath} ?{PropertyUriColumn} ?{ClassClosed} ?{PositionColumnX} ?{PositionColumnY} " +
            $"?{DestinationColumn} ?{PropertyDataTypeColumn} ?{PropertyPatternColumn} ?{PropertyConformsToColumn} ?{MinCountColumn} ?{MaxCountColumn}" +
            $"?{MinLengthColumn} ?{MaxLengthColumn} ?{OrderColumn} ?{AllowedValuesColumn} ?{TargetClassColumn} ?{UnitColumn} \r\n" +
            $"WHERE {{  ?{ClassUriColumn} a sh:NodeShape .\r\n" +
            $"OPTIONAL {{ ?{ClassUriColumn} sh:closed ?{ClassClosed} . }}" +
            $"OPTIONAL {{ ?{ClassUriColumn} sh:targetClass ?{TargetClassColumn} . }}" +
            $"OPTIONAL {{ ?{ClassUriColumn} {CoordXDefinition} ?{PositionColumnX} . }}\r\n" +
            $"OPTIONAL {{ ?{ClassUriColumn} {CoordYDefinition} ?{PositionColumnY} . }}\r\n" +
            $"OPTIONAL {{\r\n" +
                $"?{ClassUriColumn} sh:property ?{PropertyUriColumn}  .\r\n" +
                $"?{PropertyUriColumn}  sh:path ?{PropertyPath} .\r\n" +
                $"OPTIONAL {{ ?{PropertyUriColumn}  sh:class ?{DestinationColumn} . }}\r\n" +
                $"OPTIONAL {{ ?{PropertyUriColumn} sh:node ?{DestinationColumn} . }}" +
                $"OPTIONAL {{ ?{PropertyUriColumn}  sh:datatype ?{PropertyDataTypeColumn} . }}\r\n" +
                $"OPTIONAL {{ ?{PropertyUriColumn}  sh:pattern ?{PropertyPatternColumn} . }}\r\n" +
                $"OPTIONAL {{ ?{PropertyUriColumn}  dcterms:conformsTo ?{PropertyConformsToColumn} . }}\r\n" +
                $"OPTIONAL {{ ?{PropertyUriColumn}  sh:minCount ?{MinCountColumn} . }}\r\n" +
                $"OPTIONAL {{ ?{PropertyUriColumn}  sh:maxCount ?{MaxCountColumn} . }}\r\n" +
                $"OPTIONAL {{ ?{PropertyUriColumn}  sh:minLength ?{MinLengthColumn} . }}\r\n" +
                $"OPTIONAL {{ ?{PropertyUriColumn}  sh:maxLength ?{MaxLengthColumn} . }}\r\n" +
                $"OPTIONAL {{ ?{PropertyUriColumn}  sh:order ?{OrderColumn} . }}\r\n" +
                $"OPTIONAL {{ ?{PropertyUriColumn}  qudt:unit ?{UnitColumn} . }}\r\n" +
                $"OPTIONAL {{ SELECT ?{PropertyUriColumn} (GROUP_CONCAT( ?val; SEPARATOR = '{Separator}') AS ?{AllowedValuesColumn})\r\n" +
                        $"WHERE {{ ?{PropertyUriColumn} sh:in ?list. \r\n ?list rdf:rest */ rdf:first ?val .}}\r\n" +
                        $"GROUP BY ?{PropertyUriColumn} }}\r\n" +
            $"}}\r\n }}" +
                    $"\r\nORDER BY ?{ClassUriColumn} ?{OrderColumn} ?{PropertyPath}"; //We order to have the same order in the frontend each time

        SparqlResultSet resultat = (SparqlResultSet)graph.ExecuteQuery(GetAggregatedPrefixes() + queryStr);
        return resultat.Results;
    }

    internal static string GetPrefixFromUri(Uri uri)
    {
        return _sparqlQueryPrefixes
            .FirstOrDefault(x => uri.AbsoluteUri.StartsWith(x.Value, StringComparison.OrdinalIgnoreCase))
            .Key;
    }

    internal static string GetStructureRootUri(string datasetIdentifier,string baseIriUrl) =>
        $"{baseIriUrl}/dataset/{datasetIdentifier}/structure";

    internal static string GetNodeShapeIriPattern(string datasetIdentifier, string baseIriUrl) =>
        $"{baseIriUrl}/dataset/{{0}}/structure/{{1}}";

    internal static string GetPropertyShapeIriPattern(string datasetIdentifier, string baseIriUrl) =>
        $"{baseIriUrl}/dataset/{{0}}/structure/{{1}}/{{2}}";

    internal static string ListStructuresQuery()
    {
        return $@"
{GetAggregatedPrefixes()}
SELECT ?datasetId
WHERE {{
    GRAPH <{StoredDefaultGraph}> {{
        ?structure schema:identifier ?datasetId .
        ?structure dcterms:hasPart ?nodeShape .
        ?nodeShape a sh:NodeShape .
        # Filter with str applied after pattern matching -> limited performance impact
        # Ensures we only consider structure roots (.../structure)
        # Prevents false positives if imported structures also use x dcterms:hasPart nodeShape
        FILTER(STRENDS(STR(?structure), '/structure'))
    }}
}}
";
    }

    internal static string StructureExistsQuery(Guid datasetId)
    {
        return $@"
{GetAggregatedPrefixes()}
ASK {{
    GRAPH <{StoredDefaultGraph}> {{
        ?root schema:identifier ""{datasetId}"" .
        ?root dcterms:hasPart ?nodeShape .
        ?nodeShape a sh:NodeShape .
    }}
}}";
    }

    internal static string SchemaPropertyExistsQuery(Guid datasetId, Uri propertyUri)
    {
        ArgumentNullException.ThrowIfNull(propertyUri, nameof(propertyUri));

        var query = new SparqlParameterizedString();
        query.CommandText = $@"
ASK {{
    GRAPH <{StoredDefaultGraph}> {{
        ?root schema:identifier ""{datasetId}"" ;
              dcterms:hasPart ?nodeShape .
        ?nodeShape a sh:NodeShape ;
                   sh:property @propertyUri .

        # Type validation: the URI must really be a SHACL PropertyShape.
        @propertyUri a sh:PropertyShape ;
                     sh:path @propertyUri .
    }}
}}";

        query.SetUri("propertyUri", propertyUri);

        return GetAggregatedPrefixes() + query.ToString();
    }

    internal static void ExecuteUpdate(Graph graph, string cmdString)
    {
        var parser = new SparqlUpdateParser();
        SparqlUpdateCommandSet cmds = parser.ParseFromString(cmdString);

        using (InMemoryDataset ds = new InMemoryDataset(graph))
        {
            //Create an Update Processor using our dataset and apply the updates
            LeviathanUpdateProcessor processor = new LeviathanUpdateProcessor(ds);
            processor.ProcessCommandSet(cmds);
        }
    }

    internal static string RequiresRecursiveStructureTraversalQuery(Guid datasetId)
    {
        return $@"
{GetAggregatedPrefixes()}
ASK {{
    GRAPH <{StoredDefaultGraph}> {{
        ?root schema:identifier ""{datasetId}"".

        # Detect structures that contain nested blank nodes below the first level
        # under a PropertyShape. If this pattern exists, the simple query is not enough
        # and we must use the recursive traversal
        ?root dcterms:hasPart ?nodeShape .
        ?nodeShape a sh:NodeShape .
        ?nodeShape sh:property ?propertyShape .

        # First-level blank node directly attached to the PropertyShape
        # Ignore dcterms:conformsTo so we do not follow external concept links
        ?propertyShape ?p1 ?b1 .
        FILTER(isBlank(?b1))
        FILTER(?p1 != dcterms:conformsTo)

        # Second-level blank node: if present, the structure is considered to need recursive traversal to get the entire structure subgraph
        ?b1 ?p2 ?b2 .
        FILTER(isBlank(?b2))
    }}
}}";
    }

    internal static string UpdateClassCoordinateQuery(
    Dictionary<string, SchemaPoint> classesPositionInput,
    Guid? datasetId = null)
    {
        var query = new StringBuilder();
        query.Append(GetAggregatedPrefixes());

        int index = 1;

        foreach (var classPosition in classesPositionInput)
        {
            if (datasetId.HasValue)
            {
                query.AppendLine($@"
DELETE {{
    GRAPH <{StoredDefaultGraph}> {{
        <{classPosition.Key}> {CoordXDefinition} ?oldX{index} ;
                              {CoordYDefinition} ?oldY{index} .
    }}
}}
INSERT {{
    GRAPH <{StoredDefaultGraph}> {{
        <{classPosition.Key}> {CoordXDefinition} {classPosition.Value.X} ;
                              {CoordYDefinition} {classPosition.Value.Y} .
    }}
}}
WHERE {{
    GRAPH <{StoredDefaultGraph}> {{
        ?root schema:identifier ""{datasetId}"" .
        ?root (!(dcterms:conformsTo))* <{classPosition.Key}> .

        OPTIONAL {{ <{classPosition.Key}> {CoordXDefinition} ?oldX{index} }}
        OPTIONAL {{ <{classPosition.Key}> {CoordYDefinition} ?oldY{index} }}
    }}
}};");
            }
            else
            {
                var deleteStringBuilder = new StringBuilder()
                    .Append($"DELETE {{\r\n  GRAPH <{StoredDefaultGraph}> {{");

                var whereStringBuilder = new StringBuilder()
                    .Append($"WHERE {{\r\n  GRAPH <{StoredDefaultGraph}> {{");

                var insertStringBuilder = new StringBuilder()
                    .Append($"INSERT DATA {{\r\n  GRAPH <{StoredDefaultGraph}> {{");

                deleteStringBuilder.Append($"\r\n    <{classPosition.Key}> {CoordXDefinition} ?oldX{index} ; {CoordYDefinition} ?oldY{index} .");
                whereStringBuilder.Append($"\r\n    <{classPosition.Key}> {CoordXDefinition} ?oldX{index} ; {CoordYDefinition} ?oldY{index} .");
                insertStringBuilder.Append($"\r\n    <{classPosition.Key}> {CoordXDefinition} {classPosition.Value.X} ; {CoordYDefinition} {classPosition.Value.Y} .");

                deleteStringBuilder.Append("\r\n  }\r\n}\r\n");
                whereStringBuilder.Append("\r\n  }\r\n};\r\n");
                insertStringBuilder.Append("\r\n  }\r\n};\r\n");

                query.Append(deleteStringBuilder)
                     .Append(whereStringBuilder)
                     .Append(insertStringBuilder);
            }

            index++;
        }

        return query.ToString();
    }

    internal static string UpdateClassQuery(
        SchemaClass classInput,
        Guid datasetId)
    {
        ArgumentNullException.ThrowIfNull(classInput, nameof(classInput));

        StringBuilder queryStringBuilder = new StringBuilder();
        queryStringBuilder.Append(GetAggregatedPrefixes());

        queryStringBuilder.Append(BuildUpdateClassMulilanguageQuery(
            datasetId,
            classInput.UriComplete,
            "rdfs:label",
            "sh:name",
            classInput.Label));

        queryStringBuilder.Append(BuildUpdateClassMulilanguageQuery(
            datasetId,
            classInput.UriComplete,
            "dcterms:description",
            "sh:description",
            classInput.Description));

        return queryStringBuilder.ToString();
    }

    internal static string UpdatePropertyQuery(
    SchemaProperty propertyInput,
    Guid datasetId,
    Uri classUri)
    {
        ArgumentNullException.ThrowIfNull(propertyInput, nameof(propertyInput));

        StringBuilder queryStringBuilder = new StringBuilder();
        queryStringBuilder.Append(GetAggregatedPrefixes());

        queryStringBuilder.Append(BuildUpdatePropertyMultilanguageQuery(
            datasetId,
            propertyInput.Path,
            classUri,
            "sh:name",
            "rdfs:label",
            propertyInput.Label));

        queryStringBuilder.Append(BuildUpdatePropertyMultilanguageQuery(
            datasetId,
            propertyInput.Path,
            classUri,
            "sh:description",
            "dcterms:description",
            propertyInput.Description));

        if (propertyInput.MinCardinality.HasValue)
        {
            queryStringBuilder.Append(UpsertPropertyTriple(
                datasetId,
                propertyInput.Path,
                classUri,
                "sh:minCount",
                propertyInput.MinCardinality.Value));
        }
        else
        {
            queryStringBuilder.Append(DeletePropertyTriple(
                datasetId,
                propertyInput.Path,
                classUri,
                "sh:minCount"));
        }

        if (propertyInput.MaxCardinality.HasValue)
        {
            queryStringBuilder.Append(UpsertPropertyTriple(
                datasetId,
                propertyInput.Path,
                classUri,
                "sh:maxCount",
                propertyInput.MaxCardinality.Value));
        }
        else
        {
            queryStringBuilder.Append(DeletePropertyTriple(
                datasetId,
                propertyInput.Path,
                classUri,
                "sh:maxCount"));
        }

        if (propertyInput.MinLength.HasValue)
        {
            queryStringBuilder.Append(UpsertPropertyTriple(
                datasetId,
                propertyInput.Path,
                classUri,
                "sh:minLength",
                propertyInput.MinLength.Value));
        }
        else
        {
            queryStringBuilder.Append(DeletePropertyTriple(
                datasetId,
                propertyInput.Path,
                classUri,
                "sh:minLength"));
        }

        if (propertyInput.MaxLength.HasValue)
        {
            queryStringBuilder.Append(UpsertPropertyTriple(
                datasetId,
                propertyInput.Path,
                classUri,
                "sh:maxLength",
                propertyInput.MaxLength.Value));
        }
        else
        {
            queryStringBuilder.Append(DeletePropertyTriple(
                datasetId,
                propertyInput.Path,
                classUri,
                "sh:maxLength"));
        }

        if (propertyInput.Order.HasValue)
        {
            queryStringBuilder.Append(UpsertPropertyTriple(
                datasetId,
                propertyInput.Path,
                classUri,
                "sh:order",
                propertyInput.Order.Value));
        }
        else
        {
            queryStringBuilder.Append(DeletePropertyTriple(
                datasetId,
                propertyInput.Path,
                classUri,
                "sh:order"));
        }

        if (!string.IsNullOrWhiteSpace(propertyInput.Pattern))
        {
            queryStringBuilder.Append(UpsertPropertyTriple(
                datasetId,
                propertyInput.Path,
                classUri,
                "sh:pattern",
                propertyInput.Pattern));
        }
        else
        {
            queryStringBuilder.Append(DeletePropertyTriple(
                datasetId,
                propertyInput.Path,
                classUri,
                "sh:pattern"));
        }

        if (!string.IsNullOrWhiteSpace(propertyInput.ConformsTo?.AbsoluteUri))
        {
            queryStringBuilder.Append(UpsertPropertyTriple(
                datasetId,
                propertyInput.Path,
                classUri,
                "dcterms:conformsTo",
                propertyInput.ConformsTo));
        }
        else
        {
            queryStringBuilder.Append(DeletePropertyTriple(
                datasetId,
                propertyInput.Path,
                classUri,
                "dcterms:conformsTo"));
        }

        if (!string.IsNullOrWhiteSpace(propertyInput.DataType))
        {
            Uri dataTypeUri = ExpandPrefixedName(propertyInput.DataType, "xsd");
            queryStringBuilder.Append(UpsertPropertyTriple(
                datasetId,
                propertyInput.Path,
                classUri,
                "sh:datatype",
                dataTypeUri));
        }
        else
        {
            queryStringBuilder.Append(DeletePropertyTriple(
                datasetId,
                propertyInput.Path,
                classUri,
                "sh:datatype"));
        }

        if (!string.IsNullOrWhiteSpace(propertyInput.Unit))
        {
            Uri unitUri = ExpandPrefixedName($"unit:{propertyInput.Unit}", "unit");

            queryStringBuilder.Append(UpsertPropertyTriple(
                datasetId,
                propertyInput.Path,
                classUri,
                "qudt:unit",
                unitUri));
        }
        else
        {
            queryStringBuilder.Append(DeletePropertyTriple(
                datasetId,
                propertyInput.Path,
                classUri,
                "qudt:unit"));
        }

        queryStringBuilder.AppendLine(UpdateListOfValueQuery(
            datasetId,
            propertyInput.Path,
            classUri,
            propertyInput.AllowedValues));

        return queryStringBuilder.ToString();
    }

    internal static string GetPropertyCountFromClassQuery(Uri classUri)
    {
        var queryString = new SparqlParameterizedString();
        queryString.CommandText = $@"
SELECT (COUNT(?prop) AS ?{PropertyCountColumn})
WHERE {{
  @classUri sh:property ?prop .
}}";
        queryString.SetUri("classUri", classUri);
        return GetAggregatedPrefixes() + queryString.ToString();
    }

    internal static string UpdatePropertyUriQuery(
        Guid datasetId,
        Uri oldPropertyUri,
        Uri newPropertyUri,
        Uri classUri)
    {
        var queryString = new SparqlParameterizedString();
        queryString.CommandText = $@"
DELETE {{
    GRAPH <{StoredDefaultGraph}> {{
        ?propertyResource ?p ?o .
        ?propertyResource sh:path @oldUri .
        ?parent sh:property ?propertyResource .
    }}
}}
INSERT {{
    GRAPH <{StoredDefaultGraph}> {{
        @newUri ?p ?newO .
        @newUri sh:path @newUri .
        ?parent sh:property @newUri .
    }}
}}
WHERE {{
    GRAPH <{StoredDefaultGraph}> {{
        # Validation: ensure we're in the right dataset
        ?root schema:identifier ""{datasetId}"".
        ?root (!(dcterms:conformsTo))* @classUri .
        
        # Find property resource by sh:path (more reliable than URI)
        ?propertyResource sh:path @oldUri .
        
        # Verify it's referenced by the class
        @classUri sh:property ?propertyResource .
        
        # Get all properties except sh:path (handle separately)
        ?propertyResource ?p ?o .
        FILTER(?p != sh:path)
        
        BIND(?o AS ?newO)
        
        # Find parent reference
        ?parent sh:property ?propertyResource .
    }}
}};";
        queryString.SetUri("oldUri", oldPropertyUri);
        queryString.SetUri("newUri", newPropertyUri);
        queryString.SetUri("classUri", classUri);
        return GetAggregatedPrefixes() + queryString.ToString();
    }

    /// <summary>
    /// Builds a SPARQL INSERT query that creates a new class (sh:NodeShape)
    /// attached to the dataset structure root via <c>dcterms:hasPart</c>.
    /// Only <c>rdfs:label</c> and <c>dcterms:description</c> multi-language values from
    /// <paramref name="classInput"/> are inserted, in addition to the <c>sh:NodeShape</c> type.
    /// The class URI used in the query is <paramref name="newClassUri"/>.
    /// The structure root (<paramref name="structureRootUri"/>) is also asserted with its
    /// <c>schema:identifier</c>, which safely bootstraps the root when creating the first class.
    /// Re-asserting an existing triple is a no-op in RDF.
    /// </summary>
    internal static string CreateSchemaClassQuery(
        Guid datasetId,
        SchemaClass classInput,
        Uri newClassUri,
        Uri structureRootUri)
    {
        ArgumentNullException.ThrowIfNull(classInput, nameof(classInput));
        ArgumentNullException.ThrowIfNull(newClassUri, nameof(newClassUri));
        ArgumentNullException.ThrowIfNull(structureRootUri, nameof(structureRootUri));

        var insertTriples = new StringBuilder();
        insertTriples.AppendLine($"        @rootUri schema:identifier \"{datasetId}\" .");
        insertTriples.AppendLine("        @classUri a sh:NodeShape .");
        insertTriples.AppendLine("        @rootUri dcterms:hasPart @classUri .");

        AppendMultilangInsertTriples(insertTriples, "rdfs:label", classInput.Label);
        AppendMultilangInsertTriples(insertTriples, "dcterms:description", classInput.Description);

        var queryString = new SparqlParameterizedString();
        queryString.CommandText = $@"
INSERT DATA {{
    GRAPH <{StoredDefaultGraph}> {{
{insertTriples}    }}
}};";
        queryString.SetUri("classUri", newClassUri);
        queryString.SetUri("rootUri", structureRootUri);

        return GetAggregatedPrefixes() + queryString.ToString();
    }

    /// <summary>
    /// Builds a SPARQL INSERT DATA query that attaches a new PropertyShape to an existing
    /// class (<paramref name="classUri"/>) via <c>sh:property</c>, with only its mandatory
    /// <c>sh:path</c> triple set. The property shape IRI is the same as its <c>sh:path</c>,
    /// matching the convention used across the codebase (see <see cref="UpdatePropertyUriQuery"/>).
    /// Other attributes (label, description, cardinalities, etc.) should be applied afterwards
    /// via <see cref="UpdatePropertyQuery"/>.
    /// </summary>
    internal static string CreateSchemaPropertyQuery(
        Uri classUri,
        Uri propertyUri)
    {
        ArgumentNullException.ThrowIfNull(classUri, nameof(classUri));
        ArgumentNullException.ThrowIfNull(propertyUri, nameof(propertyUri));

        var queryString = new SparqlParameterizedString();
        queryString.CommandText = $@"
INSERT DATA {{
    GRAPH <{StoredDefaultGraph}> {{
        @classUri sh:property @propUri .
        @propUri a sh:PropertyShape ;
                 sh:path @propUri .
    }}
}};";
        queryString.SetUri("classUri", classUri);
        queryString.SetUri("propUri", propertyUri);

        return GetAggregatedPrefixes() + queryString.ToString();
    }

    private static void AppendMultilangInsertTriples(
        StringBuilder builder,
        string predicate,
        MultiLanguageModel? languageModel)
    {
        if (languageModel == null)
        {
            return;
        }

        void AppendIfNotEmpty(string? value, string lang)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            var escaped = EscapeLiteral(value);
            builder.AppendLine($"        @classUri {predicate} \"{escaped}\"@{lang} .");
        }

        AppendIfNotEmpty(languageModel.En, "en");
        AppendIfNotEmpty(languageModel.Fr, "fr");
        AppendIfNotEmpty(languageModel.De, "de");
        AppendIfNotEmpty(languageModel.It, "it");
        AppendIfNotEmpty(languageModel.Rm, "rm");
    }

    internal static string UpdateClassUriQuery(
        Guid datasetId,
        Uri oldClassUri,
        Uri newClassUri)
    {
        var queryString = new SparqlParameterizedString();
        queryString.CommandText = $@"
DELETE {{
    GRAPH <{StoredDefaultGraph}> {{
        @oldUri ?p ?o .
        ?s ?p2 @oldUri .
    }}
}}
INSERT {{
    GRAPH <{StoredDefaultGraph}> {{
        @newUri ?p ?newO .
        ?newS ?p2 @newUri .
    }}
}}
WHERE {{
    GRAPH <{StoredDefaultGraph}> {{
        ?root schema:identifier ""{datasetId}"".
        ?root (!(dcterms:conformsTo))* @oldUri .
        {{
            @oldUri ?p ?o .
            BIND(IF(?o = @oldUri, @newUri, ?o) AS ?newO)
        }}
        UNION
        {{
            ?s ?p2 @oldUri .
            BIND(IF(?s = @oldUri, @newUri, ?s) AS ?newS)
        }}
    }}
}};";

        queryString.SetUri("oldUri", oldClassUri);
        queryString.SetUri("newUri", newClassUri);

        return GetAggregatedPrefixes() + queryString.ToString();
    }

    internal static string DeleteSchemaPropertyQuery(Guid datasetId, Uri propertyUri)
    {
        var queryString = new SparqlParameterizedString();
        queryString.CommandText = $@"
DELETE {{
    GRAPH <{StoredDefaultGraph}> {{
        @propertyUri ?p ?o .
        ?parent sh:property @propertyUri .
        ?listNode ?lp ?lo .
    }}
}}
WHERE {{
    GRAPH <{StoredDefaultGraph}> {{
        # Dataset-scope and type guard: only delete real PropertyShapes attached to NodeShapes
        ?root schema:identifier ""{datasetId}"" .
        ?root dcterms:hasPart ?parent .
        ?parent a sh:NodeShape ;
                sh:property @propertyUri .
        @propertyUri a sh:PropertyShape ;
                     sh:path @propertyUri .

        {{
            # All triples where the PropertyShape is subject (sh:path, sh:name, sh:minCount, ...)
            @propertyUri ?p ?o .
        }}
        UNION
        {{
            # Back-reference from the NodeShape: remove @propertyUri from the sh:property list
            ?parent sh:property @propertyUri .
        }}
        UNION
        {{
            # Cascade sh:in allowed-value RDF list nodes (blank nodes) if any
            @propertyUri sh:in ?list .
            ?list rdf:rest* ?listNode .
            ?listNode ?lp ?lo .
        }}
    }}
}};";

        queryString.SetUri("propertyUri", propertyUri);

        return GetAggregatedPrefixes() + queryString.ToString();
    }

    internal static void CleanStructureBeforeExport(Graph graph, Guid datasetId)
    {
        var schemaIdentifier = graph.CreateUriNode(new Uri("http://schema.org/#identifier"));

        var datasetIdLiteral = graph.CreateLiteralNode(datasetId.ToString());

        // Remove the structure root node added during import, which is used only to index the structure in the triplestore
        var rootTriples = graph
            .GetTriplesWithPredicateObject(schemaIdentifier, datasetIdLiteral)
            .ToList();

        foreach (var rootTriple in rootTriples)
        {
            var rootSubject = rootTriple.Subject;

            var triplesToRemove = graph
                .GetTriplesWithSubject(rootSubject)
                .ToList();

            graph.Retract(triplesToRemove);
        }

        // The graph BaseUri is set from the SPARQL endpoint and is not part of the exported model
        // Clear it to avoid emitting an artificial @base directive in Turtle-based formats (e.g. @base <http://localhost:3030/ds/sparql>.)
        graph.BaseUri = null;

        SortShPropertyByShOrder(graph);
    }

    private static void SortShPropertyByShOrder(Graph graph)
    {
        var shPropertyNode = graph.CreateUriNode(ExpandPrefixedName("sh:property", "sh"));
        var shOrderNode = graph.CreateUriNode(ExpandPrefixedName("sh:order", "sh"));

        // Group sh:property triples by their NodeShape subject.
        var groups = graph
            .GetTriplesWithPredicate(shPropertyNode)
            .GroupBy(t => t.Subject)
            .ToList();

        foreach (var group in groups)
        {
            var propertyTriples = group.ToList();
            if (propertyTriples.Count < 2)
            {
                continue;
            }

            var ordered = propertyTriples
                .Select((t, index) => new
                {
                    Triple = t,
                    Order = TryGetShOrder(graph, shOrderNode, t.Object),
                    OriginalIndex = index,
                })
                .OrderBy(x => x.Order ?? int.MaxValue)
                .ThenBy(x => x.OriginalIndex)
                .Select(x => x.Triple)
                .ToList();

            if (ordered.SequenceEqual(propertyTriples))
            {
                continue;
            }

            graph.Retract(propertyTriples);

            for (var i = ordered.Count - 1; i >= 0; i--)
            {
                graph.Assert(ordered[i]);
            }
        }
    }

    private static int? TryGetShOrder(Graph graph, IUriNode shOrderNode, INode propertyShape)
    {
        var orderTriple = graph
            .GetTriplesWithSubjectPredicate(propertyShape, shOrderNode)
            .FirstOrDefault();

        if (orderTriple?.Object is ILiteralNode literal
            && int.TryParse(literal.Value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var value))
        {
            return value;
        }

        return null;
    }

    /// <summary>
    /// Post-processes a Turtle document so that, for every NodeShape, its child
    /// PropertyShape "subject blocks" appear in ascending <c>sh:order</c>. Blocks for
    /// PropertyShapes attached to different NodeShapes are never mixed: within the
    /// positions originally occupied by a given NodeShape's PropertyShape blocks,
    /// those blocks are sorted by <c>sh:order</c>. Non-PropertyShape blocks keep
    /// their original position.
    /// </summary>
    /// <remarks>
    /// The <see cref="CompressingTurtleWriter"/> emits subject blocks in an order
    /// derived from the graph's internal indexing (typically alphabetical by IRI),
    /// which is not controllable via triple insertion order. Rewriting the produced
    /// Turtle text is therefore the pragmatic way to guarantee <c>sh:order</c>
    /// preservation in the exported document.
    /// </remarks>
    internal static string SortPropertyShapeBlocksByShOrder(string turtle, Graph graph)
    {
        ArgumentNullException.ThrowIfNull(turtle, nameof(turtle));
        ArgumentNullException.ThrowIfNull(graph, nameof(graph));

        if (string.IsNullOrEmpty(turtle))
        {
            return turtle;
        }

        var shPropertyNode = graph.CreateUriNode(ExpandPrefixedName("sh:property", "sh"));
        var shOrderNode = graph.CreateUriNode(ExpandPrefixedName("sh:order", "sh"));

        // propertyShape IRI -> (parent NodeShape IRI, sh:order or int.MaxValue)
        var propertyShapeMeta = new Dictionary<string, (string ParentUri, int Order)>(StringComparer.Ordinal);
        foreach (var triple in graph.GetTriplesWithPredicate(shPropertyNode))
        {
            if (triple.Subject is not IUriNode parent || triple.Object is not IUriNode propertyShape)
            {
                continue;
            }

            var order = TryGetShOrder(graph, shOrderNode, propertyShape) ?? int.MaxValue;
            propertyShapeMeta[propertyShape.Uri.AbsoluteUri] = (parent.Uri.AbsoluteUri, order);
        }

        if (propertyShapeMeta.Count == 0)
        {
            return turtle;
        }

        var newline = turtle.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
        var lines = turtle.Split(new[] { newline }, StringSplitOptions.None);

        var blocks = new List<List<string>>();
        var current = new List<string>();
        var currentHasSubject = false;

        foreach (var line in lines)
        {
            var startsSubjectBlock = line.Length > 0 && line[0] == '<';
            if (startsSubjectBlock && currentHasSubject)
            {
                blocks.Add(current);
                current = new List<string>();
                currentHasSubject = false;
            }

            current.Add(line);
            if (startsSubjectBlock)
            {
                currentHasSubject = true;
            }
        }

        if (current.Count > 0)
        {
            blocks.Add(current);
        }

        // Extract the subject IRI of each block, if any.
        var blockSubjects = new string?[blocks.Count];
        for (var i = 0; i < blocks.Count; i++)
        {
            var firstIriLine = blocks[i].FirstOrDefault(l => l.Length > 0 && l[0] == '<');
            if (firstIriLine is null)
            {
                continue;
            }

            var end = firstIriLine.IndexOf('>', 1);
            if (end > 1)
            {
                blockSubjects[i] = firstIriLine.Substring(1, end - 1);
            }
        }

        // Group PropertyShape block positions by parent NodeShape.
        var groups = new Dictionary<string, List<int>>(StringComparer.Ordinal);
        for (var i = 0; i < blocks.Count; i++)
        {
            var subject = blockSubjects[i];
            if (subject is null || !propertyShapeMeta.ContainsKey(subject))
            {
                continue;
            }

            var parent = propertyShapeMeta[subject].ParentUri;
            if (!groups.TryGetValue(parent, out var positions))
            {
                positions = new List<int>();
                groups[parent] = positions;
            }

            positions.Add(i);
        }

        // Within each group, sort the PropertyShape blocks by sh:order.
        foreach (var (_, positions) in groups)
        {
            if (positions.Count < 2)
            {
                continue;
            }

            var orderedBlocks = positions
                .Select(p => new
                {
                    Position = p,
                    Block = blocks[p],
                    Order = propertyShapeMeta[blockSubjects[p]!].Order,
                })
                .OrderBy(x => x.Order)
                .ThenBy(x => x.Position)
                .Select(x => x.Block)
                .ToList();

            for (var j = 0; j < positions.Count; j++)
            {
                blocks[positions[j]] = orderedBlocks[j];
            }
        }

        var builder = new StringBuilder(turtle.Length);
        for (var i = 0; i < blocks.Count; i++)
        {
            var block = blocks[i];
            for (var k = 0; k < block.Count; k++)
            {
                builder.Append(block[k]);
                var isLast = i == blocks.Count - 1 && k == block.Count - 1;
                if (!isLast)
                {
                    builder.Append(newline);
                }
            }
        }

        return builder.ToString();
    }

    /// <summary>
    /// Returns, per concept IRI, the distinct structure-attribute references — one
    /// <c>(conceptUri, datasetId, propertyUri)</c> row per property shape that
    /// <c>dcterms:conformsTo</c> the concept across all dataset models. The caller filters the
    /// rows by the datasets the user may read and counts the survivors (so the count respects the
    /// same read-authorization as the catalogue search).
    /// </summary>
    internal static string GetConceptStructureReferencesQuery(IEnumerable<string> conceptIris)
    {
        // Concept IRIs are built from client-supplied identifier/version, so they must never be
        // interpolated raw into the query. Validate each as an absolute URI (skipping invalid ones)
        // and emit it through the SPARQL formatter, which escapes any characters that could break
        // out of the <...> IRI token. An empty VALUES block is valid SPARQL and simply yields no rows.
        var formatter = new SparqlFormatter();
        using var nodeFactory = new Graph();

        var valuesList = string.Join(" ", conceptIris
            .Where(iri => Uri.TryCreate(iri, UriKind.Absolute, out _))
            .Select(iri => formatter.Format(nodeFactory.CreateUriNode(new Uri(iri)))));

        return $@"
{GetAggregatedPrefixes()}
SELECT DISTINCT ?{ConceptUriColumn} ?{DatasetIdColumn} ?{PropertyUriColumn}
WHERE {{
  GRAPH <{StoredDefaultGraph}> {{
    ?{PropertyUriColumn} dcterms:conformsTo ?{ConceptUriColumn} .
    ?nodeShape sh:property ?{PropertyUriColumn} ;
               a sh:NodeShape .
    ?root dcterms:hasPart ?nodeShape ;
          schema:identifier ?{DatasetIdColumn} .
    VALUES ?{ConceptUriColumn} {{ {valuesList} }}
  }}
}}";
    }

    internal static string GetConceptConformsToReuseQuery(Uri conceptUri)
    {
        var query = new SparqlParameterizedString();
        query.CommandText = $@"
{GetAggregatedPrefixes()}
SELECT DISTINCT ?datasetId ?propertyUri
WHERE {{
  GRAPH <{StoredDefaultGraph}> {{
    ?propertyUri dcterms:conformsTo @conceptUri .

    ?nodeShape sh:property ?propertyUri ;
               a sh:NodeShape .

    ?root dcterms:hasPart ?nodeShape ;
          schema:identifier ?datasetId .
  }}
}}
ORDER BY ?datasetId ?propertyUri";

        query.SetUri("conceptUri", conceptUri);

        return query.ToString();
    }

    private static List<ISparqlResult> GetPropertyDescriptionFromGraphQuery(string uriClass, string uriPath, Graph graph)
    {
        string queryStr = $@"SELECT DISTINCT ?{ContentColumn}
WHERE {{
  BIND(<{uriPath}> AS ?targetPath)
  BIND(<{uriClass}> AS ?targetClass)

  ?targetClass sh:property ?prop .
  ?prop sh:path ?targetPath .

  OPTIONAL {{ ?prop sh:description ?shDesc }}
  OPTIONAL {{ ?prop dcterms:description ?dctDesc }}

  BIND(COALESCE(?shDesc, ?dctDesc) AS ?{ContentColumn})
}}";

        SparqlResultSet resultat = (SparqlResultSet)graph.ExecuteQuery(GetAggregatedPrefixes() + queryStr);
        return resultat.Results;
    }

    private static List<ISparqlResult> GetPropertyLabelFromGraphQuery(string uriClass, string uriPath, Graph graph)
    {
        string queryStr = $"SELECT DISTINCT  ?{ContentColumn}\r\nWHERE {{\r\n  BIND(<{uriPath}> AS ?targetPath)\r\n  BIND(<{uriClass}> AS ?targetClass)  \r\n  {{\r\n      ?targetClass sh:property [sh:path ?targetPath ; sh:name ?{ContentColumn} ] .\r\n  }}\r\n  UNION\r\n  {{\r\n  ?targetClass sh:property [sh:path ?targetPath ; rdfs:label ?{ContentColumn} ] .\r\n    FILTER(NOT EXISTS {{\r\n    ?targetClass sh:property [sh:path ?targetPath ; sh:name ?description ] .\r\n    }})\r\n  }} }}";
        SparqlResultSet resultat = (SparqlResultSet)graph.ExecuteQuery(GetAggregatedPrefixes() + queryStr);
        return resultat.Results;
    }

    private static string BuildRecursiveStructureSelectionWhereClause(Guid datasetId)
    {
        return $@"
WHERE {{
GRAPH <{StoredDefaultGraph}> {{
    {{
        # Get the structure root itself and all its triples
        ?root schema:identifier ""{datasetId}"" .
        ?root ?p ?o .
        BIND(?root AS ?s)
    }}
    UNION
    {{
        # Get all NodeShapes linked from the structure root and all their triples
        ?root schema:identifier ""{datasetId}"" .
        ?root dcterms:hasPart ?nodeShape .
        ?nodeShape a sh:NodeShape .
        ?nodeShape ?p ?o .
        BIND(?nodeShape AS ?s)
    }}
    UNION
    {{
        # Get all ontologies linked from the structure root (for example in SPHN) and all their triples
        ?root schema:identifier ""{datasetId}"" .
        ?root dcterms:hasPart ?ontology .
        ?ontology a owl:Ontology .
        ?ontology ?p ?o .
        BIND(?ontology AS ?s)
    }}
    UNION
    {{
        # Get all PropertyShapes linked from the NodeShapes and all their triples
        ?root schema:identifier ""{datasetId}"" .
        ?root dcterms:hasPart ?nodeShape .
        ?nodeShape a sh:NodeShape .
        ?nodeShape sh:property ?propertyShape .
        ?propertyShape ?p ?o .
        BIND(?propertyShape AS ?s)
    }}
    UNION
    {{
        # Recursively get all blank nodes reachable from each PropertyShape
        # Do not traverse beyond links of the form x dcterms:conformsTo concept
        # so that external concepts are not pulled into the structure subgraph
        ?root schema:identifier ""{datasetId}"" .
        ?root dcterms:hasPart ?nodeShape .
        ?nodeShape a sh:NodeShape .
        ?nodeShape sh:property ?propertyShape .

        ?propertyShape (!dcterms:conformsTo)* ?s .
        FILTER(isBlank(?s))

        ?s ?p ?o .
    }}
}}
}}";
    }

    private static string BuildSimpleStructureSelectionWhereClause(Guid datasetId)
    {
        return $@"
WHERE {{
    GRAPH <{StoredDefaultGraph}> {{

        {{
            # Get the structure root itself and all its triples
            ?root schema:identifier ""{datasetId}"" .
            ?root ?p ?o .
            BIND(?root AS ?s)
        }}
        UNION
        {{
            # Get all NodeShapes linked from the structure root and all their triples
            ?root schema:identifier ""{datasetId}"" .
            ?root dcterms:hasPart ?nodeShape .
            ?nodeShape a sh:NodeShape .
            ?nodeShape ?p ?o .
            BIND(?nodeShape AS ?s)
        }}
        UNION
        {{
            # Get all ontologies linked from the structure root (for example in SPHN) and all their triples
            ?root schema:identifier ""{datasetId}"" .
            ?root dcterms:hasPart ?ontology .
            ?ontology a owl:Ontology .
            ?ontology ?p ?o .
            BIND(?ontology AS ?s)
        }}
        UNION
        {{
            # Get all PropertyShapes linked from the NodeShapes and all their triples
            ?root schema:identifier ""{datasetId}"" .
            ?root dcterms:hasPart ?nodeShape .
            ?nodeShape a sh:NodeShape .
            ?nodeShape sh:property ?propertyShape .
            ?propertyShape ?p ?o .
            BIND(?propertyShape AS ?s)
        }}
        UNION
        {{
            # Get the first-level blank nodes directly linked from each PropertyShape and all their triples
            # Ignore dcterms:conformsTo so that external concepts are not followed
            ?root schema:identifier ""{datasetId}"" .
            ?root dcterms:hasPart ?nodeShape .
            ?nodeShape a sh:NodeShape .
            ?nodeShape sh:property ?propertyShape .

            ?propertyShape ?lp1 ?b1 .
            FILTER(isBlank(?b1))
            FILTER(?lp1 != dcterms:conformsTo)

            ?b1 ?p ?o .

            BIND(?b1 AS ?s)
        }}
    }}
}}";
    }

    private static string BuildUpdateClassMulilanguageQuery(
    Guid datasetId,
    Uri uriComplete,
    string predicate,
    string secondaryPredicate,
    MultiLanguageModel? languageModel)
    {
        if (languageModel == null)
        {
            return DeleteClassTripleMultilangue(datasetId, uriComplete, predicate, secondaryPredicate);
        }

        var values = new Dictionary<string, string>();

        void AddIfNotEmpty(string? value, string lang)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                values.Add(lang, EscapeLiteral(value));
            }
        }

        AddIfNotEmpty(languageModel.En, "en");
        AddIfNotEmpty(languageModel.Fr, "fr");
        AddIfNotEmpty(languageModel.De, "de");
        AddIfNotEmpty(languageModel.It, "it");
        AddIfNotEmpty(languageModel.Rm, "rm");

        if (values.Count == 0)
        {
            return DeleteClassTripleMultilangue(datasetId, uriComplete, predicate, secondaryPredicate);
        }

        var queryString = new SparqlParameterizedString();

        queryString.CommandText = $@"
DELETE {{
    GRAPH <{StoredDefaultGraph}> {{
        @subject ?p ?o .
    }}
}}
INSERT {{
    GRAPH <{StoredDefaultGraph}> {{
        @subject ";

        foreach (var langValue in values)
        {
            queryString.Append($"{predicate} @langValue{langValue.Key};\r\n");
            queryString.SetLiteral($"@langValue{langValue.Key}", langValue.Value, langValue.Key, false);
        }

        queryString.Append($@" .
    }}
}}
WHERE {{
    GRAPH <{StoredDefaultGraph}> {{
        ?root schema:identifier ""{datasetId}"".
        ?root (!(dcterms:conformsTo))* @subject .
        OPTIONAL {{
            @subject ?p ?o .
            VALUES ?p {{ {predicate} {secondaryPredicate} }}
        }}
    }}
}};");

        queryString.SetUri("subject", uriComplete);
        return queryString.ToString();
    }

    private static string BuildUpdatePropertyMultilanguageQuery(
    Guid datasetId,
    Uri uriPath,
    Uri uriClass,
    string predicate,
    string secondaryPredicate,
    MultiLanguageModel? languageModel)
    {
        if (languageModel == null)
        {
            return DeletePropertyTripleMultilangue(datasetId, uriPath, uriClass, predicate, secondaryPredicate);
        }

        var values = new Dictionary<string, string>();

        void AddIfNotEmpty(string? value, string lang)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                values.Add(lang, EscapeLiteral(value));
            }
        }

        AddIfNotEmpty(languageModel.En, "en");
        AddIfNotEmpty(languageModel.Fr, "fr");
        AddIfNotEmpty(languageModel.De, "de");
        AddIfNotEmpty(languageModel.It, "it");
        AddIfNotEmpty(languageModel.Rm, "rm");

        if (values.Count == 0)
        {
            return DeletePropertyTripleMultilangue(datasetId, uriPath, uriClass, predicate, secondaryPredicate);
        }

        var queryString = new SparqlParameterizedString();

        queryString.CommandText = $@"
DELETE {{
    GRAPH <{StoredDefaultGraph}> {{
        ?prop ?oldP ?oldValue .
    }}
}}
INSERT {{
    GRAPH <{StoredDefaultGraph}> {{
        ?prop ";

        foreach (var langValue in values)
        {
            queryString.Append($"{predicate} @langValue{langValue.Key};\r\n");
            queryString.SetLiteral($"@langValue{langValue.Key}", langValue.Value, langValue.Key, false);
        }

        queryString.Append($@" .
    }}
}}
WHERE {{
    GRAPH <{StoredDefaultGraph}> {{
        ?root schema:identifier ""{datasetId}"".
        ?root (!(dcterms:conformsTo))* @uriClass .
        @uriClass sh:property ?prop .
        ?prop sh:path @uriPath .
        OPTIONAL {{
            ?prop ?oldP ?oldValue .
            VALUES ?oldP {{ {predicate} {secondaryPredicate} }}
        }}
    }}
}};");

        queryString.SetUri("uriPath", uriPath);
        queryString.SetUri("uriClass", uriClass);
        return queryString.ToString();
    }

    private static string DeleteClassTriple(
    Guid datasetId,
    Uri uriComplete,
    string predicate)
    {
        var queryString = new SparqlParameterizedString();
        queryString.CommandText = $@"
DELETE {{
    GRAPH <{StoredDefaultGraph}> {{
        @subject {predicate} ?oldValue .
    }}
}}
WHERE {{
    GRAPH <{StoredDefaultGraph}> {{
        ?root schema:identifier ""{datasetId}"".
        ?root (!(dcterms:conformsTo))* @subject .
        @subject {predicate} ?oldValue .
    }}
}};";

        queryString.SetUri("subject", uriComplete);

        return queryString.ToString();
    }

    private static string DeleteClassTripleMultilangue(
    Guid datasetId,
    Uri uriComplete,
    string predicate,
    string secondPredicate)
    {
        var queryString = new SparqlParameterizedString();
        queryString.CommandText = $@"
DELETE {{
    GRAPH <{StoredDefaultGraph}> {{
        @subject ?p ?oldValue .
    }}
}}
WHERE {{
    GRAPH <{StoredDefaultGraph}> {{
        ?root schema:identifier ""{datasetId}"".
        ?root (!(dcterms:conformsTo))* @subject .
        @subject ?p ?oldValue .
        FILTER (?p IN ({predicate}, {secondPredicate}))
    }}
}};";

        queryString.SetUri("subject", uriComplete);

        return queryString.ToString();
    }

    private static string DeletePropertyTriple(
    Guid datasetId,
    Uri uriPath,
    Uri classUri,
    string predicate)
    {
        var queryString = new SparqlParameterizedString();
        queryString.CommandText = $@"
DELETE {{
    GRAPH <{StoredDefaultGraph}> {{
        ?prop {predicate} ?oldValue .
    }}
}}
WHERE {{
    GRAPH <{StoredDefaultGraph}> {{
        ?root schema:identifier ""{datasetId}"".
        ?root (!(dcterms:conformsTo))* @classUri .
        @classUri sh:property ?prop .
        ?prop sh:path @subject .
        OPTIONAL {{ ?prop {predicate} ?oldValue }}
    }}
}};";

        queryString.SetUri("subject", uriPath);
        queryString.SetUri("classUri", classUri);

        return queryString.ToString();
    }

    private static string DeletePropertyTripleMultilangue(
    Guid datasetId,
    Uri uriPath,
    Uri uriClass,
    string predicate,
    string secondaryPredicate)
    {
        var queryString = new SparqlParameterizedString();
        queryString.CommandText = $@"
DELETE {{
    GRAPH <{StoredDefaultGraph}> {{
        ?prop ?p ?oldValue .
    }}
}}
WHERE {{
    GRAPH <{StoredDefaultGraph}> {{
        ?root schema:identifier ""{datasetId}"".
        ?root (!(dcterms:conformsTo))* @uriClass .
        @uriClass sh:property ?prop .
        ?prop sh:path @uriPath .
        ?prop ?p ?oldValue .
        FILTER (?p IN ({predicate}, {secondaryPredicate}))
    }}
}};";

        queryString.SetUri("uriPath", uriPath);
        queryString.SetUri("uriClass", uriClass);

        return queryString.ToString();
    }

    private static string EscapeLiteral(string value)
    {
        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r");
    }

    private static StringBuilder GetAggregatedPrefixes()
    {
        return _sparqlQueryPrefixes.Aggregate(new StringBuilder(),
              (sb, kvp) => sb.AppendFormat("PREFIX {0}: <{1}> {2}", kvp.Key, kvp.Value, "\n"),
        sb => sb);
    }

    private static string UpdateListOfValueQuery(
    Guid datasetId,
    Uri uriPath,
    Uri uriClass,
    List<string> allowedValues)
    {
        var queryString = new SparqlParameterizedString();
        queryString.CommandText = $@"
DELETE {{
    GRAPH <{StoredDefaultGraph}> {{
        ?prop sh:in ?list .
        ?node ?p ?o .
    }}
}}
WHERE {{
    GRAPH <{StoredDefaultGraph}> {{
        ?root schema:identifier ""{datasetId}"".
        ?root (!(dcterms:conformsTo))* @uriClass .
        @uriClass sh:property ?prop .
        ?prop sh:path @uriPath .
        OPTIONAL {{
            ?prop sh:in ?list .
            ?list rdf:rest* ?node .
            ?node ?p ?o .
        }}
    }}
}};";

        if (allowedValues.Count > 0)
        {
            string listValues = "( " + string.Join(" ", allowedValues.Select(v => $"\"{EscapeLiteral(v)}\"")) + " )";

            queryString.Append($@"
INSERT {{
    GRAPH <{StoredDefaultGraph}> {{
        ?prop sh:in {listValues} .
    }}
}}
WHERE {{
    GRAPH <{StoredDefaultGraph}> {{
        ?root schema:identifier ""{datasetId}"".
        ?root (!(dcterms:conformsTo))* @uriClass .
        @uriClass sh:property ?prop .
        ?prop sh:path @uriPath .
    }}
}};");
        }

        queryString.SetUri("uriPath", uriPath);
        queryString.SetUri("uriClass", uriClass);

        return queryString.ToString();
    }

    private static string UpsertClassTriple(
    Guid datasetId,
    Uri uriComplete,
    string predicate,
    string insert)
    {
        var queryString = new SparqlParameterizedString();
        queryString.CommandText = $@"
DELETE {{
    GRAPH <{StoredDefaultGraph}> {{
        @subject {predicate} ?oldValue .
    }}
}}
INSERT {{
    GRAPH <{StoredDefaultGraph}> {{
        @subject {predicate} @insert .
    }}
}}
WHERE {{
    GRAPH <{StoredDefaultGraph}> {{
        ?root schema:identifier ""{datasetId}"".
        ?root (!(dcterms:conformsTo))* @subject .
        OPTIONAL {{ @subject {predicate} ?oldValue }}
    }}
}};";

        queryString.SetUri("subject", uriComplete);
        queryString.SetLiteral("insert", insert, false);

        return queryString.ToString();
    }

    private static string UpsertPropertyTriple(
    Guid datasetId,
    Uri uriPath,
    Uri classUri,
    string predicate,
    int insert)
    {
        var queryString = new SparqlParameterizedString();
        queryString.CommandText = $@"
DELETE {{
    GRAPH <{StoredDefaultGraph}> {{
        ?prop {predicate} ?oldValue .
    }}
}}
INSERT {{
    GRAPH <{StoredDefaultGraph}> {{
        ?prop {predicate} @insert .
    }}
}}
WHERE {{
    GRAPH <{StoredDefaultGraph}> {{
        ?root schema:identifier ""{datasetId}"" .
        ?root (!(dcterms:conformsTo))* @classUri .
        @classUri sh:property ?prop .
        ?prop sh:path @subject .
        OPTIONAL {{ ?prop {predicate} ?oldValue }}
    }}
}};";

        queryString.SetUri("subject", uriPath);
        queryString.SetLiteral("insert", insert);
        queryString.SetUri("classUri", classUri);

        return queryString.ToString();
    }

    private static string UpsertPropertyTriple(
    Guid datasetId,
    Uri uriPath,
    Uri classUri,
    string predicate,
    Uri insert)
    {
        var queryString = new SparqlParameterizedString();
        queryString.CommandText = $@"
DELETE {{
    GRAPH <{StoredDefaultGraph}> {{
        ?prop {predicate} ?oldValue .
    }}
}}
INSERT {{
    GRAPH <{StoredDefaultGraph}> {{
        ?prop {predicate} @insert .
    }}
}}
WHERE {{
    GRAPH <{StoredDefaultGraph}> {{
        ?root schema:identifier ""{datasetId}"" .
        ?root (!(dcterms:conformsTo))* @classUri .
        @classUri sh:property ?prop .
        ?prop sh:path @subject .
        OPTIONAL {{ ?prop {predicate} ?oldValue }}
    }}
}};";

        queryString.SetUri("subject", uriPath);
        queryString.SetUri("classUri", classUri);
        queryString.SetUri("insert", insert);

        return queryString.ToString();
    }

    private static string UpsertPropertyTriple(
    Guid datasetId,
    Uri uriPath,
    Uri classUri,
    string predicate,
    string insert)
    {
        var queryString = new SparqlParameterizedString();
        queryString.CommandText = $@"
DELETE {{
    GRAPH <{StoredDefaultGraph}> {{
        ?prop {predicate} ?oldValue .
    }}
}}
INSERT {{
    GRAPH <{StoredDefaultGraph}> {{
        ?prop {predicate} @insert .
    }}
}}
WHERE {{
    GRAPH <{StoredDefaultGraph}> {{
        ?root schema:identifier ""{datasetId}"".
        ?root (!(dcterms:conformsTo))* @classUri .
        @classUri sh:property ?prop .
        ?prop sh:path @subject .
        OPTIONAL {{ ?prop {predicate} ?oldValue }}
    }}
}};";

        queryString.SetUri("subject", uriPath);
        queryString.SetLiteral("insert", insert, false);
        queryString.SetUri("classUri", classUri);

        return queryString.ToString();
    }

    private static string GetConceptConformsToReuseBatchQuery(IEnumerable<Uri> conceptUris)
    {
        var conceptUriValues = conceptUris
            .Distinct()
            .Select(x => $"<{x.AbsoluteUri}>")
            .ToArray();

        return $@"
{GetAggregatedPrefixes()}
SELECT DISTINCT ?conceptUri ?datasetId ?propertyUri
WHERE {{
    VALUES ?conceptUri {{ {string.Join(" ", conceptUriValues)} }}

    GRAPH <{StoredDefaultGraph}> {{
        ?propertyUri dcterms:conformsTo ?conceptUri .

        ?nodeShape sh:property ?propertyUri ;
                   a sh:NodeShape .

        ?root dcterms:hasPart ?nodeShape ;
              schema:identifier ?datasetId .
    }}
}}
ORDER BY ?conceptUri ?datasetId ?propertyUri";
    }
}
