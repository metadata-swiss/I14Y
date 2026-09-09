using Bfs.Iop.Admin.Models;
using MediatR;

namespace Bfs.Iop.Admin.Commands.ConceptInput;

public class CreateVersionCommand : IRequest<Models.ConceptInput>
{
    public CreateVersionCommand(ConceptInputCreateVersion model)
    {
        Model = model;
    }

    public ConceptInputCreateVersion Model { get; }
}