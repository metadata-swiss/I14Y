using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Models.Search.Filters;
using Bfs.Iop.Core.Lucene.Index.Analyzers;
using Bfs.Iop.Core.Lucene.Index.Extensions;
using Bfs.Iop.Core.Lucene.Index.Parsers;
using Bfs.Iop.Core.Lucene.Search;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.Infrastructure.Security;
using Bfs.Iop.Infrastructure.Security.Services;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Miscellaneous;
using Lucene.Net.Documents;
using Lucene.Net.Documents.Extensions;
using Lucene.Net.Facet;
using Lucene.Net.Facet.Taxonomy.Directory;
using Lucene.Net.Index;
using Lucene.Net.Queries;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace Bfs.Iop.Core.Lucene.Index;

internal sealed class CatalogIndexService : ICatalogIndexService, IDisposable
{
    private const LuceneVersion AppLuceneVersion = LuceneVersion.LUCENE_48;
    private const int MaxResults = 100000;

    private readonly IUserContextService _userContextService;
    private readonly ILogger<CatalogIndexService> _logger;
    private readonly Analyzer _analyzer;
    private readonly global::Lucene.Net.Store.Directory _indexDirectory;
    private readonly global::Lucene.Net.Store.Directory _taxonomyDirectory;
    private readonly FacetsConfig _facetsConfig;

    private readonly Dictionary<string, Dictionary<string, string>> _fieldsByAttributeAndLanguages;

    private readonly SemaphoreSlim _writeGate = new(1, 1);

    private static readonly string[] _multiLanguageTextProperties =
        [
            LuceneFields.Catalog.Description,
            LuceneFields.Catalog.DistributionDescription,
            LuceneFields.Catalog.DistributionTitle,
            LuceneFields.Catalog.Keyword,
            LuceneFields.Catalog.Name,
            LuceneFields.Catalog.Title,
            LuceneFields.Catalog.ContactPointFn,
            LuceneFields.Catalog.ContactPointHasAddress,
            LuceneFields.Catalog.ContactPointNote
        ];

    private static readonly string[] _textProperties =
        [
            LuceneFields.Catalog.DataOwner,
            LuceneFields.RawField(LuceneFields.Catalog.DataOwner),
            LuceneFields.Catalog.DistributionIdentifier,
            LuceneFields.Catalog.Identifier,
            LuceneFields.RawField(LuceneFields.Catalog.Identifier),
            LuceneFields.Catalog.ResponsibleDeputyEmail,
            LuceneFields.Catalog.ResponsibleDeputyFamilyName,
            LuceneFields.Catalog.ResponsibleDeputyGivenName,
            LuceneFields.Catalog.ResponsiblePersonEmail,
            LuceneFields.Catalog.ResponsiblePersonFamilyName,
            LuceneFields.Catalog.ResponsiblePersonGivenName,
            LuceneFields.Catalog.Version,
            LuceneFields.Catalog.ContactPointHasEmail,
            LuceneFields.Catalog.ContactPointHasTelephone,
            LuceneFields.Catalog.Id,
            LuceneFields.RawField(LuceneFields.Catalog.Id),
            LuceneFields.Catalog.DataServiceIdentifier
        ];

    // Fields holding an email address, indexed as a single exact (keyword) token.
    private static readonly string[] _emailFields =
        [
            LuceneFields.Catalog.ResponsiblePersonEmail,
            LuceneFields.Catalog.ResponsibleDeputyEmail,
            LuceneFields.Catalog.ContactPointHasEmail
        ];

    // Matches a query consisting of a single email address (no surrounding whitespace/quotes).
    private static readonly Regex _emailQueryRegex = new(
        @"^[^\s@""]+@[^\s@""]+\.[^\s@""]+$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly string[] _languages =
        [
            "de",
            "en",
            "fr",
            "it",
            "rm"
        ];

    public CatalogIndexService(
        IUserContextService userContextService,
        ILogger<CatalogIndexService> logger,
        IConfiguration configuration)
    {
        _userContextService = userContextService;
        _logger = logger;

        _fieldsByAttributeAndLanguages = _multiLanguageTextProperties
            .ToDictionary(
                x => x, x => _languages
                    .ToDictionary(y => y, y => $"{x}_{y}", StringComparer.InvariantCultureIgnoreCase),
            StringComparer.InvariantCultureIgnoreCase);

        var _analyzerByFields = _fieldsByAttributeAndLanguages
            .SelectMany(x => x.Value)
            .ToDictionary(x => x.Value, x => new LanguageDependentAnalyzer(AppLuceneVersion, x.Key));

        _analyzer = new PerFieldAnalyzerWrapper(
            new KeywordAnalyzer(),
            _analyzerByFields.ToDictionary(x => x.Key, x => x.Value as Analyzer));

        var useRam = bool.TryParse(configuration["Lucene:UseRamDirectory"], out var useRamValue)
             && useRamValue;

        if (useRam)
        {
            _indexDirectory = new RAMDirectory();
            _taxonomyDirectory = new RAMDirectory();
        }
        else
        {
            var indexPath = configuration["Lucene:CatalogDirectory"] ?? "lucene-catalog-index";
            var taxonomyPath = configuration["Lucene:TaxonomyDirectory"] ?? "lucene-taxonomy-index";

            _indexDirectory = FSDirectory.Open(new DirectoryInfo(indexPath));
            _taxonomyDirectory = FSDirectory.Open(new DirectoryInfo(taxonomyPath));
        }

        _facetsConfig = new FacetsConfig();
        _facetsConfig.SetRequireDimCount(LuceneFields.Catalog.AccessRights, true);
        _facetsConfig.SetMultiValued(LuceneFields.Catalog.BusinessEvents, true);
        _facetsConfig.SetRequireDimCount(LuceneFields.Catalog.BusinessEvents, true);
        _facetsConfig.SetMultiValued(LuceneFields.Catalog.Formats, true);
        _facetsConfig.SetRequireDimCount(LuceneFields.Catalog.Formats, true);
        _facetsConfig.SetMultiValued(LuceneFields.Catalog.LifeEvents, true);
        _facetsConfig.SetRequireDimCount(LuceneFields.Catalog.LifeEvents, true);
        _facetsConfig.SetRequireDimCount(LuceneFields.Catalog.Publisher, true);
        _facetsConfig.SetRequireDimCount(LuceneFields.Catalog.PublisherIdentifier, true);
        _facetsConfig.SetMultiValued(LuceneFields.Catalog.QualifiedAttributionAgentIdentifier, true);
        _facetsConfig.SetRequireDimCount(LuceneFields.Catalog.QualifiedAttributionAgentIdentifier, true);
        _facetsConfig.SetMultiValued(LuceneFields.Catalog.Themes, true);
        _facetsConfig.SetRequireDimCount(LuceneFields.Catalog.Themes, true);
        _facetsConfig.SetRequireDimCount(LuceneFields.Catalog.Type, true);
    }

    public void UpdateIndex(DcatDatasetModel model, bool? hasStructure = null)
    {
        ArgumentNullException.ThrowIfNull(model, nameof(model));

        Dictionary<Guid, Document> documents = [];

        var hasFile = hasStructure ?? GetDatasetHasStructureValueFromIndex(model.Id);

        documents.Add(model.Id, GetDocumentFromDatasetModel(model, hasFile));

        IndexDocuments(documents);
    }

    public void UpdateIndex(IEnumerable<DcatDatasetModel> models, IEnumerable<string> datasetsIds)
    {
        ArgumentNullException.ThrowIfNull(models, nameof(models));
        ArgumentNullException.ThrowIfNull(datasetsIds, nameof(datasetsIds));

        Dictionary<Guid, Document> documents = [];

        foreach (var item in models)
        {
            try
            {
                documents.Add(item.Id, GetDocumentFromDatasetModel(item, datasetsIds.Any(x => x.StartsWith(item.Id.ToString()))));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "The object of the type '{Type}' with the id '{Id}' could not be indexed.", item.GetType().Name, item.Id);
            }
        }

        IndexDocuments(documents);
    }

