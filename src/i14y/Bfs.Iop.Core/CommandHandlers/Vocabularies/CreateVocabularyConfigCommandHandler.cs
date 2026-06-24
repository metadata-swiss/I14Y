using Bfs.Iop.Core.Abstractions.Commands.Vocabularies;
using Bfs.Iop.Core.Services.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Vocabularies;

internal sealed class CreateVocabularyConfigCommandHandler : IRequestHandler<CreateVocabularyConfigCommand, Guid>
{
    private readonly IVocabulariesService _vocabularyService;

    public CreateVocabularyConfigCommandHandler(IVocabulariesService vocabularyService) => 
        _vocabularyService = vocabularyService;

    public Task<Guid> Handle(CreateVocabularyConfigCommand request, CancellationToken cancellationToken) => 
        _vocabularyService.AddVocabularyConfig(request.Model, cancellationToken);
}
