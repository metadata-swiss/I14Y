using Bfs.Iop.Core.Abstractions.Commands.Vocabularies;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Vocabularies;

internal sealed class GetVocabularyConfigCommandHandler : IRequestHandler<GetVocabularyConfigCommand, VocabularyConfigModel>
{
    private readonly IVocabulariesService _vocabularyService;

    public GetVocabularyConfigCommandHandler(IVocabulariesService vocabularyService) => 
        _vocabularyService = vocabularyService;

    public async Task<VocabularyConfigModel> Handle(GetVocabularyConfigCommand request, CancellationToken cancellationToken) => 
        await _vocabularyService.GetVocabularyConfig(request.Id, cancellationToken);
}
