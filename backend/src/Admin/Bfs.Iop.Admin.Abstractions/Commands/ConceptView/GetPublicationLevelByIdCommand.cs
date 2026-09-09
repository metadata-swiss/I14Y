using Bfs.Iop.DataAccess.Abstractions;
using MediatR;
using System;

namespace Bfs.Iop.Admin.Commands.ConceptView;

public sealed record GetPublicationLevelByIdCommand(Guid ConceptId) : IRequest<PublicationLevelInfoModel>
{ }