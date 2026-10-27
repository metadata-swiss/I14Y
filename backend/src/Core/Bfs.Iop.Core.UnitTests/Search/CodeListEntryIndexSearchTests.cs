using AwesomeAssertions;
using Bfs.Iop.Core.Abstractions.Commands.FilterConfigurations;
using Bfs.Iop.Core.Abstractions.Models.FilterConfigurations;
using Bfs.Iop.Core.Search;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Abstractions.Exceptions;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.IndexSearch.ApiClient;
using Bfs.Iop.IndexSearch.Contracts.Search;
using Bfs.Iop.Infrastructure.ApiClient;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Bfs.Iop.Core.UnitTests.Search;

[TestFixture]
internal sealed class CodeListEntryIndexSearchTests
{
    private readonly Guid _conceptId = Guid.NewGuid();
    private IIopConceptsService _concepts = null!;
    private IIndexSearchApiClient _client = null!;
    private ILogger<CodeListEntryIndexSearch> _logger = null!;
    private CodeListEntryIndexSearch _subject = null!;
    private Dictionary<string, CodeListEntryModel> _database = null!;

    [SetUp]
    public void Setup()
    {
        _database = new(StringComparer.Ordinal);
        _concepts = Substitute.For<IIopConceptsService>();
        _client = Substitute.For<IIndexSearchApiClient>();
        _logger = Substitute.For<ILogger<CodeListEntryIndexSearch>>();
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<GetFilterConfigurationCommand>(), Arg.Any<CancellationToken>())
            .Returns(new FilterConfigurationModel { Filters = [] });
        _subject = new(_client, _concepts, mediator, _logger);

        // Match the real bulk service: a requested code missing from the database throws.
        _concepts.GetCodeListEntriesByCodes(_conceptId, Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<IEnumerable<string>>().Select(code => _database[code]).ToArray());
        _concepts.GetCodeListEntryByCode(_conceptId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(call => _database.TryGetValue(call.Arg<string>(), out var entry)
                ? entry
                : throw new NotFoundException("Parent code not found."));
        _concepts.GetCodeListEntriesByIds(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(call => _database.Values.Where(entry => call.Arg<IEnumerable<Guid>>().Contains(entry.Id)).ToArray());
    }

    [Test]
    public async Task Missing_indexed_ancestors_are_resolved_from_database_parent_links()
    {
        Entry("C5");
        Entry("21", "C5");
        var child = Entry("21.1", "21");

        (await Path(child)).Should().Equal("C5", "21", "21.1");
    }

    [Test]
    public async Task Complete_indexed_ancestors_need_no_individual_parent_queries()
    {
        Entry("C5");
        Entry("21", "C5");
        var child = Entry("21.1", "21");

        (await Path(child, ["21", "C5"])).Should().Equal("C5", "21", "21.1");
        await _concepts.DidNotReceive().GetCodeListEntryByCode(
            Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Stale_ancestors_are_not_included_after_reparenting()
    {
        Entry("A");
        Entry("B");
        var child = Entry("X", "B");

        (await Path(child, ["A", "B"])).Should().Equal("B", "X");
    }

    [Test]
    public async Task Removed_indexed_ancestor_does_not_fail_the_search()
    {
        Entry("B");
        var child = Entry("X", "B");

        (await Path(child, ["deleted", "B"])).Should().Equal("B", "X");
        HasWarning().Should().BeTrue();
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Missing_database_parent_is_reported_and_removed_from_the_breadcrumb_root(bool indexed)
    {
        var child = Entry("21", "missing");

        (await Path(child, indexed ? ["missing"] : [])).Should().Equal("21");
        child.ParentCode.Should().Be("missing");
        HasWarning().Should().BeTrue();
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Cycles_are_reported_and_the_returned_path_is_acyclic(bool indexed)
    {
        var child = Entry("A", "B");
        Entry("B", "A");

        (await Path(child, indexed ? ["B", "A"] : [])).Should().Equal("B", "A");
        HasWarning().Should().BeTrue();
    }

    [Test]
    public async Task Valid_hierarchies_deeper_than_64_ancestors_are_not_truncated()
    {
        Entry("c0");
        for (var level = 1; level <= 70; level++)
        {
            Entry($"c{level}", $"c{level - 1}");
        }

        var path = await Path(_database["c70"]);
        path.Should().Equal(Enumerable.Range(0, 71).Select(level => $"c{level}"));
        HasWarning().Should().BeFalse();
    }

    [Test]
    public async Task Searches_without_paths_do_not_load_parents()
    {
        var child = Entry("21", "missing");

        (await Path(child, addPaths: false)).Should().BeEmpty();
        await _concepts.DidNotReceive().GetCodeListEntryByCode(
            Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Unrelated_parent_lookup_errors_are_not_silenced()
    {
        var child = Entry("21", "C5");
        _concepts.GetCodeListEntryByCode(_conceptId, "C5", Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromException<CodeListEntryModel>(new InvalidOperationException("Database unavailable.")));

        Func<Task> search = async () => await Path(child);
        await search.Should().ThrowAsync<InvalidOperationException>();
    }

    private CodeListEntryModel Entry(string code, string? parent = null)
    {
        var entry = new CodeListEntryModel
        {
            Id = Guid.NewGuid(),
            ConceptId = _conceptId,
            Code = code,
            ParentCode = parent,
            Name = new MultiLanguageModel { De = code },
        };
        _database.Add(code, entry);
        return entry;
    }

    private async Task<string[]> Path(CodeListEntryModel entry, string[]? ancestors = null, bool addPaths = true)
    {
        ICollection<CodeListSearchHit> hits =
        [
            new()
            {
                Id = entry.Id,
                ConceptId = _conceptId,
                Code = entry.Code,
                AncestorCodes = ancestors ?? [],
            },
        ];
        _client.PostSearchCodelistsAllByBodyAsync(Arg.Any<CodeListSearchRequest>(), Arg.Any<CancellationToken>())
            .Returns(new SwaggerResponse<ICollection<CodeListSearchHit>>(
                200, new Dictionary<string, IEnumerable<string>>(), hits));

        var response = await _subject.SearchAsync(_conceptId, "de", null, [], addPaths, null, null);
        var path = response.Results.Single().Path.ToArray();
        if (addPaths)
        {
            path.Should().NotBeEmpty();
            path[0].ParentCode.Should().BeNull();
            path.Select(x => x.Code).Should().OnlyHaveUniqueItems();
            for (var index = 1; index < path.Length; index++)
            {
                path[index].ParentCode.Should().Be(path[index - 1].Code);
            }
        }

        return path.Select(x => x.Code).ToArray();
    }

    private bool HasWarning() => _logger.ReceivedCalls()
        .Any(call => call.GetMethodInfo().Name == nameof(ILogger.Log)
            && call.GetArguments()[0] is LogLevel.Warning);
}
