using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.FilterConfigurations;

public sealed record CreateFilterConfigurationCommand(Guid Id, Stream Stream) : IRequest;