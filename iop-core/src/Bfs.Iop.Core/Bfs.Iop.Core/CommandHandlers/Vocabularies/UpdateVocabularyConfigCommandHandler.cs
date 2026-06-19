using Bfs.Iop.Core.Abstractions.Commands.Vocabularies;
using Bfs.Iop.Core.Services.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Vocabularies;

internal sealed class UpdateVocabularyConfigCommandHandler : IRequestHandler<UpdateVocabularyConfigCommand>
{
    private readonly IVocabulariesService _vocabularyService;

    public UpdateVocabularyConfigCommandHandler(IVocabulariesService vocabularyService) =>
        _vocabularyService = vocabularyService;

    public Task Handle(UpdateVocabularyConfigCommand request, CancellationToken cancellationToken)
    {
        return _vocabularyService.UpdateVocabularyConfig(request.Id, request.Model, cancellationToken);
    }
}