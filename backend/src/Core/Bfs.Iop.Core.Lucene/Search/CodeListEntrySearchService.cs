using Bfs.Iop.Core.Abstractions.Commands.FilterConfigurations;
using Bfs.Iop.Core.Abstractions.Models.FilterConfigurations;
using Bfs.Iop.Core.Abstractions.Models.Search;
using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.TokenAttributes;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Search.Join;
using Lucene.Net.Util;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Bfs.Iop.Core.Lucene.Search;

internal sealed class CodeListEntrySearchService : ICodeListEntrySearchService
{
    private readonly ICodeListEntryIndexService _indexService;
    private readonly IIopConceptsService _iopConceptsService;
    private readonly ILogger<CodeListEntrySearchService> _logger;
    private readonly IMediator _mediator;

    public CodeListEntrySearchService(
        ICodeListEntryIndexService indexService,
        IIopConceptsService iopConceptsService,
        ILogger<CodeListEntrySearchService> logger,
        IMediator mediator)
    {
        _indexService = indexService;
        _iopConceptsService = iopConceptsService;
        _logger = logger;
        _mediator = mediator;
    }

    public async Task<PagedResult<CodeListEntrySearchResultEntryModel>> Search(
        Guid conceptId,
        string language,
        string? query,
        List<string> filters,
        bool addCodeListEntriesPaths,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        _ = await _iopConceptsService.GetIopConcept(conceptId, false, cancellationToken); // ensure user is allowed to access the resource

        var filterConfiguration = new FilterConfigurationModel { Filters = [] };
        try
        {
            filterConfiguration = await _mediator.Send(new GetFilterConfigurationCommand(conceptId), cancellationToken);
        }
        catch
        {
            //if there is no filterConfiguration we use the default value
        }

        using var reader = DirectoryReader.Open(_indexService.IndexDirectory);
        var searcher = new IndexSearcher(reader);

        BooleanQuery finalQuery = BuildLuceneQuery(conceptId, language, query, filters, filterConfiguration, searcher);

        var topDocs = searcher.Search(finalQuery, page * pageSize);

        var codeListEntryGuidsWithScores = new Dictionary<Guid, float>();

        foreach (var hit in topDocs.ScoreDocs.Skip((page - 1) * pageSize).Take(pageSize))
        {
            var doc = searcher.Doc(hit.Doc);

            var guidString = doc.Get(LuceneFields.CodeListEntry.Id);
            if (!string.IsNullOrWhiteSpace(guidString))
            {
                codeListEntryGuidsWithScores.Add(Guid.Parse(guidString), hit.Score);
            }

            var explanation = searcher.Explain(finalQuery, hit.Doc);
            _logger.LogDebug("Explanation: {Explain}", explanation.ToString());
        }

        var codeListEntryModels = await _iopConceptsService.GetCodeListEntriesByIds(codeListEntryGuidsWithScores.Keys, cancellationToken);

        var entryPaths = addCodeListEntriesPaths
            ? await CreatePaths(conceptId, codeListEntryModels, cancellationToken)
            : [];

        var result = new PagedResult<CodeListEntrySearchResultEntryModel>
        {
            Results = codeListEntryGuidsWithScores.Select((k) => new CodeListEntrySearchResultEntryModel()
            {
                Entry = codeListEntryModels.Single(i => i.Id == k.Key),
                Score = codeListEntryGuidsWithScores[k.Key],
                Path = addCodeListEntriesPaths 
                    ? entryPaths[k.Key]
                    : []
            }).ToList().AsReadOnly(),
            Page = page,
            PageSize = pageSize is int.MaxValue ? topDocs.TotalHits : pageSize,
            TotalCount = topDocs.TotalHits
        };

        return result;
    }

