using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Contracts;
using Bfs.Iop.Core.Lucene.Index.Analyzers;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Miscellaneous;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bfs.Iop.Core.Lucene.Index;

internal sealed class CodeListEntryIndexService : ICodeListEntryIndexService, IDisposable
{
    private readonly ILogger<CodeListEntryIndexService> _logger;
    private readonly int _maxConcurrentTasks = 5;

    private readonly global::Lucene.Net.Store.Directory _indexDirectory;
    private readonly Analyzer _analyzer;
    private const LuceneVersion AppLuceneVersion = LuceneVersion.LUCENE_48;

    // The service provider is necessary to avoid the circular dependency with iop concepts service.
    private readonly IServiceProvider _serviceProvider;

    public global::Lucene.Net.Store.Directory IndexDirectory => _indexDirectory;

    private readonly SemaphoreSlim _writeGate = new(1, 1);

    public CodeListEntryIndexService(
        IServiceProvider serviceProvider,
        ILogger<CodeListEntryIndexService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;

        var useRamDirectory = bool.TryParse(
            configuration["Lucene:UseRamDirectory"],
            out var useRamDirectoryAppConfigValue) && useRamDirectoryAppConfigValue;

        _indexDirectory = useRamDirectory
            ? new RAMDirectory()
            : FSDirectory.Open(new DirectoryInfo(configuration["Lucene:IndexDirectory"] ?? "lucene-index"));

        _analyzer = new StandardAnalyzer(AppLuceneVersion);

        Analyzer defaultAnalyzer = new IopStandardAnalyzer(AppLuceneVersion);

        var fieldAnalyzers = new Dictionary<string, Analyzer>(StringComparer.InvariantCultureIgnoreCase)
        {
            [LuceneFields.CodeListEntry.Name("de")] = new LanguageDependentAnalyzer(AppLuceneVersion, "de"),
            [LuceneFields.CodeListEntry.Name("en")] = new LanguageDependentAnalyzer(AppLuceneVersion, "en"),
            [LuceneFields.CodeListEntry.Name("fr")] = new LanguageDependentAnalyzer(AppLuceneVersion, "fr"),
            [LuceneFields.CodeListEntry.Name("it")] = new LanguageDependentAnalyzer(AppLuceneVersion, "it"),

            [LuceneFields.CodeListEntry.Description("de")] = new LanguageDependentAnalyzer(AppLuceneVersion, "de"),
            [LuceneFields.CodeListEntry.Description("en")] = new LanguageDependentAnalyzer(AppLuceneVersion, "en"),
            [LuceneFields.CodeListEntry.Description("fr")] = new LanguageDependentAnalyzer(AppLuceneVersion, "fr"),
            [LuceneFields.CodeListEntry.Description("it")] = new LanguageDependentAnalyzer(AppLuceneVersion, "it"),

            [LuceneFields.CodeListEntryAnnotation.AnnotationText("de")] = new LanguageDependentAnalyzer(AppLuceneVersion, "de"),
            [LuceneFields.CodeListEntryAnnotation.AnnotationText("en")] = new LanguageDependentAnalyzer(AppLuceneVersion, "en"),
            [LuceneFields.CodeListEntryAnnotation.AnnotationText("fr")] = new LanguageDependentAnalyzer(AppLuceneVersion, "fr"),
            [LuceneFields.CodeListEntryAnnotation.AnnotationText("it")] = new LanguageDependentAnalyzer(AppLuceneVersion, "it"),

            [LuceneFields.NgramField(LuceneFields.CodeListEntry.Name("de"))] = new LanguageDependentAnalyzer(AppLuceneVersion, "de", true),
            [LuceneFields.NgramField(LuceneFields.CodeListEntry.Name("en"))] = new LanguageDependentAnalyzer(AppLuceneVersion, "en", true),
            [LuceneFields.NgramField(LuceneFields.CodeListEntry.Name("fr"))] = new LanguageDependentAnalyzer(AppLuceneVersion, "fr", true),
            [LuceneFields.NgramField(LuceneFields.CodeListEntry.Name("it"))] = new LanguageDependentAnalyzer(AppLuceneVersion, "it", true),
        };

        _analyzer = new PerFieldAnalyzerWrapper(defaultAnalyzer, fieldAnalyzers);
    }

