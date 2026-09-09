using Bfs.Iop.Core.Abstractions.Commands.Vocabularies;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Vocabularies;

internal sealed class DeleteVocabularyConfigCommandHandler : IRequestHandler<DeleteVocabularyConfigCommand>
{
    private readonly IVocabulariesService _vocabularyService;

    public DeleteVocabularyConfigCommandHandler(IVocabulariesService vocabularyService) => 
        _vocabularyService = vocabularyService;

    public Task Handle(DeleteVocabularyConfigCommand request, CancellationToken cancellationToken)
    {
        return _vocabularyService.DeleteVocabularyConfig(request.Id, cancellationToken);
    }
}
