using Bfs.Iop.Core.Abstractions.Commands.Vocabularies;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Services.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Vocabularies;

internal sealed class GetVocabularyCommandHandler : IRequestHandler<GetVocabularyCommand, VocabularyModel>
{
    private readonly IVocabulariesService _vocabularyService;

    public GetVocabularyCommandHandler(IVocabulariesService vocabularyService) =>
        _vocabularyService = vocabularyService;

    public Task<VocabularyModel> Handle(GetVocabularyCommand request, CancellationToken cancellationToken) =>
        _vocabularyService.GetVocabulary(request.VocabularyIdentifier, cancellationToken);
}
