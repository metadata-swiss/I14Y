using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Models.LinkedData;
using Bfs.Iop.Core.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Writing;

namespace Bfs.Iop.Core.LinkedData.Helpers;

internal static class DatasetModelProcessHelper
{
    public const string JsonLdExtension = ".jsonld";
    public const string RdfExtension = ".rdf";
    public const string TtlExtension = ".ttl";
    private const string DefaultLanguage = "en";

    public static SchemaGraph ConvertGraphToSchemaGraph(Graph graph, string schemaName)
    {
        var resultClasses = ShaclSparqlQueryHelper.GetClassAndPropertyFromGraphQuery(graph);
        var schemaClasses = ConvertResultClassesToSchemaClasses(resultClasses, graph);

        SchemaGraph schemaGraph = new()
        {
            SchemaName = schemaName,
            Classes = schemaClasses.Values
        };

        return schemaGraph;
    }

    internal static Graph ConvertFileToGraph(IFormFile importFile, Guid datasetId)
    {
        ArgumentNullException.ThrowIfNull(importFile, nameof(importFile));

        using Stream streamFile = importFile.OpenReadStream();

        var fileExtension = Path.GetExtension(importFile.FileName);
        Graph graph;

        try
        {
            graph = LoadGraphAccordingToExtension(datasetId, streamFile, fileExtension);
            if (graph.Nodes.Count() < 2)
            {
                throw new BadRequestException("The file does not contain valid information.");
            }
        }
        catch (Exception ex) when (ex is RdfException or IOException)
        {
            throw new BadRequestException("The file contains errors.", ex);
        }

        return graph;
    }

    internal static string GetFileName(LinkedDataFormat format, Guid datasetId) =>
        $"{datasetId}.{format.ToString().ToLowerInvariant()}";

    internal static string GetMimeType(LinkedDataFormat format)
    {
        var mimeType = MimeTypesHelper.GetDefinitionsByFileExtension(format.ToString().ToLower()).FirstOrDefault()
            ?? throw new InvalidOperationException("Mime type is not supported");

        return mimeType.CanonicalMimeType;
    }

    internal static Graph LoadGraphAccordingToExtension(Guid datasetId, Stream inputFile, string fileExtension)
    {
        if (fileExtension is JsonLdExtension)
        {
            return LoadGraphFromJsonLdFile(datasetId, inputFile);
        }

        IRdfReader parser = fileExtension switch
        {
            RdfExtension => new RdfXmlParser(),
            TtlExtension => new TurtleParser(),
            _ => throw new NotSupportedException(
                $"File format is not supported. Accepted extensions: '{string.Join(", ", Enum.GetNames(typeof(LinkedDataFormat)))}'.")
        };

        Graph g = new();

        using (StreamReader renderingFileStream = new(inputFile))
        {
            parser.Load(g, renderingFileStream);
        }

        return g;
    }

    internal static void WriteGraphAccordingToFormat(Graph g, Stream stream, LinkedDataFormat format)
    {
        IRdfWriter writer = format switch
        {
            LinkedDataFormat.Ttl => new CompressingTurtleWriter(),
            LinkedDataFormat.Rdf => new RdfXmlWriter(),
            LinkedDataFormat.JsonLd => new SingleGraphWriter(new JsonLdWriter()),
            _ => throw new NotSupportedException($"The format '{format}' is not supported."),
        };

        using TextWriter textWriter = new StreamWriter(stream, leaveOpen: true);

        writer.Save(g, textWriter);
        stream.Position = 0;
    }

    private static Dictionary<string, MultiLanguageModel> BuildClassDescriptionMap(Graph graph)
    {
        var result = new Dictionary<string, MultiLanguageModel>();

        var dctDescriptionNode = graph.CreateUriNode(ShaclSparqlQueryHelper.ExpandPrefixedName("dcterms:description", "dcterms"));

        foreach (var group in graph.GetTriplesWithPredicate(dctDescriptionNode)
                                   .Where(t => t.Subject is IUriNode)
                                   .GroupBy(t => ((IUriNode)t.Subject).Uri.AbsoluteUri))
        {
            result[group.Key] = BuildMultiLanguageModelFromLiteralTriples(group);
        }

        return result;
    }

