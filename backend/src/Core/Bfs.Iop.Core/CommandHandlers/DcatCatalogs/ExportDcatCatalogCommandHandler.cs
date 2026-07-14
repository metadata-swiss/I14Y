using Bfs.Iop.Core.Abstractions.Commands.DcatCatalogs;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.CommandHandlers.DcatCatalogs.Extensions;
using Bfs.Iop.Core.Data.Contracts;
using Bfs.Iop.Core.Settings;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text;
using VDS.RDF;
using VDS.RDF.Writing;

namespace Bfs.Iop.Core.CommandHandlers.DcatCatalogs;

internal sealed class ExportDcatCatalogCommandHandler : IRequestHandler<ExportDcatCatalogCommand, string>
{
    private const string BaseUriString = "https://i14y.admin.ch/resources/dcat/catalogs/";
    private const string CatalogDatasetNamespace = "https://www.i14y.admin.ch/catalog/datasets/";
    private const string DataServiceType = "DataService";
    private const string DatasetNamespace = "http://www.w3.org/ns/dcat#Dataset";
    private const string DatasetType = "Dataset";
    private const string DcatApNamespace = "http://data.europa.eu/r5r/";
    private const string DcatNamespace = "http://www.w3.org/ns/dcat#";
    private const string DecimalNamespace = "http://www.w3.org/2001/XMLSchema#decimal";
    private const string DublinCoreNamespace = "http://purl.org/dc/terms/";
    private const string FoafNamespace = "http://xmlns.com/foaf/0.1/";
    private const string InvalidUriReplacementNamespace = "https://en.wikipedia.org/wiki/Uniform_Resource_Identifier";
    private const string OGDThemesType = "VOCAB_EU_DATA_THEME";
    private const string RdfSchemaNamespace = "http://www.w3.org/2000/01/rdf-schema#";
    private const string SchemaOrgNamespace = "http://schema.org/";
    private const string SpdxNamespace = "http://spdx.org/rdf/terms#";
    private const string VcardNamespace = "http://www.w3.org/2006/vcard/ns#";

    private static readonly string[] _allowedUriSchemes = [
        Uri.UriSchemeHttp,
        Uri.UriSchemeHttps,
        Uri.UriSchemeMailto,
        Uri.UriSchemeFtp,
        Uri.UriSchemeFtps,
        Uri.UriSchemeSftp];

    private static readonly Uri _invalidUriReplacement = new(InvalidUriReplacementNamespace, UriKind.Absolute);

    private readonly ILogger<ExportDcatCatalogCommandHandler> _logger;
    private readonly IDcatCatalogsService _dcatCatalogsService;
    private readonly IDatasetsService _datasetsService;
    private readonly IDataServicesService _dataServicesService;

    private readonly string _baseDatasetUri;
    private readonly string _baseDataserviceUri;

    private IGraph _graph;
    private IUriNode _catalog;
    private IUriNode _rdfType;

    public ExportDcatCatalogCommandHandler(
        IDcatCatalogsService dcatCatalogsService,
        IDatasetsService datasetsService,
        IDataServicesService dataServicesService,
        IOptions<I14YOptions> i14yOptions,
        ILogger<ExportDcatCatalogCommandHandler> logger)
    {
        _dcatCatalogsService = dcatCatalogsService;
        _datasetsService = datasetsService;
        _dataServicesService = dataServicesService;
        _logger = logger;

        var baseUrl = i14yOptions.Value.IriBaseUrl.TrimEnd('/');
        _baseDatasetUri = $"{baseUrl}/dataset/";
        _baseDataserviceUri = $"{baseUrl}/dataservice/";

        // Initialize graph and reusable nodes
        _graph = new Graph();

    }