    public void UpdateIndex(params PublicServiceModel[] models) =>
        UpdateIndex<PublicServiceModel>(models);

    public void UpdateIndex(params DataServiceModel[] models) => 
        UpdateIndex<DataServiceModel>(models);

    public void UpdateIndex(params IopConceptModel[] models) => 
        UpdateIndex<IopConceptModel>(models);

    public void UpdateIndex(params MappingTableModel[] models) =>
        UpdateIndex<MappingTableModel>(models);

    public void DeIndex(params Guid[] ids)
    {
        ArgumentNullException.ThrowIfNull(ids, nameof(ids));

        _writeGate.Wait();

        try
        {
            using var indexWriter = new IndexWriter(_indexDirectory, new IndexWriterConfig(AppLuceneVersion, _analyzer));
            using var taxonomyWriter = new DirectoryTaxonomyWriter(_taxonomyDirectory);

            foreach (var id in ids)
            {
                indexWriter.DeleteDocuments(new Term(LuceneFields.Catalog.Id, GuidToString(id)));
            }
            indexWriter.Flush(true, true);
        }
        finally
        {
            _writeGate.Release();
        }
    }

    public void Dispose()
    {
        _indexDirectory.Dispose();
        _analyzer.Dispose();
    }

    public PagedResult<CatalogSearchResultEntry> Search(
        string? queryString,
        string? language,
        CatalogSearchFilter? searchFilter,
        int page,
        int pageSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(page, nameof(page));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize, nameof(pageSize));

        using var reader = DirectoryReader.Open(_indexDirectory);
        var searcher = new IndexSearcher(reader);
        var collector = TopScoreDocCollector.Create(MaxResults, true);
        var facetCollector = new FacetsCollector();

        var query = BuildSearchQuery(queryString, language is null ? _languages : [language], searchFilter);

        if (!string.IsNullOrWhiteSpace(queryString))
            query = new RegistrationStatusBoostQuery(query);

        var filter = TryGetUsersAuthorizationFilter();
        searcher.Search(query, filter, MultiCollector.Wrap([collector, facetCollector]));

        var topDocs = collector.GetTopDocs((page - 1) * pageSize, pageSize);

        var results = new List<CatalogSearchResultEntry>();