    private static Dictionary<string, MultiLanguageModel> BuildClassLabelMap(Graph graph)
    {
        var result = new Dictionary<string, MultiLanguageModel>();

        var shNameNode = graph.CreateUriNode(ShaclSparqlQueryHelper.ExpandPrefixedName("sh:name", "sh"));
        var rdfsLabelNode = graph.CreateUriNode(ShaclSparqlQueryHelper.ExpandPrefixedName("rdfs:label", "rdfs"));

        foreach (var group in graph.GetTriplesWithPredicate(shNameNode)
                                   .Where(t => t.Subject is IUriNode)
                                   .GroupBy(t => ((IUriNode)t.Subject).Uri.AbsoluteUri))
        {
            result[group.Key] = BuildMultiLanguageModelFromLiteralTriples(group);
        }

        foreach (var group in graph.GetTriplesWithPredicate(rdfsLabelNode)
                                   .Where(t => t.Subject is IUriNode)
                                   .GroupBy(t => ((IUriNode)t.Subject).Uri.AbsoluteUri)
                                   .Where(group => !result.ContainsKey(group.Key)))
        {
            result[group.Key] = BuildMultiLanguageModelFromLiteralTriples(group);
        }

        return result;
    }

    private static MultiLanguageModel BuildMultiLanguageModelFromLiteralTriples(IEnumerable<Triple> triples)
    {
        var languageDic = new Dictionary<string, string>();

        foreach (var literal in triples.Select(t => t.Object).OfType<LiteralNode>())
        {
            if (string.IsNullOrWhiteSpace(literal.Value))
            {
                continue;
            }

            var language = string.IsNullOrWhiteSpace(literal.Language)
                ? DefaultLanguage
                : literal.Language;

            if (!languageDic.ContainsKey(language))
            {
                languageDic.Add(language, literal.Value);
            }
        }

        return MultiLanguageModel.FromDictionary(languageDic);
    }

    private static Dictionary<string, MultiLanguageModel> BuildPropertyDescriptionMap(Graph graph)
    {
        var result = new Dictionary<string, MultiLanguageModel>();

        var shPropertyNode = graph.CreateUriNode(ShaclSparqlQueryHelper.ExpandPrefixedName("sh:property", "sh"));
        var shPathNode = graph.CreateUriNode(ShaclSparqlQueryHelper.ExpandPrefixedName("sh:path", "sh"));
        var shDescriptionNode = graph.CreateUriNode(ShaclSparqlQueryHelper.ExpandPrefixedName("sh:description", "sh"));
        var dctDescriptionNode = graph.CreateUriNode(ShaclSparqlQueryHelper.ExpandPrefixedName("dcterms:description", "dcterms"));

        foreach (var classTriple in graph.GetTriplesWithPredicate(shPropertyNode).Where(t => t.Subject is IUriNode))
        {
            if (classTriple.Subject is not IUriNode classNode)
            {
                continue;
            }

            var propertyNode = classTriple.Object;
            var pathTriple = graph.GetTriplesWithSubjectPredicate(propertyNode, shPathNode).FirstOrDefault();
            if (pathTriple?.Object is not IUriNode pathNode)
            {
                continue;
            }

            var key = $"{classNode.Uri.AbsoluteUri}|{pathNode.Uri.AbsoluteUri}";

            var descriptionTriples = graph.GetTriplesWithSubjectPredicate(propertyNode, shDescriptionNode).ToList();
            if (descriptionTriples.Count == 0)
            {
                descriptionTriples = graph.GetTriplesWithSubjectPredicate(propertyNode, dctDescriptionNode).ToList();
            }

            if (descriptionTriples.Count > 0)
            {
                result[key] = BuildMultiLanguageModelFromLiteralTriples(descriptionTriples);
            }
        }

        return result;
    }

