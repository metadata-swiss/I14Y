using AwesomeAssertions;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.DataAccess.Relational.Services;
using Bfs.Iop.DataAccess.Relational.UnitTests.Helpers;
using NSubstitute;

namespace Bfs.Iop.DataAccess.Relational.UnitTests.Services;

[TestFixture(TestOf = typeof(SearchIndexProviderService))]
internal sealed class SearchIndexProviderServiceCodeListTests
{
    private IopDbContext _dbContext = null!;
    private SearchIndexProviderService _subject = null!;

    [SetUp]
    public void Setup()
    {
        _dbContext = TestHelper.CreateFakeIopDbContext();
        _subject = new SearchIndexProviderService(
            _dbContext,
            Substitute.For<IVocabulariesService>());
    }

    [TearDown]
    public void Teardown()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Parent_codes_are_loaded_without_tracking_for_full_and_concept_indexing(bool fullRebuild)
    {
        var concept = EntitiesHelper.IopConcept;
        var root = EntitiesHelper.CodeListEntry;
        root.IopConceptId = concept.Id;
        root.Code = "C5";
        var parent = EntitiesHelper.CodeListEntry;
        parent.IopConceptId = concept.Id;
        parent.Code = "21";
        parent.ParentCodeListEntryId = root.Id;
        var child = EntitiesHelper.CodeListEntry;
        child.IopConceptId = concept.Id;
        child.Code = "21.1";
        child.ParentCodeListEntryId = parent.Id;

        _dbContext.IopConcepts.Add(concept);
        _dbContext.CodeListEntries.AddRange(root, parent, child);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        var entries = new List<CodeListEntryModel>();
        if (fullRebuild)
        {
            await foreach (var batch in _subject.GetCodeListEntriesInBatches(batchSize: 1))
            {
                entries.AddRange(batch);
            }
        }
        else
        {
            entries.AddRange(await _subject.GetCodeListEntriesByConcept(concept.Id));
        }

        entries.Should().HaveCount(3);
        entries.Single(x => x.Code == "C5").ParentCode.Should().BeNull();
        entries.Single(x => x.Code == "21").ParentCode.Should().Be("C5");
        entries.Single(x => x.Code == "21.1").ParentCode.Should().Be("21");
        _dbContext.ChangeTracker.Entries().Should().BeEmpty();
    }
}