    public async Task<string> Handle(ExportDcatCatalogCommand request, CancellationToken cancellationToken)
    {
        _graph.BaseUri = new Uri($"{BaseUriString}{request.Id}");
        _catalog = _graph.CreateUriNode(new Uri(string.Empty, UriKind.Relative));
        _rdfType = _graph.CreateUriNode("rdf:type");

        // namespaces
        _graph.NamespaceMap.AddNamespace("dcat", new Uri(DcatNamespace));
        _graph.NamespaceMap.AddNamespace("vcard", new Uri(VcardNamespace));
        _graph.NamespaceMap.AddNamespace("dct", new Uri(DublinCoreNamespace));
        _graph.NamespaceMap.AddNamespace("foaf", new Uri(FoafNamespace));
        _graph.NamespaceMap.AddNamespace("rdfs", new Uri(RdfSchemaNamespace));
        _graph.NamespaceMap.AddNamespace("spdx", new Uri(SpdxNamespace));
        _graph.NamespaceMap.AddNamespace("dcatap", new Uri(DcatApNamespace));
        _graph.NamespaceMap.AddNamespace("schema", new Uri(SchemaOrgNamespace));

        var dcatCatalog = await _dcatCatalogsService.GetDcatCatalog(request.Id, cancellationToken);

        AddDcatCatalog(dcatCatalog);

        var dcatCatalogRecords = await _dcatCatalogsService.GetDcatCatalogRecords(
            request.Id,
            page: 1,
            pageSize: int.MaxValue,
            cancellationToken);

        foreach (var item in dcatCatalogRecords.Results)
        {
            AddDcatCatalogRecord(item);
        }

        return await WriteOutput(request.Format, cancellationToken);
    }

    private void AddDcatCatalog(DcatCatalogModel dcatCatalog)
    {
        var catalogType = _graph.CreateUriNode("dcat:Catalog");
        _graph.Assert(_catalog, _rdfType, catalogType);

        var publisher = _graph.CreateBlankNode();
        _graph.Assert(_catalog, "dct:publisher", publisher);
        _graph.AssertUri(publisher, _rdfType, "foaf:Agent");
        _graph.Assert(_catalog, "foaf:name", dcatCatalog.Publisher.Name);

        _graph.Assert(_catalog, "dct:title", dcatCatalog.Title);

        _graph.Assert(_catalog, "dct:description", dcatCatalog.Description);
    }

    private void AddDcatCatalogRecord(DcatCatalogRecordModel record)
    {
        if (record.PrimaryTopic is null)
        {
            return;
        }

        var id = record.PrimaryTopic.ResourceId;
        var dcatThemes = record.Themes;

        switch (record.PrimaryTopic.ResourceType)
        {
            case DcatCatalogType.Dataset:
                {
                    AddDataset(id, dcatThemes);
                    break;
                }

            case DcatCatalogType.DataService:
                {
                    AddDataService(id, dcatThemes);
                    break;
                }

            default: throw new NotSupportedException($"The resource type '{record.PrimaryTopic.ResourceType}' is not supported.");
        }
    }

