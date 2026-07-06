using Bfs.Iop.Core.Abstractions.Models;
using MediatR;
using System;

namespace Bfs.Iop.Admin.Commands.ConceptView;

public sealed record GetRegistrationStatusByIdCommand(Guid ConceptId) : IRequest<RegistrationStatusInfoModel>
{ }