        foreach (var item in topDocs.ScoreDocs)
        {
            var doc = searcher.Doc(item.Doc);

            results.Add(new CatalogSearchResultEntry()
            {
                AccessRights = doc.GetField(LuceneFields.Catalog.AccessRights)?.GetStringValue(),
                BusinessEvents = doc.GetFields(LuceneFields.Catalog.BusinessEvents).Select(x => x.GetStringValue()),
                ConceptType = (ConceptType?)doc.GetField(LuceneFields.Catalog.ConceptType)?.GetInt32Value(),
                CreatedAt = DateTimeOffset.Parse(doc.GetField(LuceneFields.Catalog.CreatedAt).GetStringValue()),
                CreationType = doc.GetField(LuceneFields.Catalog.CreationType)?.GetInt32Value() is int creationType
                    ? (CreationType)creationType
                    : null,
                Description = MultiLanguageModel.FromDictionary(_fieldsByAttributeAndLanguages[LuceneFields.Catalog.Description].ToDictionary(x => x.Key, x => doc.GetField(x.Value)?.GetStringValue())),
                Formats = doc.GetFields(LuceneFields.Catalog.Formats).Select(x => x.GetStringValue()),
                HasStructure = doc.GetField(LuceneFields.Catalog.HasStructure) is not null
                    ? bool.Parse(doc.GetField(LuceneFields.Catalog.HasStructure).GetStringValue())
                    : null,
                Id = Guid.Parse(doc.GetField(LuceneFields.Catalog.Id).GetStringValue()),
                Identifier = doc.GetField(LuceneFields.OriginalField(LuceneFields.Catalog.Identifier)).GetStringValue(),
                LifeEvents = doc.GetFields(LuceneFields.Catalog.LifeEvents).Select(x => x.GetStringValue()),
                ModifiedAt = DateTimeOffset.TryParse(doc.GetField(LuceneFields.Catalog.ModifiedAt)?.GetStringValue(), out var modifiedAt)
                    ? modifiedAt
                    : null,
                PublicationLevel = (PublicationLevel)doc.GetField(LuceneFields.Catalog.PublicationLevel).GetInt32Value()!,
                PublicationLevelProposal = doc.GetField(LuceneFields.Catalog.PublicationLevelProposal)?.GetInt32Value() is int value1 &&
                    Enum.IsDefined(typeof(PublicationLevel), value1)
                        ? (PublicationLevel)value1
                        : null,
                Publisher = Guid.Parse(doc.GetField(LuceneFields.Catalog.Publisher).GetStringValue()),
                PublisherIdentifier = doc.GetField(LuceneFields.Catalog.PublisherIdentifier).GetStringValue(),
                RegistrationStatus = (RegistrationStatus)doc.GetField(LuceneFields.Catalog.RegistrationStatus).GetInt32Value()!,
                RegistrationStatusProposal = doc.GetField(LuceneFields.Catalog.RegistrationStatusProposal)?.GetInt32Value() is int value2 &&
                    Enum.IsDefined(typeof(RegistrationStatus), value2)
                        ? (RegistrationStatus)value2
                        : null,
                Themes = doc.GetFields(LuceneFields.Catalog.Themes).Select(x => x.GetStringValue()),
                Title = MultiLanguageModel.FromDictionary(_fieldsByAttributeAndLanguages[LuceneFields.Catalog.Title].ToDictionary(x => x.Key, x => doc.GetField(x.Value)?.GetStringValue())),
                Type = Enum.Parse<SearchResourceType>(doc.GetField(LuceneFields.Catalog.Type).GetStringValue()),
                ValidFrom = DateTimeOffset.TryParse(doc.GetField(LuceneFields.Catalog.ValidFrom)?.GetStringValue(), out var validFrom) 
                    ? validFrom 
                    : null,
                ValidTo = DateTimeOffset.TryParse(doc.GetField(LuceneFields.Catalog.ValidTo)?.GetStringValue(), out var validTo) 
                    ? validTo 
                    : null,
                Version = doc.GetField(LuceneFields.Catalog.Version)?.GetStringValue()
            });
        }

