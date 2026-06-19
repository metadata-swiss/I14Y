using Bfs.Iop.Core.Abstractions.Models.FilterConfigurations;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.FilterConfigurations;

public sealed record GetFilterConfigurationCommand(Guid Id) : IRequest<FilterConfigurationModel>;