using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Contracts;
using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.Core.Lucene.Search;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Bfs.Iop.Core.Lucene.UnitTests;

[TestFixture(TestOf = typeof(CodeListEntrySearchService))]
public class CodeListEntrySearchServiceTests
{
    private CodeListEntrySearchService _service = null!;
    private CodeListEntryIndexService _indexService = null!;
    private IIopConceptsService _mockConceptsService = null!;
    private IMediator _mockMediator = null!;
    private Guid _conceptId;

    private List<CodeListEntryModel> _testEntries = [];

    [SetUp]
    public async Task SetUp()
    {
        _conceptId = Guid.NewGuid();

        var codeListEntryIdLuft = Guid.NewGuid();

        var testEntryLuft = new CodeListEntryModel
        {
            Id = codeListEntryIdLuft,
            ConceptId = _conceptId,
            Code = "Luft",
            Name = new MultiLanguageModel { De = "Luftfahrt" },
            Description = new MultiLanguageModel { De = "Fliegen im Himmel" },
            Annotations = new List<AnnotationModel>
            {
                new()
                {
                    CodeListEntryId = codeListEntryIdLuft,
                    Type = "topic",
                    Title = "Aviation",
                    Uri = "http://example.com/aviation"
                }
            }
        };

        var codeListEntryIdStrasse = Guid.NewGuid();

        var testEntryStrasse = new CodeListEntryModel
        {
            Id = codeListEntryIdStrasse,
            ConceptId = _conceptId,
            Code = "Erde",
            Name = new MultiLanguageModel { De = "Autofahrt" },
            Description = new MultiLanguageModel { De = "Fahren auf der Straße" },
            Annotations = new List<AnnotationModel>
            {
                new()
                {
                    CodeListEntryId = codeListEntryIdStrasse,
                    Type = "topic",
                    Title = "Automobile",
                    Uri = "http://example.com/automobile"
                }
            }
        };

        _testEntries = [testEntryLuft, testEntryStrasse];

        _mockConceptsService = Substitute.For<IIopConceptsService>();

        _mockConceptsService
            .GetCodeListEntriesForIndexInBatches(cancellationToken: Arg.Any<CancellationToken>())
            .Returns(BatchResult());

        _mockConceptsService
            .GetCodeListEntriesByIds(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(_testEntries);

        _mockConceptsService
            .GetCodeListEntriesByCodes(_conceptId, Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IEnumerable<CodeListEntryModel>>([]));

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<IIopConceptsService>(_ => _mockConceptsService);
        var sp = serviceCollection.BuildServiceProvider();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Lucene:UseRamDirectory"] = "true"
            })
            .Build();

        _indexService = new CodeListEntryIndexService(sp, NullLoggerFactory.Instance.CreateLogger<CodeListEntryIndexService>(), config);

        await _indexService.BuildIndex(CancellationToken.None);

        var logger = NullLoggerFactory.Instance.CreateLogger<CodeListEntrySearchService>();

        _mockMediator = Substitute.For<IMediator>();

        _service = new CodeListEntrySearchService(_indexService, _mockConceptsService, logger, _mockMediator);
    }

    [TearDown]
    public void OnTearDown() => _indexService.Dispose();

    private async IAsyncEnumerable<List<CodeListEntryModel>> BatchResult()
    {
        await Task.Delay(1);

        yield return  _testEntries;
    }

    [Test]
    public async Task Search_Term_ReturnsMultipleMatches()
    {
        var result = await _service.Search(_conceptId, "de", "fahrt", [], true, 1, 10);
        Assert.That(result.Results.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task Search_Term_Auto_ReturnsSingleMatch()
    {
        var result = await _service.Search(_conceptId, "de", "auto", [], true, 1, 10);
        Assert.That(result.Results.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task Search_Term_Luft_ReturnsSingleMatch()
    {
        var result = await _service.Search(_conceptId, "de", "auto", [], true, 1, 10);
        Assert.That(result.Results.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task Search_TermWithTypo_InTheBeginning_ReturnsMultipleMatches()
    {
        var result = await _service.Search(_conceptId, "de", "tfahrt", [], true, 1, 10);
        Assert.That(result.Results.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task Search_TermWithTypo_AtTheEnd_ReturnsMultipleMatches()
    {
        var result = await _service.Search(_conceptId, "de", "fahrd", [], true, 1, 10);
        Assert.That(result.Results.Count(), Is.EqualTo(2));
    }
}