        return new PagedResult<CatalogSearchResultEntry>()
        {
            Page = page,
            PageSize = pageSize is int.MaxValue ? collector.TotalHits : pageSize,
            Results = results,
            TotalCount = collector.TotalHits
        };
    }

    public IEnumerable<CatalogSearchCountResultEntry> SearchCount(string? queryString, string? language, CatalogSearchFilter? searchFilter)
    {
        // Check if the taxonomy index exists (happens on empty DBs)
        if (!DirectoryReader.IndexExists(_taxonomyDirectory))
        {
            using var taxonomyWriter = new DirectoryTaxonomyWriter(_taxonomyDirectory);
            taxonomyWriter.Commit();
        }

        using var reader = DirectoryReader.Open(_indexDirectory);
        var searcher = new IndexSearcher(reader);

        var drillDownQuery = BuildCountDrillDownQuery(queryString, language is null ? _languages : [language], searchFilter);
        var filter = TryGetUsersAuthorizationFilter();

        using var taxonomyReader = new DirectoryTaxonomyReader(_taxonomyDirectory);

        // DrillSideways computes, for each filter category that has a selection, the facet counts
        // as if that category's own drill-down were removed (OR within the category), while keeping
        // the other categories applied (AND across categories). This mirrors the result list and
        // ensures every filter always returns counts for all of its values, so selecting one value
        // no longer collapses the others to zero in the filter dropdowns.
        var drillSideways = new DrillSideways(searcher, _facetsConfig, taxonomyReader);
        var result = drillSideways.Search(drillDownQuery, filter, null, 1, null, false, false);

        // The true total must come from the actual query hits, not from a facet dimension: under
        // DrillSideways a drilled-down dimension's total reflects the sideways (own-filter-removed)
        // set, which would over-report the filtered result count.
        var totalDocumentsCount = result.Hits?.TotalHits ?? 0;

        if (result.Facets is null)
        {
            return [];
        }

        // DrillSideways returns a MultiFacets whose GetAllDims can contain null entries: a drilled-down
        // dimension (e.g. the publisher filter) whose sideways set matches no documents yields a null
        // FacetResult (unlike the default FastTaxonomyFacetCounts, which skips empty dimensions). This
        // happens when a filter is combined with a query term that narrows the results to nothing, so
        // filter the nulls out before projecting.
        return result.Facets.GetAllDims(int.MaxValue)
            .Where(x => x is not null)
            .Select(x => new CatalogSearchCountResultEntry()
            {
                Identifier = x.Dim,
                TotalDocumentsCount = totalDocumentsCount,
                CountByValues = x.LabelValues
                    .ToDictionary(y => y.Label.Equals("_") ? string.Empty : y.Label, y => (int)y.Value)
                    .AsReadOnly()
            });
    }

    private void UpdateIndex<T>(params T[] models) where T : class, IPublishableEntityModel
    {
        ArgumentNullException.ThrowIfNull(models, nameof(models));

        Dictionary<Guid, Document> documents = [];

        foreach (var item in models)
        {
            try
            {
                var document = item switch
                {
                    DataServiceModel dataServiceModel => GetDocumentFromDataServiceModel(dataServiceModel),
                    IopConceptModel iopConceptModel => GetDocumentFromIopConceptModel(iopConceptModel),
                    PublicServiceModel publicServiceModel => GetDocumentFromPublicServiceModel(publicServiceModel),
                    MappingTableModel mappingTableModel => GetDocumentFromMappingTableModel(mappingTableModel),
                    _ => throw new NotSupportedException($"The type '{item.GetType()}' is not supported.")
                };

                documents.Add(item.Id, document);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "The object of the type '{Type}' with the id '{Id}' could not be indexed.", item.GetType().Name, item.Id);
            }
        }

        IndexDocuments(documents);
    }

    private bool GetDatasetHasStructureValueFromIndex(Guid datasetId)
    {
        using var reader = DirectoryReader.Open(_indexDirectory);
        var searcher = new IndexSearcher(reader);

        var query = new TermQuery(new Term(LuceneFields.Catalog.Id, GuidToString(datasetId)));

        var docs = searcher.Search(query, 1);

        if (docs.TotalHits > 0)
        {
            var doc = searcher.Doc(docs.ScoreDocs[0].Doc);
            var field = doc.Get(LuceneFields.Catalog.HasStructure);
            return bool.Parse(field);
        }

        return false;
    }

    private Document GetDocumentFromDataServiceModel(DataServiceModel dataServiceModel)
    {
        var document = BuildDocument(dataServiceModel, dataServiceModel.Identifiers.First());

        foreach (var identifier in dataServiceModel.Identifiers)
        {
            document.Add(new TextField(LuceneFields.Catalog.DataServiceIdentifier, identifier, Field.Store.YES));
        }

        // Version
        if (dataServiceModel.Version is not null)
        {
            document.Add(new TextField(LuceneFields.Catalog.Version, dataServiceModel.Version, Field.Store.YES));
        }

        // Access Rights
        document.AddStoredAndFacetField(LuceneFields.Catalog.AccessRights, dataServiceModel.AccessRights.Code);

        // Themes
        document.AddStoredAndFacetField(LuceneFields.Catalog.Themes, dataServiceModel.Themes.Select(x => x.Code));

        // Title
        FillDocumentWithMultiLanguageModelProperty(document, LuceneFields.Catalog.Title, dataServiceModel.Title, boostBase: 2.0f);

        // ResponsibleDeputy
        if (dataServiceModel.ResponsibleDeputy is not null)
        {
            FillDocumentWithIopPersonProperties(document, dataServiceModel.ResponsibleDeputy, LuceneFields.Catalog.ResponsibleDeputy);
        }

        // ResponsiblePerson
        if (dataServiceModel.ResponsiblePerson is not null)
        {
            FillDocumentWithIopPersonProperties(document, dataServiceModel.ResponsiblePerson, LuceneFields.Catalog.ResponsiblePerson);
        }

        // ContactPoints
        if (dataServiceModel.ContactPoints?.Any() ?? false)
        {
            FillDocumentWithContactPointsProperty(document, dataServiceModel.ContactPoints);
        }

        return document;
    }

    private Document GetDocumentFromPublicServiceModel(PublicServiceModel publicServiceModel)
    {
        var document = BuildDocument(publicServiceModel, publicServiceModel.Identifiers.First());

        // Themes
        document.AddStoredAndFacetField(LuceneFields.Catalog.Themes,
            publicServiceModel.ThematicAreas.Concat(publicServiceModel.Sectors).Select(x => x.Code));

        // Title
        FillDocumentWithMultiLanguageModelProperty(document, LuceneFields.Catalog.Title, publicServiceModel.Name, boostBase: 2.0f);

        // Business events
        document.AddStoredAndFacetField(LuceneFields.Catalog.BusinessEvents,
            publicServiceModel.BusinessEvents.Select(x => x.Code));

        // Life events
        document.AddStoredAndFacetField(LuceneFields.Catalog.LifeEvents,
            publicServiceModel.LifeEvents.Select(x => x.Code));

        // ResponsibleDeputy
        if (publicServiceModel.ResponsibleDeputy is not null)
        {
            FillDocumentWithIopPersonProperties(document, publicServiceModel.ResponsibleDeputy, LuceneFields.Catalog.ResponsibleDeputy);
        }

        // ResponsiblePerson
        if (publicServiceModel.ResponsiblePerson is not null)
        {
            FillDocumentWithIopPersonProperties(document, publicServiceModel.ResponsiblePerson, LuceneFields.Catalog.ResponsiblePerson);
        }

        // Contact emails from Channels
        foreach (var email in publicServiceModel.Channels
                     .Select(channel => channel.Email?.Trim())
                     .Where(email => !string.IsNullOrWhiteSpace(email)))
        {
            document.Add(new TextField(LuceneFields.Catalog.ContactPointHasEmail, email.ToLowerInvariant(), Field.Store.NO));
        }

        return document;
    }

    private Document GetDocumentFromIopConceptModel(IopConceptModel concept)
    {
        var document = BuildDocument(concept, concept.Identifiers.First());

        // Version
        if (concept.Version is not null)
        {
            document.Add(new TextField(LuceneFields.Catalog.Version, concept.Version, Field.Store.YES));
        }

        // Name
        FillDocumentWithMultiLanguageModelProperty(document, LuceneFields.Catalog.Name, concept.Name, boostBase: 2.0f);
        FillDocumentWithMultiLanguageModelProperty(document, LuceneFields.Catalog.Title, concept.Name, boostBase: 2.0f);

        // Themes
        document.AddStoredAndFacetField(LuceneFields.Catalog.Themes, concept.Themes.Select(x => x.Code));

        // ConceptType
        document.Add(new Int32Field(LuceneFields.Catalog.ConceptType, (int)concept.ConceptType, Field.Store.YES));
        document.Add(new FacetField(LuceneFields.Catalog.ConceptType, concept.ConceptType.ToString()));

        // Valid From
        if (concept.ValidFrom.HasValue)
        {
            document.Add(new StoredField(LuceneFields.Catalog.ValidFrom, concept.ValidFrom.ToString()));
        }

        // Valid To
        if (concept.ValidTo.HasValue)
        {
            document.Add(new StoredField(LuceneFields.Catalog.ValidTo, concept.ValidTo.ToString()));
        }

        // ResponsibleDeputy
        if (concept.ResponsibleDeputy is not null)
        {
            FillDocumentWithIopPersonProperties(document, concept.ResponsibleDeputy, LuceneFields.Catalog.ResponsibleDeputy);
        }

        // ResponsiblePerson
        if (concept.ResponsiblePerson is not null)
        {
            FillDocumentWithIopPersonProperties(document, concept.ResponsiblePerson, LuceneFields.Catalog.ResponsiblePerson);
        }

        return document;
    }

    private Document GetDocumentFromDatasetModel(DcatDatasetModel dataset, bool hasStructure)
    {
        var document = BuildDocument(dataset, dataset.Identifiers.First());

        // Access Rights
        document.AddStoredAndFacetField(LuceneFields.Catalog.AccessRights, dataset.AccessRights.Code);

        // Version
        if (dataset.Version is not null)
        {
            document.Add(new TextField(LuceneFields.Catalog.Version, dataset.Version, Field.Store.YES));
        }

        // Themes
        document.AddStoredAndFacetField(LuceneFields.Catalog.Themes, dataset.Themes.Select(x => x.Code));

        // Qualified Attributions
        document.AddStoredAndFacetField(
            LuceneFields.Catalog.QualifiedAttributionAgentIdentifier,
            dataset.QualifiedAttributions.Select(x => x.Agent.Identifier));

        // Formats
        var formats = dataset.Distributions
            .Select(x => x.Format)
            .Where(x => x is not null)
            .Select(x => x!.Code)
            .Distinct();

        document.AddStoredAndFacetField(LuceneFields.Catalog.Formats, formats);

        // Title
        FillDocumentWithMultiLanguageModelProperty(document, LuceneFields.Catalog.Title, dataset.Title, boostBase: 2.0f);

        // DataOwner
        if (!string.IsNullOrWhiteSpace(dataset.DataOwner))
        {
            var dataOwnerWords = dataset.DataOwner
                .Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(word => word.ToLowerInvariant());

            foreach (var dataOwnerWord in dataOwnerWords)
            {
                document.Add(new TextField(LuceneFields.Catalog.DataOwner, dataOwnerWord, Field.Store.NO));
            }

            document.Add(new StringField(LuceneFields.RawField(LuceneFields.Catalog.DataOwner), dataset.DataOwner.ToLowerInvariant(), Field.Store.NO));
        }

        // ResponsibleDeputy
        if (dataset.ResponsibleDeputy is not null)
        {
            FillDocumentWithIopPersonProperties(document, dataset.ResponsibleDeputy, LuceneFields.Catalog.ResponsibleDeputy);
        }

        // ResponsiblePerson
        if (dataset.ResponsiblePerson is not null)
        {
            FillDocumentWithIopPersonProperties(document, dataset.ResponsiblePerson, LuceneFields.Catalog.ResponsiblePerson);
        }

        // ContactPoints
        if (dataset.ContactPoints?.Any() ?? false)
        {
            FillDocumentWithContactPointsProperty(document, dataset.ContactPoints);
        }

        // HasDataStructure
        document.AddStoredAndFacetField(LuceneFields.Catalog.HasStructure, hasStructure.ToString());

        return document;
    }

    private Document GetDocumentFromMappingTableModel(MappingTableModel mappingTable)
    {
        var document = BuildDocument(mappingTable, mappingTable.Identifiers.First());

        // Version
        if (mappingTable.Version is not null)
        {
            document.Add(new TextField(LuceneFields.Catalog.Version, mappingTable.Version, Field.Store.YES));
        }

        // Name
        FillDocumentWithMultiLanguageModelProperty(document, LuceneFields.Catalog.Name, mappingTable.Name, boostBase: 2.0f);
        FillDocumentWithMultiLanguageModelProperty(document, LuceneFields.Catalog.Title, mappingTable.Name, boostBase: 2.0f);

        // Themes
        document.AddStoredAndFacetField(LuceneFields.Catalog.Themes, mappingTable.Themes.Select(x => x.Code));

        // Valid From
        if (mappingTable.ValidFrom.HasValue)
        {
            document.Add(new StoredField(LuceneFields.Catalog.ValidFrom, mappingTable.ValidFrom.ToString()));
        }

        // Valid To
        if (mappingTable.ValidTo.HasValue)
        {
            document.Add(new StoredField(LuceneFields.Catalog.ValidTo, mappingTable.ValidTo.ToString()));
        }

        // ResponsibleDeputy
        if (mappingTable.ResponsibleDeputy is not null)
        {
            FillDocumentWithIopPersonProperties(document, mappingTable.ResponsibleDeputy, LuceneFields.Catalog.ResponsibleDeputy);
        }

        // ResponsiblePerson
        if (mappingTable.ResponsiblePerson is not null)
        {
            FillDocumentWithIopPersonProperties(document, mappingTable.ResponsiblePerson, LuceneFields.Catalog.ResponsiblePerson);
        }

        return document;
    }

    private void IndexDocuments(IReadOnlyDictionary<Guid, Document> documents)
    {
        _writeGate.Wait();
        try
        {
            using var writer = new IndexWriter(_indexDirectory, new IndexWriterConfig(AppLuceneVersion, _analyzer));
            using var taxonomyWriter = new DirectoryTaxonomyWriter(_taxonomyDirectory);

            foreach (var item in documents)
            {
                var processedDocument = _facetsConfig.Build(taxonomyWriter, item.Value);
                writer.UpdateDocument(new Term(LuceneFields.Catalog.Id, GuidToString(item.Key)), processedDocument);
            }
            writer.Flush(true, true);
        }
        finally
        {
            _writeGate.Release();
        }
    }

    private Document BuildDocument(IPublishableEntityModel model, string identifier)
    {
        var document = new Document();

        // Id
        document.AddStringField(LuceneFields.RawField(LuceneFields.Catalog.Id), GuidToString(model.Id), Field.Store.YES); // exact
        document.AddTextField(LuceneFields.Catalog.Id, GuidToString(model.Id), Field.Store.YES); // tokenized

        // Identifier
        document.AddStoredField(LuceneFields.OriginalField(LuceneFields.Catalog.Identifier), identifier);
        document.AddStringField(LuceneFields.RawField(LuceneFields.Catalog.Identifier), identifier.ToLowerInvariant(), Field.Store.YES); // exact
        document.Add(new TextField(LuceneFields.Catalog.Identifier, identifier.ToLowerInvariant(), Field.Store.YES)
        {
            Boost = 1.5f
        });

        // Publication level
        document.AddInt32Field(LuceneFields.Catalog.PublicationLevel, (int)model.PublicationLevel, Field.Store.YES);
        document.Add(new FacetField(LuceneFields.Catalog.PublicationLevel, model.PublicationLevel.ToString()));

        if (model.PublicationLevelProposal.HasValue)
        {
            document.AddInt32Field(LuceneFields.Catalog.PublicationLevelProposal, (int)model.PublicationLevelProposal.Value, Field.Store.YES);
            document.Add(new FacetField(LuceneFields.Catalog.PublicationLevelProposal, model.PublicationLevelProposal.ToString()));
        }

        // Publisher
        document.Add(new FacetField(LuceneFields.Catalog.Publisher, model.Publisher.Id.ToString("N")));
        document.Add(new FacetField(LuceneFields.Catalog.PublisherIdentifier, model.Publisher.Identifier));
        document.AddStringField(LuceneFields.Catalog.Publisher, model.Publisher.Id.ToString("N").ToLowerInvariant(), Field.Store.YES);
        document.AddStringField(LuceneFields.Catalog.PublisherIdentifier, model.Publisher.Identifier.ToLowerInvariant(), Field.Store.YES);

        // Registration status
        document.AddInt32Field(LuceneFields.Catalog.RegistrationStatus, (int)model.RegistrationStatus, Field.Store.YES);
        document.Add(new FacetField(LuceneFields.Catalog.RegistrationStatus, model.RegistrationStatus.ToString()));

        if (model.RegistrationStatusProposal.HasValue)
        {
            document.AddInt32Field(LuceneFields.Catalog.RegistrationStatusProposal, (int)model.RegistrationStatusProposal.Value, Field.Store.YES);
            document.Add(new FacetField(LuceneFields.Catalog.RegistrationStatusProposal, model.RegistrationStatusProposal.ToString()));
        }

        document.Add(new NumericDocValuesField(LuceneFields.Catalog.RegistrationStatusWeight, RegistrationStatusToWeight(model.RegistrationStatus)));

        // Type 
        var type = model switch
        {
            DcatDatasetModel _ => SearchResourceType.Dataset,
            DataServiceModel _ => SearchResourceType.DataService,
            IopConceptModel _ => SearchResourceType.Concept,
            MappingTableModel _ => SearchResourceType.MappingTable,
            PublicServiceModel _ => SearchResourceType.PublicService,
            _ => throw new NotSupportedException($"The type {model.GetType()} is not supported")
        };

        document.AddStoredAndFacetField(LuceneFields.Catalog.Type, type.ToString());

        // Keywords
        FillDocumentWithMultiLanguageModelsProperty(document, LuceneFields.Catalog.Keyword, model.Keywords.Where(x => x.Label is not null).Select(x => x.Label!), boostBase: 1.75f, Field.Store.NO);

        // Description
        FillDocumentWithMultiLanguageModelProperty(document, LuceneFields.Catalog.Description, model.Description, boostBase: 1.5f);

        // System info
        document.Add(new StringField(LuceneFields.Catalog.CreatedAt, model.System.CreatedAt.ToString(), Field.Store.YES));

        if (model.System.CreationType.HasValue)
        {
            document.Add(new Int32Field(LuceneFields.Catalog.CreationType, (int)model.System.CreationType, Field.Store.YES));
        }

        if (model.System.ModifiedAt.HasValue)
        {
            document.Add(new StringField(LuceneFields.Catalog.ModifiedAt, model.System.ModifiedAt.ToString(), Field.Store.YES));
        }

        return document;
    }

    private static void FillDocumentWithIopPersonProperties(Document document, IopPersonModel model, string propertyName)
    {
        document.Add(new TextField($"{propertyName}{LuceneFields.Catalog.Email}", model.Email.ToLower(), Field.Store.NO));
        document.Add(new TextField($"{propertyName}{LuceneFields.Catalog.GivenName}", model.GivenName.ToLower(), Field.Store.NO));
        document.Add(new TextField($"{propertyName}{LuceneFields.Catalog.FamilyName}", model.FamilyName.ToLower(), Field.Store.NO));
    }

    private void FillDocumentWithMultiLanguageModelsProperty(
        Document document,
        string fieldName,
        IEnumerable<MultiLanguageModel> models,
        float boostBase = 1.0f,
        Field.Store store = Field.Store.YES)
    {
        foreach (var item in models)
        {
            FillDocumentWithMultiLanguageModelProperty(document, fieldName, item, boostBase, store);
        }
    }

    private void FillDocumentWithMultiLanguageModelProperty(
        Document document,
        string fieldName,
        MultiLanguageModel model,
        float boostBase = 1.0f,
        Field.Store store = Field.Store.YES)
    {
        foreach (var (language, text) in model.ToDictionary())
        {
            if (_fieldsByAttributeAndLanguages[fieldName].TryGetValue(language, out var field))
            {
                document.Add(new TextField(field, text, store)
                {
                    Boost = boostBase
                });
            }
        }
    }

    private void FillDocumentWithContactPointsProperty(Document document, IEnumerable<VCardModel> contactPoints)
    {
        foreach (var contactPoint in contactPoints)
        {
            if (contactPoint.Fn is not null)
            {
                FillDocumentWithMultiLanguageModelProperty(document, LuceneFields.Catalog.ContactPointFn, contactPoint.Fn, 1, Field.Store.NO);
            }

            if (contactPoint.HasAddress is not null)
            {
                FillDocumentWithMultiLanguageModelProperty(document, LuceneFields.Catalog.ContactPointHasAddress, contactPoint.HasAddress, 1, Field.Store.NO);
            }

            document.Add(new TextField(LuceneFields.Catalog.ContactPointHasEmail, contactPoint.HasEmail.ToLower(), Field.Store.NO));

            if (contactPoint.HasTelephone is not null)
            {
                document.Add(new TextField(LuceneFields.Catalog.ContactPointHasTelephone, contactPoint.HasTelephone.ToLower(), Field.Store.NO));
            }

            if (contactPoint.Note is not null)
            {
                FillDocumentWithMultiLanguageModelProperty(document, LuceneFields.Catalog.ContactPointNote, contactPoint.Note, 1, Field.Store.NO);
            }
        }
    }

    private Query BuildFreeTextQuery(string queryString, IEnumerable<string> languages)
    {
        var queryFields = _fieldsByAttributeAndLanguages.SelectMany(x => x.Value.Where(y => languages.Contains(y.Key)).Select(y => y.Value));
        queryFields = queryFields.Concat(_textProperties);
        var queryParser = new PartialTermMultiFieldQueryParser(LuceneVersion.LUCENE_48, queryFields.ToArray(), _analyzer) { PartialMatchCorrectionFactor = 0.75f };
        return queryParser.Parse(queryString.ToLowerInvariant());
    }

    /// <summary>
    ///     "My Data" and email lookups pass an email address as the search query. Email fields are indexed as a
    ///     single exact (keyword) token, but the multilanguage free-text fields tokenize the address
    ///     (foo.bar@x.admin.ch -> foo/bar/x/admin/ch) and OR-match each token, drowning the exact match in noise
    ///     (bug #725). When the whole query is a single email address, restrict it to an exact match against the
    ///     email fields only. Quotes added by the admin UI are tolerated.
    /// </summary>
    private static Query? TryBuildExactEmailQuery(string queryString)
    {
        var trimmed = queryString.Trim().Trim('"').Trim();
        if (!_emailQueryRegex.IsMatch(trimmed))
        {
            return null;
        }

        var term = trimmed.ToLowerInvariant();
        var query = new BooleanQuery();
        foreach (var field in _emailFields)
        {
            query.Add(new TermQuery(new Term(field, term)), Occur.SHOULD);
        }

        return query;
    }

    private Query BuildSearchQuery(string? queryString, IEnumerable<string> languages, CatalogSearchFilter? searchFilter)
    {
        var baseQuery = BuildBaseQuery(queryString, languages);

        if (searchFilter is null)
        {
            return baseQuery;
        }

        // Facet-backed categories become drill-downs (OR within a category, AND across categories);
        // the numeric categories are applied below as range queries.
        var drillDownQuery = new DrillDownQuery(_facetsConfig, baseQuery);
        AddFacetDrillDowns(drillDownQuery, searchFilter);

        // Create boolean query for combining all conditions
        var booleanQuery = new BooleanQuery
        {
            { drillDownQuery, Occur.MUST }
        };

        // Handle numeric fields with special null value handling
        if (searchFilter.RegistrationStatuses.Any())
        {
            var statusQuery = new BooleanQuery();
            foreach (var status in searchFilter.RegistrationStatuses)
            {
                var numericQuery = NumericRangeQuery.NewInt32Range(
                    LuceneFields.Catalog.RegistrationStatus,
                    (int)status,
                    (int)status,
                    true,
                    true
                );
                statusQuery.Add(numericQuery, Occur.SHOULD);
            }
            booleanQuery.Add(statusQuery, Occur.MUST);
        }

        if (searchFilter.RegistrationStatusProposals.Any())
        {
            var proposalQuery = new BooleanQuery();
            foreach (var proposal in searchFilter.RegistrationStatusProposals)
            {
                var numericQuery = NumericRangeQuery.NewInt32Range(
                    LuceneFields.Catalog.RegistrationStatusProposal,
                    (int)proposal,
                    (int)proposal,
                    true,
                    true
                );
                proposalQuery.Add(numericQuery, Occur.SHOULD);
            }
            booleanQuery.Add(proposalQuery, Occur.MUST);
        }

        if (searchFilter.PublicationLevels.Any())
        {
            var levelQuery = new BooleanQuery();
            foreach (var level in searchFilter.PublicationLevels)
            {
                var numericQuery = NumericRangeQuery.NewInt32Range(
                    LuceneFields.Catalog.PublicationLevel,
                    (int)level,
                    (int)level,
                    true,
                    true
                );
                levelQuery.Add(numericQuery, Occur.SHOULD);
            }
            booleanQuery.Add(levelQuery, Occur.MUST);
        }

        if (searchFilter.PublicationLevelProposals.Any())
        {
            var proposalQuery = new BooleanQuery();
            foreach (var proposal in searchFilter.PublicationLevelProposals)
            {
                var numericQuery = NumericRangeQuery.NewInt32Range(
                    LuceneFields.Catalog.PublicationLevelProposal,
                    (int)proposal,
                    (int)proposal,
                    true,
                    true
                );
                proposalQuery.Add(numericQuery, Occur.SHOULD);
            }
            booleanQuery.Add(proposalQuery, Occur.MUST);
        }

        if (searchFilter.ConceptValueTypes.Any())
        {
            var conceptTypeQuery = new BooleanQuery();
            foreach (var conceptValueType in searchFilter.ConceptValueTypes)
            {
                var numericQuery = NumericRangeQuery.NewInt32Range(
                    LuceneFields.Catalog.ConceptType,
                    (int)conceptValueType,
                    (int)conceptValueType,
                    true,
                    true
                );
                conceptTypeQuery.Add(numericQuery, Occur.SHOULD);
            }
            booleanQuery.Add(conceptTypeQuery, Occur.MUST);
        }

        return booleanQuery;
    }

    // Base query shared by the result-list and count query builders: an exact-email or free-text query
    // when a search term is given, otherwise match-all.
    private Query BuildBaseQuery(string? queryString, IEnumerable<string> languages)
    {
        languages = languages.Any()
            ? languages
            : _languages;

        if (string.IsNullOrEmpty(queryString))
        {
            return new MatchAllDocsQuery();
        }

        return TryBuildExactEmailQuery(queryString) ?? BuildFreeTextQuery(queryString, languages);
    }

    // Facet-backed filter categories shared by the result-list query (BuildSearchQuery) and the count
    // query (BuildCountDrillDownQuery). Centralised so a new/changed facet filter is updated in one place.
    private static void AddFacetDrillDowns(DrillDownQuery drillDownQuery, CatalogSearchFilter searchFilter)
    {
        foreach (var accessRight in searchFilter.AccessRights)
        {
            drillDownQuery.Add(LuceneFields.Catalog.AccessRights, accessRight);
        }

        foreach (var businessEvent in searchFilter.BusinessEvents)
        {
            drillDownQuery.Add(LuceneFields.Catalog.BusinessEvents, businessEvent);
        }

        foreach (var format in searchFilter.Formats)
        {
            drillDownQuery.Add(LuceneFields.Catalog.Formats, format);
        }

        if (searchFilter.Structure.HasValue)
        {
            var hasStructure = searchFilter.Structure.Value is SearchStructureOption.WithStructure;

            drillDownQuery.Add(LuceneFields.Catalog.HasStructure, hasStructure.ToString());
        }

        foreach (var lifeEvent in searchFilter.LifeEvents)
        {
            drillDownQuery.Add(LuceneFields.Catalog.LifeEvents, lifeEvent);
        }

        foreach (var publisher in searchFilter.PublisherIdentifiers)
        {
            drillDownQuery.Add(LuceneFields.Catalog.PublisherIdentifier, publisher);
        }

        foreach (var attributedAgent in searchFilter.AttributedAgentIdentifiers)
        {
            drillDownQuery.Add(LuceneFields.Catalog.QualifiedAttributionAgentIdentifier, attributedAgent);
        }

        foreach (var theme in searchFilter.Themes)
        {
            drillDownQuery.Add(LuceneFields.Catalog.Themes, theme);
        }

        foreach (var type in searchFilter.Types)
        {
            drillDownQuery.Add(LuceneFields.Catalog.Type, type.ToString());
        }
    }

    // Count-only query builder. Unlike BuildSearchQuery (used by the result list), this also expresses
    // the numeric filter categories as drill-downs, so DrillSideways can treat every filter category as
    // a facet dimension. The base query and facet drill-downs are shared with BuildSearchQuery.
    private DrillDownQuery BuildCountDrillDownQuery(string? queryString, IEnumerable<string> languages, CatalogSearchFilter? searchFilter)
    {
        var baseQuery = BuildBaseQuery(queryString, languages);
        var drillDownQuery = new DrillDownQuery(_facetsConfig, baseQuery);

        if (searchFilter is null)
        {
            return drillDownQuery;
        }

        AddFacetDrillDowns(drillDownQuery, searchFilter);

        // The numeric categories are also indexed as facet fields, so add them as drill-downs here
        // (rather than the NumericRangeQuery that BuildSearchQuery uses) to make them DrillSideways dimensions.
        foreach (var status in searchFilter.RegistrationStatuses)
        {
            drillDownQuery.Add(LuceneFields.Catalog.RegistrationStatus, status.ToString());
        }

        foreach (var proposal in searchFilter.RegistrationStatusProposals)
        {
            drillDownQuery.Add(LuceneFields.Catalog.RegistrationStatusProposal, proposal.ToString());
        }

        foreach (var level in searchFilter.PublicationLevels)
        {
            drillDownQuery.Add(LuceneFields.Catalog.PublicationLevel, level.ToString());
        }

        foreach (var proposal in searchFilter.PublicationLevelProposals)
        {
            drillDownQuery.Add(LuceneFields.Catalog.PublicationLevelProposal, proposal.ToString());
        }

        foreach (var conceptValueType in searchFilter.ConceptValueTypes)
        {
            drillDownQuery.Add(LuceneFields.Catalog.ConceptType, conceptValueType.ToString());
        }

        return drillDownQuery;
    }

    private BooleanFilter? TryGetUsersAuthorizationFilter()
    {
        var userBusinessRole = _userContextService.GetUserBusinessRole();

        if (userBusinessRole is BusinessRole.InteroperabilityService or BusinessRole.SwissDataSteward)
        {
            return null;
        }

        var filter = new BooleanFilter
        {
            new FilterClause(
            NumericRangeFilter.NewInt32Range(
                LuceneFields.Catalog.PublicationLevel,
                (int)PublicationLevel.Public,
                (int)PublicationLevel.Public,
                true,
                true
            ),
            Occur.SHOULD)
        };

        var userAgencies = _userContextService.GetUserAgencies();

        foreach (var item in userAgencies)
        {
            filter.Add(new FilterClause(new TermFilter(new Term(LuceneFields.Catalog.PublisherIdentifier, item.ToLowerInvariant())), Occur.SHOULD));
        }

        return filter;
    }

    private static int RegistrationStatusToWeight(RegistrationStatus status) => status switch
    {
        RegistrationStatus.Incomplete => 95,
        RegistrationStatus.Candidate => 98,
        RegistrationStatus.Recorded => 100,
        RegistrationStatus.Qualified => 102,
        RegistrationStatus.Standard => 105,
        RegistrationStatus.PreferredStandard => 110,
        RegistrationStatus.Superseded => 90,
        RegistrationStatus.Retired => 85,
        _ => 100
    };

    private static string GuidToString(Guid id) => id.ToString("D").ToLowerInvariant();
}