    private void AddDataService(Guid dataServiceId, IEnumerable<DcatCatalogThemeModel> dcatCatalogThemes)
    {
        var isReadable = _dataServicesService
            .GetUserAllowActionInfo(dataServiceId)
            .GetAwaiter()
            .GetResult()
            .Single(x => x.ActionType == AllowActionType.Read)
            .Value;

        if (!isReadable)
        {
            return;
        }

        var dataService = _dataServicesService.GetDataService(dataServiceId).GetAwaiter().GetResult();

        var dataServiceUri = _graph.CreateUriNode(new Uri($"{_baseDataserviceUri}{dataService.Id}", UriKind.Absolute));
        _graph.Assert(_catalog, "dcat:service", dataServiceUri);
        _graph.AssertUri(dataServiceUri, _rdfType, "dcat:DataService");

        _graph.Assert(dataServiceUri, "dcat:accessRights", GetCorrectUri(dataService.AccessRights.Uri));

        foreach (var vCard in dataService.ContactPoints)
        {
            AssertVCard(dataServiceUri, vCard);
        }

        AssertResources(dataServiceUri, "foaf:page", dataService.Documentation, "foaf:Document");

        foreach (var endPointDescription in dataService.EndpointDescriptions)
        {
            var url = EnsureUrl(endPointDescription.Uri, $" used as a dcat:EndPointDescription");
            var hrefNode = _graph.CreateUriNode(url);
            _graph.Assert(dataServiceUri, "dcat:EndPointDescription", hrefNode);
        }

        AssertResources(dataServiceUri, "dcat:endpointURL", dataService.EndpointUrls, "rdfs:Resource");

        _graph.Assert(dataServiceUri, "dcat:keyword", dataService.Keywords.Where(x => x.Label is not null).Select(x => x.Label!));

        if (dataService.LandingPages.Any())
        {
            // First only
            AssertResources(dataServiceUri, "dcat:landingPage", dataService.LandingPages.Take(1), "foaf:Document");
        }

        if (dataService.License is not null)
        {
            _graph.Assert(dataServiceUri, "dct:license", GetCorrectUri(dataService.License.Uri));
        }

        var publisher = _graph.CreateBlankNode();
        _graph.Assert(dataServiceUri, "dct:publisher", publisher);
        _graph.AssertUri(publisher, _rdfType, "foaf:Organization");
        _graph.Assert(publisher, "foaf:name", dataService.Publisher.Name);

        _graph.Assert(dataServiceUri, "dct:title", dataService.Title);

        // datasets
        foreach (var servesDataset in dataService.ServesDatasets)
        {
            var isDatasetReadable = _datasetsService
                .GetUserAllowActionInfo(servesDataset.Id)
                .GetAwaiter()
                .GetResult()
                .Single(x => x.ActionType is AllowActionType.Read)
                .Value;

            if (isDatasetReadable)
            {
                var dataset = _datasetsService.GetDataset(servesDataset.Id).GetAwaiter().GetResult();

                var datasetUri = _graph.CreateUriNode(new Uri($"{_baseDatasetUri}{dataset.Identifiers.First()}", UriKind.Absolute));
                _graph.Assert(dataServiceUri, "dcat:servesDataset", datasetUri);
                _graph.AssertUri(datasetUri, _rdfType, "dcat:Dataset");
            }
        }

        foreach (var catalogTheme in dcatCatalogThemes)
        {
            _graph.Assert(dataServiceUri, "dcat:theme", GetCorrectUri(catalogTheme.Uri));
        }
    }

    private void AddDataset(Guid datasetId, IEnumerable<DcatCatalogThemeModel> dcatCatalogThemes)
    {
        var isReadable = _datasetsService
            .GetUserAllowActionInfo(datasetId)
            .GetAwaiter()
            .GetResult()
            .Single(x => x.ActionType == AllowActionType.Read)
            .Value;

        if (!isReadable)
        {
            return;
        }

        var dataset = _datasetsService.GetDataset(datasetId).GetAwaiter().GetResult();

        var datasetUri = _graph.CreateUriNode(new Uri($"{_baseDatasetUri}{dataset.Identifiers.First()}", UriKind.Absolute));

        _graph.Assert(_catalog, "dcat:dataset", datasetUri);
        _graph.AssertUri(datasetUri, _rdfType, "dcat:Dataset");
        _graph.Assert(datasetUri, _rdfType, _graph.CreateUriNode(new Uri(DatasetNamespace)));
        _graph.Assert(datasetUri, "dcat:accessRights", GetCorrectUri(dataset.AccessRights.Uri));

        AssertResources(datasetUri, "dct:conformsTo", dataset.ConformsTo, "dct:standard");

        foreach (var vCard in dataset.ContactPoints)
        {
            AssertVCard(datasetUri, vCard);
        }

        _graph.Assert(datasetUri, "dct:description", dataset.Description);

        AssertResources(datasetUri, "foaf:page", dataset.Documentation, "foaf:Document");

        if (dataset.Frequency is not null)
        {
            _graph.Assert(datasetUri, "dct:accrualPeriodicity", GetCorrectUri(dataset.Frequency.Uri));
        }

        _graph.AssertLiteral(datasetUri, "dct:identifier", dataset.Identifiers.First());
        AssertResources(datasetUri, "schema:image", dataset.Images, "schema:url");
        AssertResources(datasetUri, "dct:isReferencedBy", dataset.IsReferencedBy, "rdfs:Resource");
        AssertResources(datasetUri, "dct:relation", dataset.Relations, "rdfs:Resource");
        _graph.Assert(datasetUri, "dct:issued", dataset.Issued);
        _graph.Assert(datasetUri, "dcat:keyword", dataset.Keywords.Where(x => x.Label is not null).Select(x => x.Label!));

        if (dataset.LandingPages.Any())
        {
            // First only
            AssertResources(datasetUri, "dcat:landingPage", [dataset.LandingPages.First()], "foaf:Document");
        }

        foreach (var language in dataset.Languages)
        {
            _graph.AssertLiteral(datasetUri, "dct:language", language.Code);
        }

        _graph.Assert(datasetUri, "dct:modified", dataset.Modified);

        var publisher = _graph.CreateBlankNode();
        _graph.Assert(datasetUri, "dct:publisher", publisher);
        _graph.AssertUri(publisher, _rdfType, "foaf:Agent");
        _graph.Assert(publisher, "foaf:name", dataset.Publisher.Name);

        AssertQualifiedAttributions(dataset.QualifiedAttributions, datasetUri);

        AssertQualifiedRelations(dataset.QualifiedRelations, datasetUri);

        foreach (var spatial in dataset.Spatial)
        {
            _graph.AssertLiteral(datasetUri, "dct:spatial", spatial);
        }

        if (dataset.TemporalCoverage.Any())
        {
            // First only
            var temporalCoverage = _graph.CreateBlankNode();
            _graph.Assert(datasetUri, "dct:temporal", temporalCoverage);
            _graph.AssertUri(temporalCoverage, _rdfType, "dct:PeriodOfTime");
            var periodOfTime = dataset.TemporalCoverage.First();
            _graph.Assert(temporalCoverage, "schema:startDate", periodOfTime.Start);
            _graph.Assert(temporalCoverage, "schema:endDate", periodOfTime.End);
        }

        _graph.Assert(datasetUri, "dct:title", dataset.Title);

        foreach (var catalogTheme in dcatCatalogThemes)
        {
            _graph.Assert(datasetUri, "dcat:theme", GetCorrectUri(catalogTheme.Uri));
        }

        // Distributions
        foreach (var distribution in dataset.Distributions)
        {
            var distributionUriString = $"catalog/datasets/{dataset.Identifiers.First()}/distributions/{distribution.Id}";
            var distributionUri = _graph.CreateBlankNode(distributionUriString);

            // Assertion für die Beziehung zwischen Dataset und Distribution
            _graph.Assert(datasetUri, "dcat:distribution", distributionUri);

            AddDistribution(distribution, distributionUri);
        }
    }

