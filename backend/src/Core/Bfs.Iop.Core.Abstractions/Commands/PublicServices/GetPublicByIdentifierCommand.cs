using Bfs.Iop.DataAccess.Abstractions;
using Cerise;

namespace Bfs.Iop.Core.Abstractions.Commands.PublicServices;

public sealed record GetPublicServiceByIdentifierCommand(string Identifier) : IRequest<PublicServiceModel>
{ }
