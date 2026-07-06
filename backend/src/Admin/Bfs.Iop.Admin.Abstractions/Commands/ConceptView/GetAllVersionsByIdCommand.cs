using Bfs.Iop.Admin.Models;
using MediatR;
using System;
using System.Collections.Generic;

namespace Bfs.Iop.Admin.Commands.ConceptView;

public record GetAllVersionsByIdCommand(Guid ConceptId) : IRequest<IEnumerable<ConceptVersionView>>
{ }