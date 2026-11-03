using Bfs.Iop.Core.Abstractions.Models.FilterConfigurations;
using Cerise;

namespace Bfs.Iop.Core.Abstractions.Commands.FilterConfigurations;

public sealed record GetFilterConfigurationCommand(Guid Id) : IRequest<FilterConfigurationModel>;