using Bfs.Iop.Core.Abstractions.Commands.DcatCatalogs;
using Bfs.Iop.Core.CommandHandlers.DcatCatalogs;
using Bfs.Iop.Core.Messaging.AuditTrail;
using Bfs.Iop.Core.Messaging.SearchIndex;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using Cerise;
using NSubstitute;

namespace Bfs.Iop.Core.UnitTests.Commands.DcatCatalogs;

[TestFixture]
internal sealed class DcatCatalogSearchIndexNotificationTests
{
    private static readonly Guid CatalogId = Guid.NewGuid();
    private static readonly Guid RecordId = Guid.NewGuid();

    private IDcatCatalogsService _dcatCatalogsService = null!;
    private IAuditTrailNotifierService _auditTrail = null!;
    private ISearchIndexNotifierService _searchIndex = null!;
    private ISearchIndexProviderService _searchIndexProvider = null!;

    [SetUp]
    public void SetUp()
    {
        _dcatCatalogsService = Substitute.For<IDcatCatalogsService>();
        _auditTrail = Substitute.For<IAuditTrailNotifierService>();
        _searchIndex = Substitute.For<ISearchIndexNotifierService>();
        _searchIndexProvider = Substitute.For<ISearchIndexProviderService>();
    }

    private static DcatCatalogResourceModel Topic(Guid id, DcatCatalogType type = DcatCatalogType.Dataset) =>
        new() { ResourceId = id, ResourceType = type };

    private static DcatCatalogRecordInputModel Input(DcatCatalogResourceModel topic) => new() { PrimaryTopic = topic };

    private void GivenStoredRecord(DcatCatalogResourceModel topic) =>
        _dcatCatalogsService.GetDcatCatalogRecord(CatalogId, RecordId, Arg.Any<CancellationToken>())
            .Returns(new DcatCatalogRecordModel { Id = RecordId, DcatCatalogId = CatalogId, PrimaryTopic = topic });

    [Test]
    public async Task Recording_datasets_reindexes_each_dataset_once_and_no_data_service()
    {
        var dataset = Guid.NewGuid();
        var dataService = Guid.NewGuid();

        var handler = new CreateDcatCatalogRecordsCommandHandler(_dcatCatalogsService, _auditTrail, _searchIndex);

        await handler.Handle(
            new CreateDcatCatalogRecordsCommand(CatalogId, [Input(Topic(dataset)), Input(Topic(dataset)), Input(Topic(dataService, DcatCatalogType.DataService))]),
            CancellationToken.None);

        await _searchIndex.Received(1).NotifyResourceChangedAsync(SearchResourceType.Dataset, dataset, Arg.Any<CancellationToken>());
        await _searchIndex.DidNotReceive().NotifyResourceChangedAsync(Arg.Any<SearchResourceType>(), dataService, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Moving_a_record_to_another_dataset_reindexes_both_datasets()
    {
        var before = Guid.NewGuid();
        var after = Guid.NewGuid();
        GivenStoredRecord(Topic(before));

        IRequestHandler<UpdateDcatCatalogRecordCommand> handler =
            new UpdateDcatCatalogRecordCommandHandler(_dcatCatalogsService, _auditTrail, _searchIndex);

        await handler.Handle(new UpdateDcatCatalogRecordCommand(CatalogId, RecordId, Input(Topic(after))), CancellationToken.None);

        await _searchIndex.Received(1).NotifyResourceChangedAsync(SearchResourceType.Dataset, before, Arg.Any<CancellationToken>());
        await _searchIndex.Received(1).NotifyResourceChangedAsync(SearchResourceType.Dataset, after, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Deleting_a_record_reindexes_the_dataset_it_recorded()
    {
        var dataset = Guid.NewGuid();
        GivenStoredRecord(Topic(dataset));

        IRequestHandler<DeleteDcatCatalogRecordCommand> handler =
            new DeleteDcatCatalogRecordCommandHandler(_dcatCatalogsService, _auditTrail, _searchIndex);

        await handler.Handle(new DeleteDcatCatalogRecordCommand(CatalogId, RecordId), CancellationToken.None);

        Received.InOrder(() =>
        {
            _dcatCatalogsService.DeleteDcatCatalogRecord(CatalogId, RecordId, Arg.Any<CancellationToken>());
            _searchIndex.NotifyResourceChangedAsync(SearchResourceType.Dataset, dataset, Arg.Any<CancellationToken>());
        });
    }

    [Test]
    public async Task Deleting_a_catalogue_reindexes_every_dataset_it_recorded_after_the_delete()
    {
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();

        _searchIndexProvider.GetDatasetIdsByDcatCatalog(CatalogId, Arg.Any<CancellationToken>()).Returns([first, second]);

        IRequestHandler<DeleteDcatCatalogCommand> handler =
            new DeleteDcatCatalogCommandHandler(_dcatCatalogsService, _auditTrail, _searchIndex, _searchIndexProvider);

        await handler.Handle(new DeleteDcatCatalogCommand(CatalogId), CancellationToken.None);

        Received.InOrder(() =>
        {
            _dcatCatalogsService.DeleteDcatCatalog(CatalogId, Arg.Any<CancellationToken>());
            _searchIndex.NotifyResourceChangedAsync(SearchResourceType.Dataset, first, Arg.Any<CancellationToken>());
            _searchIndex.NotifyResourceChangedAsync(SearchResourceType.Dataset, second, Arg.Any<CancellationToken>());
        });
    }
}
