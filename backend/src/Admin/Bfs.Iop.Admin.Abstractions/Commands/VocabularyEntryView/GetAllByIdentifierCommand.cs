using MediatR;
using System.Collections.Generic;

namespace Bfs.Iop.Admin.Commands.VocabularyEntryView;

public class GetAllByIdentifierCommand : IRequest<IEnumerable<Models.VocabularyEntry>>
{
    public GetAllByIdentifierCommand(string identifier)
    {
        Identifier = identifier;
    }

    public string Identifier { get; }
}