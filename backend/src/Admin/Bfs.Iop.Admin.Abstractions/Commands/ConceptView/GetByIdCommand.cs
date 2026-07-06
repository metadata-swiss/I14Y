using MediatR;
using System;

namespace Bfs.Iop.Admin.Commands.ConceptView;

/// <summary>
/// Command to get out a concept based on the Id
/// </summary>
/// <param name="ConceptId">The Id of the concept</param>
public record GetByIdCommand(Guid ConceptId) : IRequest<Models.ConceptView>
{ }