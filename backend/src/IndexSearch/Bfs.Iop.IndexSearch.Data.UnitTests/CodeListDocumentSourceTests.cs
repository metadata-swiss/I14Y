using AwesomeAssertions;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.IndexSearch.Contracts.Indexing;
using NSubstitute;

namespace Bfs.Iop.IndexSearch.Data.UnitTests;

// The database stores one parent code per entry; the index stores the whole chain, because a filter
// on "everything under 01" has to match a grandchild without walking the tree per hit. Flattening it
// wrongly is invisible in the document and only shows up as entries missing from a filtered search.
[TestFixture]
internal sealed class CodeListDocumentSourceTests
{
    private static readonly Guid Concept = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid OtherConcept = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    [Test]
    public async Task An_entry_with_no_parent_has_no_ancestors()
    {
        var documents = await ReadAsync(Entry("01"));

        documents.Single().AncestorCodes.Should().BeEmpty();
    }

    [Test]
    public async Task A_direct_parent_is_the_only_ancestor()
    {
        var documents = await ReadAsync(Entry("01"), Entry("01.02", parent: "01"));

        Codes(documents, "01.02").Should().Equal("01");
    }

    [Test]
    public async Task Every_level_above_an_entry_is_flattened_nearest_first()
    {
        var documents = await ReadAsync(
            Entry("01"),
            Entry("01.02", parent: "01"),
            Entry("01.02.03", parent: "01.02"),
            Entry("01.02.03.04", parent: "01.02.03"));

        // Nearest first: the order is what a breadcrumb renders, and the front end does not sort it.
        Codes(documents, "01.02.03.04").Should().Equal("01.02.03", "01.02", "01");
    }

    [Test]
    public async Task A_parent_code_is_only_followed_inside_its_own_concept()
    {
        // Two code lists routinely use the same codes. Keying the parent lookup on the code alone
        // would splice one concept's hierarchy onto another's and leak entries across code lists.
        var documents = await ReadAsync(
            Entry("01", concept: OtherConcept),
            Entry("01.02", parent: "01", concept: OtherConcept),
            Entry("01.02.03", parent: "01.02"));

        Codes(documents, "01.02.03").Should().Equal("01.02");
    }

    [Test]
    public async Task A_cycle_in_the_data_terminates_instead_of_hanging_the_rebuild()
    {
        // Nothing in the schema forbids it, and a rebuild that spins here never finishes and never
        // reports why.
        var documents = await ReadAsync(
            Entry("A", parent: "B"),
            Entry("B", parent: "A"));

        Codes(documents, "A").Should().Equal("B", "A");
    }

    [Test]
    public async Task A_chain_longer_than_the_cap_stops_at_the_cap()
    {
        // 70 levels of a chain that is not cyclic but is deeper than anything real.
        var entries = new List<CodeListEntryModel> { Entry("c0") };

        for (var level = 1; level <= 70; level++)
        {
            entries.Add(Entry($"c{level}", parent: $"c{level - 1}"));
        }

        var documents = await ReadAsync([.. entries]);

        Codes(documents, "c70").Should().HaveCount(64);
    }

    [Test]
    public async Task A_parent_that_does_not_exist_ends_the_chain_rather_than_dropping_it()
    {
        // A dangling parent code is still the entry's parent, so it belongs in the ancestors even
        // though the walk cannot continue past it.
        var documents = await ReadAsync(Entry("01.02", parent: "missing"));

        Codes(documents, "01.02").Should().Equal("missing");
    }

    private static IReadOnlyList<string> Codes(IReadOnlyList<CodeListIndexDocument> documents, string code) =>
        documents.Single(x => x.Code == code).AncestorCodes;

    private static async Task<IReadOnlyList<CodeListIndexDocument>> ReadAsync(params CodeListEntryModel[] entries)
    {
        var provider = Substitute.For<ISearchIndexProviderService>();

        // The source enumerates this twice: once to build the parent lookup, once to emit documents.
        provider.GetCodeListEntriesInBatches(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(_ => Batches(entries));

        var documents = new List<CodeListIndexDocument>();

        await foreach (var batch in new CodeListDocumentSource(provider).ReadAllAsync(batchSize: 100))
        {
            documents.AddRange(batch);
        }

        return documents;
    }

    private static async IAsyncEnumerable<List<CodeListEntryModel>> Batches(CodeListEntryModel[] entries)
    {
        await Task.CompletedTask;

        yield return [.. entries];
    }

    private static CodeListEntryModel Entry(string code, string? parent = null, Guid? concept = null) => new()
    {
        Id = Guid.NewGuid(),
        ConceptId = concept ?? Concept,
        Code = code,
        ParentCode = parent,
        Name = new MultiLanguageModel { De = code },
    };
}
