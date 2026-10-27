using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Core.CommandHandlers.PublishableTypes;

internal interface IPublishableResourceNotifier
{
    Task NotifyUpdatedAsync(PublishableResourceType type, Guid id, CancellationToken cancellationToken);
}
