using Bfs.Iop.Core.Abstractions.Commands.DcatCatalogs;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Contracts;
using Bfs.Iop.Core.Services.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.DcatCatalogs;

internal sealed class GetDcatCatalogThemesCommandHandler : IRequestHandler<GetDcatCatalogThemesCommand, PagedResult<DcatCatalogThemeModel>>
{
    private readonly IDcatCatalogsService _dcatCatalogsService;
    private readonly IVocabulariesService _vocabularyService;

    public GetDcatCatalogThemesCommandHandler(
        IDcatCatalogsService dcatCatalogsService,
        IVocabulariesService vocabulariesService)
    {
        _dcatCatalogsService = dcatCatalogsService ?? throw new ArgumentNullException(nameof(dcatCatalogsService));
        _vocabularyService = vocabulariesService ?? throw new ArgumentNullException(nameof(vocabulariesService));
    }

    public async Task<PagedResult<DcatCatalogThemeModel>> Handle(GetDcatCatalogThemesCommand request, CancellationToken cancellationToken)
    {
        var dcatCatalog = await _dcatCatalogsService.GetDcatCatalog(request.DcatCatalogId, cancellationToken);

        (var page, var pageSize) = request.Page.HasValue && request.PageSize.HasValue
            ? (request.Page.Value, request.PageSize.Value)
            : (1, int.MaxValue);

        var entries = dcatCatalog.ThemeTaxonomy
            .SelectMany(identifier =>
            {
                var entries = _vocabularyService.GetVocabulary(identifier, cancellationToken).GetAwaiter().GetResult().Entries;

                return entries.Select(x => new DcatCatalogThemeModel()
                {
                    Code = x.Code,
                    Name = x.Name,
                    ThemeTaxonomy = identifier,
                    Uri = x.Uri,
                });
            })
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new()
        {
            Page = page,
            PageSize = pageSize is int.MaxValue ? entries.Count : pageSize,
            Results = entries,
            TotalCount = entries.Count
        };

        //var records = await _dcatCatalogsService.GetDcatCatalogRecords(request.DcatCatalogId, 1, int.MaxValue, cancellationToken);

        //(var page, var pageSize) = request.Page.HasValue && request.PageSize.HasValue
        //    ? (request.Page.Value, request.PageSize.Value)
        //    : (1, int.MaxValue);

        //var themes = records.Results
        //    .SelectMany(x => x.Themes)
        //    .Skip((page - 1) * pageSize)
        //    .Take(pageSize)
        //    .ToList();

        //return new()
        //{
        //    Page = page,
        //    PageSize = pageSize is int.MaxValue ? themes.Count : pageSize,
        //    Results = themes,
        //    TotalCount = themes.Count
        //};
    }
}
