using MediatR;

namespace Bfs.Iop.Admin.Commands.IdentifierExists;

public class GetByConceptIdentifierCommand : IRequest<bool>
{
    public GetByConceptIdentifierCommand(string identifier)
    {
        Identifier = identifier;
    }

    public string Identifier { get; set; }
}