    private BooleanQuery BuildLuceneQuery(
        Guid conceptId,
        string language,
        string? query,
        List<string> filters,
        FilterConfigurationModel filterConfiguration,
        IndexSearcher searcher)
    {
        var fieldsWithBoosts = new Dictionary<string, float>
        {
            [LuceneFields.CodeListEntry.Code] = 20.0f,
            [LuceneFields.NgramField(LuceneFields.CodeListEntry.Name(language))] = 16.0f,
            [LuceneFields.CodeListEntry.Description(language)] = 12.0f,
            [LuceneFields.CodeListEntryAnnotation.AnnotationType] = 8.0f,
            [LuceneFields.CodeListEntryAnnotation.AnnotationIdentifier] = 8.0f,
            [LuceneFields.CodeListEntryAnnotation.AnnotationTitle] = 8.0f,
            [LuceneFields.CodeListEntryAnnotation.AnnotationText(language)] = 8.0f,
            [LuceneFields.CodeListEntryAnnotation.AnnotationUri] = 8.0f,
        };

        var q = string.IsNullOrWhiteSpace(query) ? "*" : query.Trim().ToLowerInvariant();

        Query textQuery;

        if (q == "*")
        {
            textQuery = new MatchAllDocsQuery();
        }
        else
        {
            var termAnalyzer = new Index.Analyzers.LanguageDependentAnalyzer(
                LuceneVersion.LUCENE_48,
                language,
                isNgramField: false);

            var analysisField = LuceneFields.CodeListEntry.Name(language);

            var userTerms = ExtractSearchTerms(termAnalyzer, analysisField, q);

            _logger.LogDebug($"Search terms after analysis: {string.Join(", ", userTerms)}");

            var ngramAnalyzer = new Index.Analyzers.LanguageDependentAnalyzer(
                LuceneVersion.LUCENE_48,
                language,
                isNgramField: true);

            var andAcrossTerms = new BooleanQuery();

            foreach (var userTerm in userTerms.Where(i => i.Length >= 2))
            {
                var orAcrossFields = new BooleanQuery();

                foreach (var (field, boost) in fieldsWithBoosts)
                {
                    Query perFieldQuery = field.EndsWith("_ngram") 
                        ? BuildNGramSoftMatchQuery(ngramAnalyzer, field, userTerm, minMatchRatio: 0.6)
                        : BuildNonNGramFieldQuery(field, userTerm);

                    perFieldQuery.Boost = boost;
                    orAcrossFields.Add(perFieldQuery, Occur.SHOULD);
                }

                andAcrossTerms.Add(orAcrossFields, Occur.MUST);
            }

            textQuery = andAcrossTerms.Clauses.Count == 0 ? new MatchAllDocsQuery() : andAcrossTerms;
        }

        var conceptFilter = new TermQuery(new Term(LuceneFields.CodeListEntry.ConceptId, conceptId.ToString()));

        var finalQuery = new BooleanQuery
        {
            { textQuery, Occur.MUST },
            { conceptFilter, Occur.MUST }
        };

        AddFilters(filters, filterConfiguration, searcher, finalQuery);

        _logger.LogDebug(finalQuery.ToString());
        return finalQuery;
    }

    private static List<string> ExtractSearchTerms(
        Analyzer analyzer,
        string fieldForAnalysis,
        string queryText)
    {
        var terms = new List<string>();

        using var reader = new StringReader(queryText);
        using var stream = analyzer.GetTokenStream(fieldForAnalysis, reader);

        var termAttr = stream.AddAttribute<ICharTermAttribute>();
        stream.Reset();

        while (stream.IncrementToken())
        {
            if (!string.IsNullOrWhiteSpace(termAttr.ToString()))
                terms.Add(termAttr.ToString());
        }

        stream.End();
        return terms;
    }

