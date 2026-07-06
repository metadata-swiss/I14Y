using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.FilterConfigurations;

public sealed record DeleteFilterConfigurationCommand(Guid Id) : IRequest;