    private static Dictionary<string, MultiLanguageModel> BuildPropertyLabelMap(Graph graph)
    {
        var result = new Dictionary<string, MultiLanguageModel>();

        var shPropertyNode = graph.CreateUriNode(ShaclSparqlQueryHelper.ExpandPrefixedName("sh:property", "sh"));
        var shPathNode = graph.CreateUriNode(ShaclSparqlQueryHelper.ExpandPrefixedName("sh:path", "sh"));
        var shNameNode = graph.CreateUriNode(ShaclSparqlQueryHelper.ExpandPrefixedName("sh:name", "sh"));
        var rdfsLabelNode = graph.CreateUriNode(ShaclSparqlQueryHelper.ExpandPrefixedName("rdfs:label", "rdfs"));

        foreach (var classTriple in graph.GetTriplesWithPredicate(shPropertyNode).Where(t => t.Subject is IUriNode))
        {
            if (classTriple.Subject is not IUriNode classNode)
            {
                continue;
            }

            var propertyNode = classTriple.Object;
            var pathTriple = graph.GetTriplesWithSubjectPredicate(propertyNode, shPathNode).FirstOrDefault();
            if (pathTriple?.Object is not IUriNode pathNode)
            {
                continue;
            }

            var key = $"{classNode.Uri.AbsoluteUri}|{pathNode.Uri.AbsoluteUri}";

            var labelTriples = graph.GetTriplesWithSubjectPredicate(propertyNode, shNameNode).ToList();
            if (labelTriples.Count == 0)
            {
                labelTriples = graph.GetTriplesWithSubjectPredicate(propertyNode, rdfsLabelNode).ToList();
            }

            if (labelTriples.Count > 0)
            {
                result[key] = BuildMultiLanguageModelFromLiteralTriples(labelTriples);
            }
        }

        return result;
    }

