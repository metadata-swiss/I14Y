using MediatR;

namespace Bfs.Iop.Admin.Commands.IdentifierExists;

public class ExistsMoreThanOneConceptForIdentifierCommand : IRequest<bool>
{
    public ExistsMoreThanOneConceptForIdentifierCommand(string identifier)
    {
        Identifier = identifier;
    }

    public string Identifier { get; set; }
}