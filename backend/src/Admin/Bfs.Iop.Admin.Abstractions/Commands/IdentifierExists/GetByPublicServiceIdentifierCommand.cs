using MediatR;

namespace Bfs.Iop.Admin.Commands.IdentifierExists;

public class GetByPublicServiceIdentifierCommand : IRequest<bool>
{
    public GetByPublicServiceIdentifierCommand(string identifier)
    {
        Identifier = identifier;
    }

    public string Identifier { get; set; }
}