    private static Dictionary<string, SchemaClass> ConvertResultClassesToSchemaClasses(IList<ISparqlResult> resultClasses, Graph graph)
    {
        var classLabelMap = BuildClassLabelMap(graph);
        var classDescriptionMap = BuildClassDescriptionMap(graph);
        var propertyLabelMap = BuildPropertyLabelMap(graph);
        var propertyDescriptionMap = BuildPropertyDescriptionMap(graph);

        Dictionary<string, SchemaClass> schemaClasses = new Dictionary<string, SchemaClass>();

        foreach (var itemClass in resultClasses)
        {
            // get Uri of class
            INode nodeUriClass;
            itemClass.TryGetValue(ShaclSparqlQueryHelper.ClassUriColumn, out nodeUriClass);
            var uriNode = nodeUriClass as UriNode;
            if (uriNode is null || uriNode.Uri is null) continue;

            // get Path of property
            INode nodePathProperty;
            itemClass.TryGetValue(ShaclSparqlQueryHelper.PropertyPath, out nodePathProperty);
            var propertyPath = nodePathProperty as UriNode;
            if (propertyPath is null || propertyPath.Uri is null) continue;

            var classUri = uriNode.Uri.AbsoluteUri;
            var propertyPathUri = propertyPath.Uri.AbsoluteUri;
            var propertyKey = $"{classUri}|{propertyPathUri}";

            classLabelMap.TryGetValue(classUri, out var classLabel);
            classDescriptionMap.TryGetValue(classUri, out var classDescription);
            propertyLabelMap.TryGetValue(propertyKey, out var propertyLabel);
            propertyDescriptionMap.TryGetValue(propertyKey, out var propertyDescription);

            //get the link between two class
            INode nodeDestination;
            itemClass.TryGetValue(ShaclSparqlQueryHelper.DestinationColumn, out nodeDestination);
            var uriDestination = nodeDestination as UriNode;

            //get the ifClosed value of the class
            INode nodeIsClassClosed;
            bool isClassClosedGet = false;
            bool isClassClosed = false;
            itemClass.TryGetValue(ShaclSparqlQueryHelper.ClassClosed, out nodeIsClassClosed);
            var isClassClosedResult = nodeIsClassClosed as LiteralNode;
            if (isClassClosedResult is not null)
            {
                isClassClosedGet = bool.TryParse(isClassClosedResult.Value, out isClassClosed);
            }

            //get Position of the Class
            INode nodePositionX;
            itemClass.TryGetValue(ShaclSparqlQueryHelper.PositionColumnX, out nodePositionX);
            var positionClassX = nodePositionX as LiteralNode;
            double positionX = 0.0;
            bool isGetPositionX = false;
            if (positionClassX is not null)
            {
                isGetPositionX = double.TryParse(positionClassX.Value, out positionX);
            }

            INode nodePositionY;
            itemClass.TryGetValue(ShaclSparqlQueryHelper.PositionColumnY, out nodePositionY);
            var positionClassY = nodePositionY as LiteralNode;
            double positionY = 0.0;
            bool isGetPositionY = false;
            if (positionClassY is not null)
            {
                isGetPositionY = double.TryParse(positionClassY.Value, out positionY);
            }

            //get TargetClass Value of the Class
            INode nodeTargetClass;
            itemClass.TryGetValue(ShaclSparqlQueryHelper.TargetClassColumn, out nodeTargetClass);
            var targetClass = nodeTargetClass as UriNode;

            // get Uri of property
            INode nodeUriProperty;
            itemClass.TryGetValue(ShaclSparqlQueryHelper.PropertyUriColumn, out nodeUriProperty);
            var uriProperty = nodeUriProperty as UriNode;

            // get Datatype of property
            INode nodeDataTypeProperty;
            itemClass.TryGetValue(ShaclSparqlQueryHelper.PropertyDataTypeColumn, out nodeDataTypeProperty);
            var dataTypeProperty = nodeDataTypeProperty as UriNode;

            // get Pattern of property
            INode nodePatternProperty;
            itemClass.TryGetValue(ShaclSparqlQueryHelper.PropertyPatternColumn, out nodePatternProperty);
            var patternProperty = nodePatternProperty as LiteralNode;

            // get ConformsTo of property
            INode nodeConformsToProperty;
            itemClass.TryGetValue(ShaclSparqlQueryHelper.PropertyConformsToColumn, out nodeConformsToProperty);
            var conformsToProperty = nodeConformsToProperty as UriNode;

            // get MinCount of property
            INode nodeMinCount;
            itemClass.TryGetValue(ShaclSparqlQueryHelper.MinCountColumn, out nodeMinCount);
            var uriMinCount = nodeMinCount as LiteralNode;
            int minCountValue = 0;
            bool minCountParseResult = false;
            if (uriMinCount is not null)
            {
                minCountParseResult = int.TryParse(uriMinCount.Value, out minCountValue);
            }

            // get MaxCount of property
            INode nodeMaxCount;
            itemClass.TryGetValue(ShaclSparqlQueryHelper.MaxCountColumn, out nodeMaxCount);
            var uriMaxCount = nodeMaxCount as LiteralNode;
            int maxCountValue = 0;
            bool maxCountParseResult = false;
            if (uriMaxCount is not null)
            {
                maxCountParseResult = int.TryParse(uriMaxCount.Value, out maxCountValue);
            }

            // get MinLength of property
            INode nodeMinLength;
            itemClass.TryGetValue(ShaclSparqlQueryHelper.MinLengthColumn, out nodeMinLength);
            var uriMinLength = nodeMinLength as LiteralNode;
            int minLengthValue = 0;
            bool minLengthParseResult = false;
            if (uriMinLength is not null)
            {
                minLengthParseResult = int.TryParse(uriMinLength.Value, out minLengthValue);
            }

            // get MaxLength of property
            INode nodeMaxLength;
            itemClass.TryGetValue(ShaclSparqlQueryHelper.MaxLengthColumn, out nodeMaxLength);
            var uriMaxLength = nodeMaxLength as LiteralNode;
            int maxLength = 0;
            bool maxLengthParseResult = false;
            if (uriMaxLength is not null)
            {
                maxLengthParseResult = int.TryParse(uriMaxLength.Value, out maxLength);
            }

            //get Order of the Property
            INode nodeOrder;
            bool isOrderGet = false;
            itemClass.TryGetValue(ShaclSparqlQueryHelper.OrderColumn, out nodeOrder);
            var orderProperty = nodeOrder as LiteralNode;
            int order = -1;
            if (orderProperty is not null)
            {
                isOrderGet = int.TryParse(orderProperty.Value, out order);
            }

            //get Available Value of the Property
            INode nodeAllowedValues;
            itemClass.TryGetValue(ShaclSparqlQueryHelper.AllowedValuesColumn, out nodeAllowedValues);
            var allowedValuesProperty = nodeAllowedValues as LiteralNode;

            //get Unit of the Property
            INode nodeUnit;
            itemClass.TryGetValue(ShaclSparqlQueryHelper.UnitColumn, out nodeUnit);
            var unitProperty = nodeUnit as UriNode;

            SchemaProperty shemaProperty = new SchemaProperty()
            {
                Label = propertyLabel ?? null,
                Description = propertyDescription ?? null,
                Path = propertyPath.Uri,
                UriComplete = uriProperty is not null ? uriProperty.Uri : null,
                Unit = unitProperty is not null ? UriHelper.GetLastUriElementOrUriComplete(unitProperty.Uri) : null,
                DataType = dataTypeProperty is not null ? GetDataType(dataTypeProperty.Uri) : null,
                Pattern = patternProperty is not null ? patternProperty.Value : null,
                ConformsTo = conformsToProperty is not null ? conformsToProperty.Uri : null,
                Identifier = UriHelper.GetLastElementFromUri(propertyPath.Uri),
                MinCardinality = minCountParseResult ? minCountValue : null,
                MaxCardinality = maxCountParseResult ? maxCountValue : null,
                MinLength = minLengthParseResult ? minLengthValue : null,
                MaxLength = maxLengthParseResult ? maxLength : null,
                Order = isOrderGet ? order : null,
                ToClassUri = uriDestination is not null ? uriDestination.Uri : null,
                AllowedValues = allowedValuesProperty is not null
                    ? allowedValuesProperty.Value.Split(new[] { ShaclSparqlQueryHelper.Separator }, StringSplitOptions.RemoveEmptyEntries).ToList()
                    : [],
            };

            var classKey = UriHelper.GetLastUriElementOrUriComplete(uriNode.Uri);
            if (!schemaClasses.TryGetValue(classKey, out var existingClass))
            {
                SchemaClass schmaClass = new()
                {
                    Label = classLabel ?? null,
                    Description = classDescription ?? null,
                    UriComplete = uriNode.Uri,
                    TargetClass = targetClass is not null ? UriHelper.GetLastUriElementOrUriComplete(targetClass.Uri) : null,
                    Closed = isClassClosedGet ? isClassClosed : null,
                    Identifier = UriHelper.GetLastElementFromUri(uriNode.Uri),
                    Properties = [shemaProperty],
                    Point = isGetPositionY && isGetPositionX ? new SchemaPoint() { X = positionX, Y = positionY } : null,
                };
                schemaClasses.Add(classKey, schmaClass);
            }
            else
            {
                existingClass.Properties.Add(shemaProperty);
            }
        }

        return schemaClasses;
    }

    private static string GetDataType(Uri uri)
    {
        var definition = ShaclSparqlQueryHelper.GetPrefixFromUri(uri);
        return $"{definition}:{uri.Fragment.Trim('#')}";
    }

    private static Graph LoadGraphFromJsonLdFile(Guid datasetId, Stream inputFile)
    {
        Graph g = new();

        var handler = new GraphHandler(g);
        var jsonLdParser = new JsonLdParser();

        using StreamReader renderingFileStream = new(inputFile);
        jsonLdParser.Load(handler, renderingFileStream);

        return g;
    }
}