using AutoMapper;
using Bfs.Iop.Admin.Commands.IopPerson;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.ApiClient;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.Commands.IopPerson;

internal class SearchIopPersonsByQueryCommandHandler : IRequestHandler<
    SearchIopPersonsByQueryCommand,
    IEnumerable<ActiveDirectoryUser>>
{
    private readonly IMapper _mapper;
    private readonly IIopCoreApiClient _dcatApiClient;

    public SearchIopPersonsByQueryCommandHandler(
        IMapper mapper,
        IIopCoreApiClient dcatApiClient)
    {
        _mapper = mapper;
        _dcatApiClient = dcatApiClient;
    }

    public async Task<IEnumerable<ActiveDirectoryUser>> Handle(
        SearchIopPersonsByQueryCommand request,
        CancellationToken cancellationToken)
    {
        var response = await _dcatApiClient.GetPersonsSearchByQueryAndPageAndPageSizeAsync(
            request.Query, 
            page: 1,
            pageSize: int.MaxValue, 
            cancellationToken);

        return _mapper.Map<IEnumerable<ActiveDirectoryUser>>(response.Result);
    }
}