    private static Query BuildNonNGramFieldQuery(string field, string term)
    {
        if (field == LuceneFields.CodeListEntry.Code ||
            field == LuceneFields.CodeListEntryAnnotation.AnnotationIdentifier ||
            field == LuceneFields.CodeListEntryAnnotation.AnnotationUri)
        {
            return term.Length <= 2
                ? (Query)new TermQuery(new Term(field, term))
                : new PrefixQuery(new Term(field, term));
        }

        var maxEdits = term.Length <= 4 ? 1 : 2;

        return new FuzzyQuery(
            new Term(field, term),
            maxEdits: maxEdits,
            prefixLength: 1,
            maxExpansions: 50,
            transpositions: true);
    }

    private static BooleanQuery BuildNGramSoftMatchQuery(
        Analyzer ngramAnalyzer,
        string ngramField,
        string userTerm,
        double minMatchRatio)
    {
        var grams = AnalyseTokens(ngramAnalyzer, ngramField, userTerm)
            .Distinct()
            .ToList();

        var booleanQuery = new BooleanQuery();

        if (grams.Count == 0)
        {
            booleanQuery.Add(new TermQuery(new Term(ngramField, userTerm)), Occur.SHOULD);
            return booleanQuery;
        }

        foreach (var gram in grams)
        {
            booleanQuery.Add(new TermQuery(new Term(ngramField, gram)), Occur.SHOULD);
        }

        booleanQuery.MinimumNumberShouldMatch = Math.Max(1, (int)Math.Ceiling(grams.Count * minMatchRatio));
        return booleanQuery;
    }

    private static List<string> AnalyseTokens(Analyzer analyzer, string field, string text)
    {
        var tokens = new List<string>();

        using var reader = new StringReader(text);
        using var stream = analyzer.GetTokenStream(field, reader);

        var termAttr = stream.AddAttribute<ICharTermAttribute>();
        stream.Reset();

        while (stream.IncrementToken())
        {
            tokens.Add(termAttr.ToString());
        }

        stream.End();
        return tokens;
    }
    
    private static void AddFilters(List<string> filters, FilterConfigurationModel filterConfiguration, IndexSearcher searcher, BooleanQuery finalQuery)
    {
        var jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var parsedFilters = filters
            .Select(filter => JsonSerializer.Deserialize<FilterInputModel>(filter, jsonSerializerOptions))
            .Where(i => i != null)
            .Cast<FilterInputModel>();

        if (!parsedFilters.Any())
        {
            return;
        }

        var (filterQueriesAnd, filterQueriesOr) = BuildFilterQueries(parsedFilters, filterConfiguration);

        foreach (var filterQuery in filterQueriesAnd)
        {
            var andJoinQuery = JoinUtil.CreateJoinQuery(
                LuceneFields.CodeListEntryAnnotation.AnnotationCodeListEntryId,
                false,
                LuceneFields.CodeListEntry.Id,
                filterQuery,
                searcher,
                ScoreMode.None);

            finalQuery.Add(andJoinQuery, Occur.MUST);
        }

        if (filterQueriesOr.Count > 0)
        {
            var orQueries = new BooleanQuery();

            foreach (var filterQuery in filterQueriesOr)
            {
                orQueries.Add(filterQuery, Occur.SHOULD);
            }

            var orJoinQuery = JoinUtil.CreateJoinQuery(
                LuceneFields.CodeListEntryAnnotation.AnnotationCodeListEntryId,
                false,
                LuceneFields.CodeListEntry.Id,
                orQueries,
                searcher,
                ScoreMode.None);

            finalQuery.Add(orJoinQuery, Occur.MUST);
        }
    }

