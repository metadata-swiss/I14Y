using AwesomeAssertions;
using Bfs.Iop.Core.Abstractions.Models.Indexing;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.LinkedData.Services;
using Bfs.Iop.IndexSearch.Api.Indexing;
using Bfs.Iop.Search.Abstractions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Bfs.Iop.IndexSearch.Api.UnitTests;

/// <summary>
/// hasStructure cannot ride along on a forwarded dataset — it is not part of DcatDatasetModel — so
/// this service has to resolve it from the object store.
/// <para>
/// It matters for exactly the two operations that change the flag: importing a dataset model and
/// deleting one. Core knows the new value but has no way to send it, so if these assertions ever
/// stop holding, a structure you just uploaded silently fails to appear in the Structures facet
/// (and one you just deleted keeps appearing) until the next full rebuild hours later. Nothing
/// throws, and nothing is logged — which is why it is pinned here.
/// </para>
/// </summary>
[TestFixture]
public sealed class IndexReconcilerHasStructureTests
{
    private static readonly Guid DatasetId = Guid.Parse("11111111-2222-3333-4444-555555555555");

    private ICatalogIndexService _catalogIndex = null!;
    private IDatasetModelProcessService _datasetModels = null!;
    private IndexReconciler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _catalogIndex = Substitute.For<ICatalogIndexService>();
        _datasetModels = Substitute.For<IDatasetModelProcessService>();

        _sut = new IndexReconciler(
            _catalogIndex,
            Substitute.For<ICodeListEntryIndexService>(),
            _datasetModels,
            NullLogger<IndexReconciler>.Instance);
    }

    private async Task ReconcileDatasetAsync()
    {
        var entry = new CatalogIndexEntry
        {
            Id = DatasetId,
            Type = SearchResourceType.Dataset,
            Identifier = "ds-1",
            PublisherId = Guid.NewGuid(),
            PublisherIdentifier = "CH_BFS",
            PublicationLevel = PublicationLevel.Public,
            RegistrationStatus = RegistrationStatus.Recorded,
            CreatedAt = DateTimeOffset.UnixEpoch,
        };

        await _sut.ReconcileAsync(
            [new IndexEvent(IndexTarget.Catalog, DatasetId, entry)],
            CancellationToken.None);
    }

    [Test]
    public async Task AStructureInTheObjectStore_IsIndexedAsTrue()
    {
        _datasetModels.GraphExists(DatasetId, Arg.Any<CancellationToken>()).Returns(true);

        await ReconcileDatasetAsync();

        await _catalogIndex.Received(1).UpdateIndexAsync(
            Arg.Is<IEnumerable<CatalogIndexEntry>>(x => x.Single().HasStructure == true), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// The delete-model case: false must be written, not left alone, or the dataset keeps claiming a
    /// structure it no longer has.
    /// </summary>
    [Test]
    public async Task NoStructureInTheObjectStore_IsIndexedAsFalse()
    {
        _datasetModels.GraphExists(DatasetId, Arg.Any<CancellationToken>()).Returns(false);

        await ReconcileDatasetAsync();

        await _catalogIndex.Received(1).UpdateIndexAsync(
            Arg.Is<IEnumerable<CatalogIndexEntry>>(x => x.Single().HasStructure == false), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// An unreachable object store must PRESERVE the indexed value (null), never clear it. Returning
    /// false here would let one outage strip the flag from every dataset edited during it, and the
    /// only symptom would be a quietly emptying facet.
    /// </summary>
    [Test]
    public async Task AnUnreachableObjectStore_PreservesTheIndexedValue()
    {
        _datasetModels.GraphExists(DatasetId, Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException("object store unreachable"));

        await ReconcileDatasetAsync();

        await _catalogIndex.Received(1).UpdateIndexAsync(
            Arg.Is<IEnumerable<CatalogIndexEntry>>(x => x.Single().HasStructure == null), Arg.Any<CancellationToken>());
    }
}
