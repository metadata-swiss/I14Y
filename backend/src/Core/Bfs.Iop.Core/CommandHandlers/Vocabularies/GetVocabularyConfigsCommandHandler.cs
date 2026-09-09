using Bfs.Iop.Core.Abstractions.Commands.Vocabularies;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Vocabularies;

internal sealed class GetVocabularyConfigsCommandHandler : IRequestHandler<GetVocabularyConfigsCommand, IEnumerable<VocabularyConfigModel>>
{
    private readonly IVocabulariesService _vocabularyService;

    public GetVocabularyConfigsCommandHandler(IVocabulariesService vocabularyService) => 
        _vocabularyService = vocabularyService;

    public Task<IEnumerable<VocabularyConfigModel>> Handle(GetVocabularyConfigsCommand request, CancellationToken cancellationToken) => 
        _vocabularyService.GetVocabularyConfigs(cancellationToken);
}