    private static (List<BooleanQuery>, List<BooleanQuery>) BuildFilterQueries(IEnumerable<FilterInputModel> parsedFilters, FilterConfigurationModel filterConfigurationModel)
    {
        List<BooleanQuery> filterQueriesAnd = [];
        List<BooleanQuery> filterQueriesOr = [];

        foreach (var parsedFilter in parsedFilters)
        {
            var filterDefinition = filterConfigurationModel.Filters.SingleOrDefault(i => i.FilterIdentifier.Equals(parsedFilter.FilterIdentifier, StringComparison.InvariantCultureIgnoreCase))
                ?? throw new InvalidOperationException($"FilterInputModel filter with identifier {parsedFilter.FilterIdentifier} does not exist.");

            var selectedfilterDefinitionValues = filterDefinition.Values.Where(i => parsedFilter.Values.Select(v => v.ToLowerInvariant()).Contains(i.FilterValueIdentifier.ToLowerInvariant()));

            foreach (var filterDefinitionValue in selectedfilterDefinitionValues)
            {

                foreach (var fieldValue in filterDefinitionValue.FieldValues)
                {

                    var fieldValueQuery = new BooleanQuery()
                        {
                            { new TermQuery(new Term(LuceneFields.RawField(LuceneFields.CodeListEntryAnnotation.AnnotationType), fieldValue.FieldName.ToLowerInvariant())), Occur.MUST },
                        };

                    if (!string.IsNullOrWhiteSpace(fieldValue.FieldValue))
                    {
                        var field = fieldValue.FieldProperty?.ToLower() switch
                        {
                            "title" => LuceneFields.RawField(LuceneFields.CodeListEntryAnnotation.AnnotationTitle),
                            "identifier" => LuceneFields.RawField(LuceneFields.CodeListEntryAnnotation.AnnotationIdentifier),
                            "type" or _ => LuceneFields.RawField(LuceneFields.CodeListEntryAnnotation.AnnotationType),
                        };

                        fieldValueQuery.Add(new TermQuery(new Term(field, fieldValue.FieldValue.ToLowerInvariant())), Occur.MUST);
                    }
                    else if (fieldValue.FieldValueMultilanguage != null)
                    {
                        var fieldValueQueryLanguage = new BooleanQuery();

                        foreach (var language in fieldValue.FieldValueMultilanguage.ToDictionary())
                        {
                            fieldValueQueryLanguage.Add(new TermQuery(new Term(LuceneFields.RawField(LuceneFields.CodeListEntryAnnotation.AnnotationText(language.Key)), language.Value)), Occur.SHOULD);
                        }

                        fieldValueQuery.Add(fieldValueQueryLanguage, Occur.MUST);
                    }

                    if (filterDefinitionValue.OccurType == (int)Occur.MUST)
                    {
                        filterQueriesAnd.Add(fieldValueQuery);
                    }
                    else
                    {
                        filterQueriesOr.Add(fieldValueQuery);
                    }
                }
            }
        }

        return (filterQueriesAnd, filterQueriesOr);
    }

    private async Task<Dictionary<Guid, IEnumerable<CodeListEntrySearchResultPathModel>>> CreatePaths(Guid conceptId, IEnumerable<CodeListEntryModel> codeListEntryModels, CancellationToken cancellationToken)
    {
        var paths = new Dictionary<Guid, IEnumerable<CodeListEntrySearchResultPathModel>>();

        foreach (var codeListEntryModel in codeListEntryModels)
        {
            paths[codeListEntryModel.Id] = await CreatePath([], conceptId, codeListEntryModel, cancellationToken);
        }

        return paths;
    }

    private async Task<IEnumerable<CodeListEntrySearchResultPathModel>> CreatePath(List<CodeListEntrySearchResultPathModel> paths, Guid conceptId, CodeListEntryModel codeListEntryModel, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(codeListEntryModel.ParentCode))
        {
            var parentCodeListEntryModel = (await _iopConceptsService.GetCodeListEntriesByCodes(conceptId, [codeListEntryModel.ParentCode], cancellationToken)).Single();

            await CreatePath(paths, conceptId, parentCodeListEntryModel, cancellationToken);
        }

        paths.Add(new CodeListEntrySearchResultPathModel() { Code = codeListEntryModel.Code, Name = codeListEntryModel.Name, ParentCode = codeListEntryModel.ParentCode });

        return paths;
    }
}