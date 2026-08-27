using Bfs.Iop.Core.Abstractions.Models;
using Microsoft.Extensions.Logging;
using Bfs.Iop.Core.Services.Contracts;

namespace Bfs.Iop.Core.IndexForwarding;

/// <summary>
/// IOP Core's whole write path to the search index: every write is turned into a queued
/// <see cref="IndexForwardItem"/> and forwarded to the standalone IndexSearch service. Core holds no
/// index of its own.
/// <para>
/// Enqueueing is a non-blocking <c>TryWrite</c> onto a bounded channel, so a user's save never waits
/// on the index and an IndexSearch outage never blocks a write to Postgres. The queue is not durable
/// — the receiving service rebuilds from Postgres on a schedule, which is what makes dropping
/// acceptable.
/// </para>
/// <para>
/// One class for both indexes because the job is identical for each: map a call to an
/// <see cref="IndexForwardTarget"/> and enqueue. The catalog and code-list halves were separate
/// classes until they had nothing left in them but that mapping.
/// </para>
/// </summary>
internal sealed class ForwardingIndexWriter : ICatalogIndexWriter, ICodeListEntryIndexWriter
{
    private readonly IIndexSearchDispatcher _dispatcher;
    private readonly ILogger<ForwardingIndexWriter> _logger;

    public ForwardingIndexWriter(IIndexSearchDispatcher dispatcher, ILogger<ForwardingIndexWriter> logger)
    {
        _dispatcher = dispatcher;
        _logger = logger;
    }

    public Task UpdateIndexAsync(DcatDatasetModel model, bool? hasStructure = null, CancellationToken cancellationToken = default)
    {
        // hasStructure is not part of the model and so cannot be forwarded. The receiving service
        // resolves it per dataset from the object store instead, and preserves the indexed value if
        // that read fails — so the Structures facet stays correct without this argument crossing the
        // wire.
        ForwardProjected(IndexForwardTarget.Catalog, [model], IndexEntryProjection.FromDataset);

        return Task.CompletedTask;
    }

    public Task UpdateIndexAsync(IEnumerable<PublicServiceModel> models, CancellationToken cancellationToken = default)
    {
        ForwardProjected(IndexForwardTarget.Catalog, models, IndexEntryProjection.FromPublicService);

        return Task.CompletedTask;
    }

    public Task UpdateIndexAsync(IEnumerable<DataServiceModel> models, CancellationToken cancellationToken = default)
    {
        ForwardProjected(IndexForwardTarget.Catalog, models, IndexEntryProjection.FromDataService);

        return Task.CompletedTask;
    }

    public Task UpdateIndexAsync(IEnumerable<IopConceptModel> models, CancellationToken cancellationToken = default)
    {
        ForwardProjected(IndexForwardTarget.Catalog, models, IndexEntryProjection.FromConcept);

        return Task.CompletedTask;
    }

    public Task UpdateIndexAsync(IEnumerable<MappingTableModel> models, CancellationToken cancellationToken = default)
    {
        ForwardProjected(IndexForwardTarget.Catalog, models, IndexEntryProjection.FromMappingTable);

        return Task.CompletedTask;
    }

    public Task IndexAsync(IEnumerable<CodeListEntryModel> codeListEntries, CancellationToken cancellationToken = default)
    {
        ForwardProjected(IndexForwardTarget.CodeListEntries, codeListEntries, IndexEntryProjection.FromCodeListEntry);

        return Task.CompletedTask;
    }

    public Task UpdateIndexAsync(IEnumerable<CodeListEntryModel> codeListEntries, CancellationToken cancellationToken = default)
    {
        ForwardProjected(IndexForwardTarget.CodeListEntries, codeListEntries, IndexEntryProjection.FromCodeListEntry);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Removes resources from the <b>catalog</b> index.
    /// <para>
    /// <b>Explicit interface implementation, and it must stay that way.</b>
    /// <see cref="ICatalogIndexWriter.DeIndexAsync"/> and
    /// <see cref="ICodeListEntryIndexWriter.DeIndexAsync"/> have identical signatures —
    /// <c>Task DeIndexAsync(IEnumerable&lt;Guid&gt;, CancellationToken)</c> — so a single implicit
    /// method would satisfy both contracts while being unable to tell which index the caller meant,
    /// and would route half of all deletes to the wrong endpoint. That fails silently: the enqueue
    /// succeeds, the POST succeeds, and the documents simply never leave the index they were in.
    /// Merging these two back into one ordinary method reintroduces exactly that bug.
    /// </para>
    /// </summary>
    Task ICatalogIndexWriter.DeIndexAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        ForwardDeletes(IndexForwardTarget.CatalogDelete, ids);

        return Task.CompletedTask;
    }

    /// <inheritdoc cref="ICatalogIndexWriter.DeIndexAsync"/>
    Task ICodeListEntryIndexWriter.DeIndexAsync(IEnumerable<Guid> codeListEntriesIds, CancellationToken cancellationToken)
    {
        ForwardDeletes(IndexForwardTarget.CodeListEntryDelete, codeListEntriesIds);

        return Task.CompletedTask;
    }

    private void ForwardDeletes(IndexForwardTarget target, IEnumerable<Guid> ids)
    {
        ArgumentNullException.ThrowIfNull(ids);

        foreach (var id in ids)
        {
            _dispatcher.Enqueue(new IndexForwardItem { Target = target, Id = id });
        }
    }

    private void Forward<T>(IndexForwardTarget target, T entry)
        where T : class =>
        _dispatcher.Enqueue(new IndexForwardItem { Target = target, Model = entry });

    /// <summary>
    /// Projects one resource, swallowing a projection failure rather than letting it reach the caller.
    /// <para>
    /// Load-bearing. Projection now happens HERE, on the request thread, where it used to happen on
    /// the far side of the queue — so a model this code cannot flatten would throw inside the user's
    /// save. That breaks the rule the whole forwarding design rests on: a save must never fail
    /// because indexing did. The resource is logged and skipped, and the periodic full rebuild
    /// repairs the index from the database.
    /// </para>
    /// </summary>
    private void ForwardProjected<TModel>(
        IndexForwardTarget target,
        IEnumerable<TModel> models,
        Func<TModel, object> project)
    {
        ArgumentNullException.ThrowIfNull(models);

        foreach (var model in models)
        {
            object entry;
            try
            {
                entry = project(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "A {Type} could not be projected for indexing and was skipped. It stays out of " +
                    "search until the next full rebuild.",
                    typeof(TModel).Name);

                continue;
            }

            Forward(target, entry);
        }
    }
}