    private void AddDistribution(DcatDistributionModel distribution, INode distributionUri)
    {
        _graph.AssertUri(distributionUri, _rdfType, "dcat:Distribution");

        AssertResources(distributionUri, "dcat:accessURL", [distribution.AccessUrl], "rdfs:Resource");

        if (distribution.Availability is not null)
        {
            _graph.Assert(distributionUri, "dcatap:availability", GetCorrectUri(distribution.Availability.Uri));
        }

        if (distribution.ByteSize.HasValue)
        {
            _graph.Assert(distributionUri, "dcat:byteSize", _graph.CreateLiteralNode(distribution.ByteSize.ToString(), new Uri(DecimalNamespace)));
        }

        if (!string.IsNullOrWhiteSpace(distribution.Rights))
        {
            _graph.AssertLiteral(distributionUri, "dct:rights", distribution.Rights);
        }

        if (distribution.Checksum is not null)
        {
            var checksum = _graph.CreateBlankNode();
            _graph.AssertUri(checksum, _rdfType, "spdx:Checksum");
            _graph.Assert(checksum, "spdx:algorithm", GetCorrectUri(distribution.Checksum.Algorithm.Uri));
            _graph.AssertLiteral(checksum, "spdx:checksumValue", distribution.Checksum.ChecksumValue);
            _graph.Assert(distributionUri, "spdx:checksum", checksum);
        }

        AssertResources(distributionUri, "dct:conformsTo", distribution.ConformsTo, "dct:standard");

        // NOTE: specification unclear, incompatibility with opendata.swiss (opendata.swiss uses Location 1..1, I14Y uses Period 1..n from/to, specification says LocationPeriodOrJurisdiction 0..n)
        // Command from Thomas: Deactivate mapping to RDF in the code, with a note.
        // NOTE 17.06.2026 : opendata.swiss needs dct:coverage, they want the start date as coverage (see https://github.com/I14Y-ch/planning/issues/696)

        //foreach (var coverage in (entity.Coverage ?? Array.Empty<string>()).Where(x => !string.IsNullOrWhiteSpace(x)))
        //{
        //    _graph.AssertLiteral(distributionUri, "dct:coverage", coverage);
        //}

        if (distribution.Coverage.Select(c => c.Start).FirstOrDefault(s => s.HasValue) is DateTimeOffset start)
        {
            _graph.AssertLiteral(
                distributionUri,
                "dct:coverage",
                start.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        }

        _graph.Assert(distributionUri, "dct:description", distribution.Description);

        AssertResources(distributionUri, "foaf:page", distribution.Documentation, "foaf:Document");

        if (distribution.DownloadUrl is not null)
        {
            AssertResources(distributionUri, "dcat:downloadURL", [distribution.DownloadUrl], "rdfs:Resource");
        }

        if (distribution.Format is not null)
        {
            _graph.Assert(distributionUri, "dct:format", GetCorrectUri(distribution.Format.Uri));
        }

        if (!string.IsNullOrWhiteSpace(distribution.Identifier))
        {
            _graph.AssertLiteral(distributionUri, "dct:identifier", distribution.Identifier);
        }

        AssertResources(distributionUri, "schema:image", distribution.Images, "schema:url");

        foreach (var language in distribution.Languages)
        {
            _graph.AssertLiteral(distributionUri, "dct:language", language.Code);
        }

        if (distribution.License is not null)
        {
            _graph.Assert(distributionUri, "dct:license", GetCorrectUri(distribution.License.Uri));
        }

        if (distribution.MediaType is not null)
        {
            _graph.Assert(distributionUri, "dcat:mediaType", GetCorrectUri(distribution.MediaType.Uri));
        }

        _graph.Assert(distributionUri, "dct:modified", distribution.Modified);

        if (distribution.PackagingFormat is not null)
        {
            _graph.Assert(distributionUri, "dcat:packageFormat", GetCorrectUri(distribution.PackagingFormat.Uri));

        }

        _graph.Assert(distributionUri, "dct:issued", distribution.Issued);

        if (!string.IsNullOrWhiteSpace(distribution.TemporalResolution))
        {
            _graph.AssertLiteral(distributionUri, "dcat:temporalResolution", distribution.TemporalResolution);
        }

        _graph.Assert(distributionUri, "dct:title", distribution.Title);

        foreach (var dataService in distribution.AccessServices)
        {
            var isDataServiceReadable = _dataServicesService
                .GetUserAllowActionInfo(dataService.Id)
                .GetAwaiter()
                .GetResult()
                .Single(x => x.ActionType is AllowActionType.Read)
                .Value;

            if (isDataServiceReadable)
            {
                var dataServiceUri = _graph.CreateUriNode(new Uri($"{_baseDataserviceUri}{dataService.Id}", UriKind.Absolute));
                _graph.Assert(distributionUri, "dcat:accessService", dataServiceUri);
            }
        }
    }

    private IUriNode GetCorrectUri(string? uri)
    {
        var correctUri = string.IsNullOrWhiteSpace(uri)
            ? InvalidUriReplacementNamespace
            : uri;

        var result = new Uri(correctUri, UriKind.Absolute);
        return _graph.CreateUriNode(result);
    }

    /// <summary>
    /// Make sure the provided string corresponds to a URL or replace it with a standard replacement Url
    /// </summary>
    private Uri EnsureUrl(string sourceUrl, string? additionalLogInfo = null)
    {
        if (Uri.TryCreate(sourceUrl, UriKind.Absolute, out Uri? uri))
        {
            if (_allowedUriSchemes.Contains(uri.Scheme))
            {
                return uri;
            }
            else
            {
                _logger.LogWarning("The scheme of '{url}' is not allowed {additionalInfo}.", sourceUrl, additionalLogInfo);
                return _invalidUriReplacement;
            }
        }
        _logger.LogWarning("'{url}' is not a valid URL{additionalInfo}.", sourceUrl, additionalLogInfo);
        return _invalidUriReplacement;
    }

    /// <summary>
    /// Assert subj - pred - resource.href and resource.href - rdftype - type
    /// </summary>
    private void AssertResources(INode subj, string pred, IEnumerable<ResourceModel> resources, string rdfTypeObj)
    {
        foreach (var resource in resources)
        {
            try
            {
                var url = EnsureUrl(resource.Uri, $" used as a {pred} of type {rdfTypeObj}");
                var hrefNode = _graph.CreateUriNode(url);
                _graph.Assert(subj, pred, hrefNode);
                _graph.AssertUri(hrefNode, _rdfType, rdfTypeObj);
            }
            catch (UriFormatException)
            {
                _logger.LogError("Uri format exception for Resource.HRef {property} - value '{href}'", pred, resource.Uri);
            }
        }
    }

    private void AssertVCard(INode subj, VCardModel vcard)
    {
        var details = _graph.CreateBlankNode();
        _graph.Assert(subj, "dcat:contactPoint", details);
        _graph.AssertUri(details, _rdfType, "vcard:Organization");

        if (vcard.Fn is not null)
        {
            _graph.Assert(details, "vcard:fn", vcard.Fn);
        }

        if (vcard.HasAddress is not null)
        {
            _graph.Assert(details, "vcard:adrWork", vcard.HasAddress);
        }

        if (vcard.Note is not null)
        {
            _graph.Assert(details, "vcard:note", vcard.Note);
        }

        _graph.AssertLiteral(details, "vcard:hasEmail", vcard.HasEmail);

        if (!string.IsNullOrWhiteSpace(vcard.HasTelephone))
        {
            _graph.AssertLiteral(details, "vcard:hasTelephone", vcard.HasTelephone);
        }
    }

    private void AssertQualifiedAttributions(IEnumerable<DcatQualifiedAttributionModel> list, INode subj)
    {
        foreach (var attribution in list)
        {
            var qualifiedAttribution = _graph.CreateBlankNode();
            _graph.Assert(subj, "dcat:qualifiedAttribution", qualifiedAttribution);
            _graph.AssertUri(qualifiedAttribution, _rdfType, "dcat:Attribution");

            _graph.AssertLiteral(qualifiedAttribution, "dcat:agent", attribution.Agent.Identifier);

            if (attribution.HadRole is not null)
            {
                _graph.Assert(qualifiedAttribution, "dcat:hadRole", GetCorrectUri(attribution.HadRole.Uri));
            }
        }
    }

    private void AssertQualifiedRelations(IEnumerable<DcatQualifiedRelationModel> list, INode subj)
    {
        foreach (var relation in list)
        {
            var qualifiedRelation = _graph.CreateBlankNode();
            _graph.Assert(subj, "dcat:qualifiedRelation", qualifiedRelation);
            _graph.AssertUri(qualifiedRelation, _rdfType, "dcat:Relationship");

            var node = _graph.CreateUriNode(new Uri(relation.Relation.Uri));
            _graph.Assert(qualifiedRelation, "dct:relation", node);

            _graph.Assert(qualifiedRelation, "dcat:hadRole", GetCorrectUri(relation.HadRole.Uri));
        }
    }

    private async Task<string> WriteOutput(CatalogExportFormat format, CancellationToken cancellationToken)
    {
        // Creating a StreamWriter with UTF-8 encoding:
        // As StringWriter does not offer the option of specifying the encoding directly (it uses UTF-16 by default, as this is the internal representation of .NET strings)
        using var memoryStream = new MemoryStream();
        using (var streamWriter = new StreamWriter(memoryStream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), leaveOpen: true))
        {
            switch (format)
            {
                case CatalogExportFormat.RDF:
                    var rdfXmlWriter = new PrettyRdfXmlWriter() { PrettyPrintMode = true };
                    rdfXmlWriter.Save(_graph, streamWriter);
                    break;

                case CatalogExportFormat.TTL:
                    var ttlWriter = new CompressingTurtleWriter();
                    ttlWriter.Save(_graph, streamWriter);
                    break;

                default:
                    throw new NotSupportedException($"The format '{format}' is not supported.");
            }

            streamWriter.Flush();
        }

        memoryStream.Seek(0, SeekOrigin.Begin);

        using var streamReader = new StreamReader(memoryStream, Encoding.UTF8);
        var result = await streamReader.ReadToEndAsync(cancellationToken);
        result = result.Replace("encoding=\"utf-16\"", "encoding=\"utf-8\"", StringComparison.OrdinalIgnoreCase);
        return result;
    }
}
