using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Services.Contracts;
using NSubstitute;

namespace Bfs.Iop.Core.IndexForwarding.UnitTests;

/// <summary>
/// The write path has no local index behind it: enqueueing IS the write. These assertions pin that
/// every mutation reaches the queue with the right target.
/// <para>
/// <b>The two delete tests are the load-bearing ones.</b>
/// <c>ICatalogIndexWriter.DeIndexAsync</c> and <c>ICodeListEntryIndexWriter.DeIndexAsync</c> have
/// identical signatures, so <see cref="ForwardingIndexWriter"/> implements them explicitly — one per
/// contract — and nothing but these tests proves the two were not crossed. A crossover fails
/// silently in production: the enqueue succeeds, the POST succeeds, and the documents simply never
/// leave the index they were in.
/// </para>
/// <para>
/// The fields are typed as the <b>interfaces</b>, not the class, deliberately: an explicitly
/// implemented member is not callable on the concrete type, and holding the interface is also how
/// every real caller reaches it.
/// </para>
/// </summary>
[TestFixture]
public sealed class ForwardingIndexWriterTests
{
    private IIndexSearchDispatcher _dispatcher = null!;
    private ICatalogIndexWriter _catalog = null!;
    private ICodeListEntryIndexWriter _codeList = null!;

    [SetUp]
    public void SetUp()
    {
        _dispatcher = Substitute.For<IIndexSearchDispatcher>();

        var writer = new ForwardingIndexWriter(_dispatcher, NullLogger<ForwardingIndexWriter>.Instance);
        _catalog = writer;
        _codeList = writer;
    }

    private static CodeListEntryModel Entry() => new()
    {
        Id = Guid.NewGuid(),
        ConceptId = Guid.NewGuid(),
        Code = "code-1",
        Name = new MultiLanguageModel { De = "Eintrag" },
    };

    [Test]
    public async Task CatalogDeIndexAsync_EnqueuesOneCatalogDeletePerId()
    {
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();

        await _catalog.DeIndexAsync([first, second]);

        _dispatcher.Received(1).Enqueue(Arg.Is<IndexForwardItem>(
            x => x.Target == IndexForwardTarget.CatalogDelete && x.Id == first));
        _dispatcher.Received(1).Enqueue(Arg.Is<IndexForwardItem>(
            x => x.Target == IndexForwardTarget.CatalogDelete && x.Id == second));

        // The half that catches a crossover: nothing may go to the code-list index.
        _dispatcher.DidNotReceive().Enqueue(Arg.Is<IndexForwardItem>(
            x => x.Target == IndexForwardTarget.CodeListEntryDelete));
    }

    [Test]
    public async Task CodeListDeIndexAsync_EnqueuesOneCodeListDeletePerId()
    {
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();

        await _codeList.DeIndexAsync([first, second]);

        _dispatcher.Received(1).Enqueue(Arg.Is<IndexForwardItem>(
            x => x.Target == IndexForwardTarget.CodeListEntryDelete && x.Id == first));
        _dispatcher.Received(1).Enqueue(Arg.Is<IndexForwardItem>(
            x => x.Target == IndexForwardTarget.CodeListEntryDelete && x.Id == second));

        _dispatcher.DidNotReceive().Enqueue(Arg.Is<IndexForwardItem>(
            x => x.Target == IndexForwardTarget.CatalogDelete));
    }

    [Test]
    public async Task IndexAsync_EnqueuesOneItemPerEntry()
    {
        await _codeList.IndexAsync([Entry(), Entry()]);

        _dispatcher.Received(2).Enqueue(Arg.Is<IndexForwardItem>(
            x => x.Target == IndexForwardTarget.CodeListEntries && x.Model != null));
    }

    [Test]
    public async Task UpdateIndexAsync_EnqueuesOneItemPerEntry()
    {
        await _codeList.UpdateIndexAsync([Entry()]);

        _dispatcher.Received(1).Enqueue(Arg.Is<IndexForwardItem>(
            x => x.Target == IndexForwardTarget.CodeListEntries));
    }
}
