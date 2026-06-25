using MediatR;
using System;

namespace Bfs.Iop.Admin.Commands.ConceptInput;

public class GetCountDataElementReferenceCommand : IRequest<int>
{
    public GetCountDataElementReferenceCommand(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; set; }
}