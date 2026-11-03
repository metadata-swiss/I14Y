using Bfs.Iop.DataAccess.Abstractions;
using Cerise;
using System;

namespace Bfs.Iop.Admin.Commands.ConceptView;

public sealed record GetRegistrationStatusByIdCommand(Guid ConceptId) : IRequest<RegistrationStatusInfoModel>
{ }
