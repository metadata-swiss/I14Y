using Bfs.Iop.DataAccess.Abstractions;
using MediatR;
using System;

namespace Bfs.Iop.Admin.Commands.ConceptView;

public sealed record GetRegistrationStatusByIdCommand(Guid ConceptId) : IRequest<RegistrationStatusInfoModel>
{ }
