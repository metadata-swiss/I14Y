using Bfs.Iop.Core.Abstractions.Commands.IopConcepts;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Models.Search;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.IopConcepts;

internal sealed class GetCodeListEntryParentsByCodeCommandHandler 
    : IRequestHandler<GetCodeListEntryParentsByCodeCommand, IEnumerable<CodeListEntrySearchResultPathModel>>
{
    private readonly IIopConceptsService _iopConceptsService;

    public GetCodeListEntryParentsByCodeCommandHandler(IIopConceptsService conceptsService) 
        => _iopConceptsService = conceptsService ?? throw new ArgumentNullException(nameof(conceptsService));

    public async Task<IEnumerable<CodeListEntrySearchResultPathModel>> Handle(
        GetCodeListEntryParentsByCodeCommand request,
        CancellationToken cancellationToken)
    {

        var codeListEntry = await _iopConceptsService.GetCodeListEntryByCode(request.ConceptId, request.Code, cancellationToken);

        return await CreatePath([], request.ConceptId, codeListEntry, cancellationToken);
    }

    private async Task<IEnumerable<CodeListEntrySearchResultPathModel>> CreatePath(
        List<CodeListEntrySearchResultPathModel> paths,
        Guid conceptId,
        CodeListEntryModel codeListEntryModel,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(codeListEntryModel.ParentCode))
        {
            var parentCodeListEntryModel = await _iopConceptsService.GetCodeListEntryByCode(conceptId, codeListEntryModel.ParentCode, cancellationToken);

            await CreatePath(paths, conceptId, parentCodeListEntryModel, cancellationToken);
        }

        paths.Add(new CodeListEntrySearchResultPathModel() { Code = codeListEntryModel.Code, Name = codeListEntryModel.Name, ParentCode = codeListEntryModel.ParentCode });

        return paths;
    }
}
