using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Api.Indexing;

namespace Bfs.Iop.IndexSearch.Api.UnitTests;

/// <summary>
/// Coalescing exists so a burst of edits to one resource costs one Elasticsearch write, not one per
/// edit.
/// <para>
/// <b>Why these tests are shaped around distinct payload instances:</b> this previously used
/// <c>batch.Distinct()</c>. <see cref="IndexEvent"/> is a record whose <c>Payload</c> is
/// <c>object?</c>, so record equality compares payloads by reference — duplicate deletes (null
/// payload) collapsed, but two forwards of the same dataset never did, which is the case coalescing
/// is actually for. A test using the same payload instance twice would have passed against that
/// broken code. Every test below therefore uses <b>separate</b> payload objects.
/// </para>
/// </summary>
[TestFixture]
public sealed class IndexEventProcessorCoalesceTests
{
    private static readonly Guid ResourceA = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");
    private static readonly Guid ResourceB = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000002");

    /// <summary>The regression: two edits, two distinct payload objects, one resource.</summary>
    [Test]
    public void RepeatedWritesToOneResource_CollapseToOne()
    {
        var batch = new[]
        {
            new IndexEvent(IndexTarget.Catalog, ResourceA, new object()),
            new IndexEvent(IndexTarget.Catalog, ResourceA, new object()),
            new IndexEvent(IndexTarget.Catalog, ResourceA, new object()),
        };

        IndexEventProcessor.Coalesce(batch).Should().HaveCount(1);
    }

    /// <summary>The newest intent survives — a reconcile applies the payload it is given.</summary>
    [Test]
    public void TheLastPayloadWins()
    {
        var newest = new object();

        var batch = new[]
        {
            new IndexEvent(IndexTarget.Catalog, ResourceA, new object()),
            new IndexEvent(IndexTarget.Catalog, ResourceA, newest),
        };

        IndexEventProcessor.Coalesce(batch).Single().Payload.Should().BeSameAs(newest);
    }

    /// <summary>An edit followed by a delete must end as a delete, or the document is resurrected.</summary>
    [Test]
    public void UpdateThenDelete_CollapsesToTheDelete()
    {
        var batch = new[]
        {
            new IndexEvent(IndexTarget.Catalog, ResourceA, new object()),
            new IndexEvent(IndexTarget.Catalog, ResourceA, null),
        };

        IndexEventProcessor.Coalesce(batch).Single().Payload.Should().BeNull();
    }

    /// <summary>And the reverse order must end as the update, or a recreated resource stays missing.</summary>
    [Test]
    public void DeleteThenUpdate_CollapsesToTheUpdate()
    {
        var recreated = new object();

        var batch = new[]
        {
            new IndexEvent(IndexTarget.Catalog, ResourceA, null),
            new IndexEvent(IndexTarget.Catalog, ResourceA, recreated),
        };

        IndexEventProcessor.Coalesce(batch).Single().Payload.Should().BeSameAs(recreated);
    }

    [Test]
    public void DifferentResources_AreNotCollapsed()
    {
        var batch = new[]
        {
            new IndexEvent(IndexTarget.Catalog, ResourceA, new object()),
            new IndexEvent(IndexTarget.Catalog, ResourceB, new object()),
        };

        IndexEventProcessor.Coalesce(batch).Should().HaveCount(2);
    }

    /// <summary>
    /// Same id, different index. The catalog and code-list indexes are separate documents, so
    /// collapsing across them would drop one of the two writes entirely.
    /// </summary>
    [Test]
    public void SameIdOnDifferentTargets_IsNotCollapsed()
    {
        var batch = new[]
        {
            new IndexEvent(IndexTarget.Catalog, ResourceA, new object()),
            new IndexEvent(IndexTarget.CodeListEntry, ResourceA, new object()),
        };

        IndexEventProcessor.Coalesce(batch).Should().HaveCount(2);
    }
}
