using Bfs.Iop.Core.Abstractions.Commands.Vocabularies;
using Bfs.Iop.DataAccess.Contracts;
using Cerise;

namespace Bfs.Iop.Core.CommandHandlers.Vocabularies;

internal sealed class UpdateVocabularyConfigCommandHandler : AsyncRequestHandler<UpdateVocabularyConfigCommand>
{
    private readonly IVocabulariesService _vocabularyService;

    public UpdateVocabularyConfigCommandHandler(IVocabulariesService vocabularyService) =>
        _vocabularyService = vocabularyService;

    protected override Task Handle(UpdateVocabularyConfigCommand request, CancellationToken cancellationToken)
    {
        return _vocabularyService.UpdateVocabularyConfig(request.Id, request.Model, cancellationToken);
    }
}