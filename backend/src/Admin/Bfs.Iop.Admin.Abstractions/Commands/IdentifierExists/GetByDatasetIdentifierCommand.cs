using MediatR;

namespace Bfs.Iop.Admin.Commands.IdentifierExists;

public class GetByDatasetIdentifierCommand : IRequest<bool>
{
    public GetByDatasetIdentifierCommand(string identifier)
    {
        Identifier = identifier;
    }

    public string Identifier { get; set; }
}