    public async Task BuildIndex(CancellationToken cancellationToken = default)
    {
        var conceptsService = _serviceProvider.GetRequiredService<IIopConceptsService>();

        _logger.LogInformation("Start building CodeListEntry index.");

        using var indexWriter = new IndexWriter(_indexDirectory, new IndexWriterConfig(AppLuceneVersion, _analyzer));
        indexWriter.DeleteAll();

        const int batchSize = 100;

        var semaphore = new SemaphoreSlim(_maxConcurrentTasks);

        await foreach (var codeListEntryModels in conceptsService.GetCodeListEntriesForIndexInBatches(batchSize, cancellationToken))
        {
            var tasks = codeListEntryModels.Select(async codeListEntryModel =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                await semaphore.WaitAsync(cancellationToken);

                try
                {
                    BuildDocument(indexWriter, codeListEntryModel);

                    _logger.LogDebug($"{codeListEntryModel.Code} value indexed for concept {codeListEntryModel.ConceptId}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"{codeListEntryModel.Code} value for concept {codeListEntryModel.ConceptId} is faulty, error: {ex}");
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);
        }

        _logger.LogInformation("Finished building CodeListEntry index.");
    }

    public void Index(IEnumerable<CodeListEntryModel> codeListEntries)
    {
        ArgumentNullException.ThrowIfNull(codeListEntries, nameof(codeListEntries));

        _writeGate.Wait();

        try
        {
            using var writer = new IndexWriter(_indexDirectory, new IndexWriterConfig(AppLuceneVersion, _analyzer));

            foreach (var codeListEntry in codeListEntries)
            {
                BuildDocument(writer, codeListEntry);
            }
        }
        finally
        {
            _writeGate.Release(); 
        }
    }

    public void UpdateIndex(IEnumerable<CodeListEntryModel> codeListEntries)
    {
        ArgumentNullException.ThrowIfNull(codeListEntries, nameof(codeListEntries));

        DeIndex(codeListEntries.Select(x => x.Id));
        Index(codeListEntries);
    }

    public void DeIndex(IEnumerable<Guid> codeListEntriesIds)
    {
        ArgumentNullException.ThrowIfNull(codeListEntriesIds, nameof(codeListEntriesIds));

        _writeGate.Wait();

        try
        {
            using var writer = new IndexWriter(_indexDirectory, new IndexWriterConfig(AppLuceneVersion, _analyzer));

            foreach (var id in codeListEntriesIds)
            {
                writer.DeleteDocuments(new Term(LuceneFields.CodeListEntry.Id, id.ToString()));
            }

            writer.Flush(true, true);
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

    private static void BuildDocument(IndexWriter writer, CodeListEntryModel codeListEntryModel)
    {
        Document doc = BuildCodeListEntryDocument(codeListEntryModel);

        if (codeListEntryModel.Annotations.Any())
        {
            var childDocs = new List<Document>();

            foreach (var annotation in codeListEntryModel.Annotations)
            {
                Document childDoc = BuildAnnotationDocument(annotation);

                childDocs.Add(childDoc);
            }

            // add children + parent as a block, it is a requirement from Lucene that the parent document is at the end of the document block
            childDocs.Add(doc);
            writer.AddDocuments(childDocs);
        }
        else
        {
            // no annotations — index parent alone
            writer.AddDocument(doc);
        }
    }

    private static Document BuildCodeListEntryDocument(CodeListEntryModel codeListEntryModel)
    {
        var doc = new Document
        {
            new StringField(LuceneFields.CodeListEntry.Id, codeListEntryModel.Id.ToString(), Field.Store.YES),
            new StringField(LuceneFields.CodeListEntry.ConceptId, codeListEntryModel.ConceptId.ToString(), Field.Store.NO),
            new TextField(LuceneFields.CodeListEntry.Code, codeListEntryModel.Code, Field.Store.NO)
        };

        foreach (var (lang, value) in codeListEntryModel.Name.ToDictionary())
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                doc.Add(new TextField(LuceneFields.CodeListEntry.Name(lang), value, Field.Store.NO));
                doc.Add(new TextField(LuceneFields.NgramField(LuceneFields.CodeListEntry.Name(lang)), value, Field.Store.NO));
            }
        }

        foreach (var (lang, value) in codeListEntryModel.Description?.ToDictionary() ?? [])
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                doc.Add(new TextField(LuceneFields.CodeListEntry.Description(lang), value, Field.Store.NO));
            }
        }

        return doc;
    }

    private static Document BuildAnnotationDocument(AnnotationModel annotation)
    {
        var childDoc = new Document
                {
                    new TextField(LuceneFields.CodeListEntryAnnotation.AnnotationType, annotation.Type, Field.Store.NO),
                    new StringField(LuceneFields.RawField(LuceneFields.CodeListEntryAnnotation.AnnotationType), annotation.Type.ToLowerInvariant(), Field.Store.NO),
                    new StringField(LuceneFields.CodeListEntryAnnotation.AnnotationCodeListEntryId, annotation.CodeListEntryId.ToString(), Field.Store.YES)
                };

        if (!string.IsNullOrWhiteSpace(annotation.Identifier))
        {
            childDoc.Add(new TextField(LuceneFields.CodeListEntryAnnotation.AnnotationIdentifier, annotation.Identifier, Field.Store.NO));
            childDoc.Add(new StringField(LuceneFields.RawField(LuceneFields.CodeListEntryAnnotation.AnnotationIdentifier), annotation.Identifier.ToLowerInvariant(), Field.Store.NO));
        }

        if (!string.IsNullOrWhiteSpace(annotation.Title))
        {
            childDoc.Add(new TextField(LuceneFields.CodeListEntryAnnotation.AnnotationTitle, annotation.Title, Field.Store.NO));
            childDoc.Add(new StringField(LuceneFields.RawField(LuceneFields.CodeListEntryAnnotation.AnnotationTitle), annotation.Title.ToLowerInvariant(), Field.Store.NO));
        }

        if (!string.IsNullOrWhiteSpace(annotation.Uri))
        {
            childDoc.Add(new TextField(LuceneFields.CodeListEntryAnnotation.AnnotationUri, annotation.Uri, Field.Store.NO));
            childDoc.Add(new StringField(LuceneFields.RawField(LuceneFields.CodeListEntryAnnotation.AnnotationUri), annotation.Uri.ToLowerInvariant(), Field.Store.NO));
        }

        foreach (var (lang, value) in annotation.Text?.ToDictionary() ?? [])
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                childDoc.Add(new TextField(LuceneFields.CodeListEntryAnnotation.AnnotationText(lang), value, Field.Store.NO));
                childDoc.Add(new StringField(LuceneFields.RawField(LuceneFields.CodeListEntryAnnotation.AnnotationText(lang)), value.ToLowerInvariant(), Field.Store.NO));
            }
        }

        return childDoc;
    }
}