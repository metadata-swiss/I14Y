using AutoMapper;
using Bfs.Iop.Admin.Commands.DatasetInput;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.ApiClient;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Business.Commands.DatasetInput;

internal sealed class CreateVersionCommandHandler : IRequestHandler<CreateVersionCommand, Guid>
{
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly IIopCoreApiClient _apiClient;

    public CreateVersionCommandHandler(
        IMediator mediator,
        IMapper mapper,
        IIopCoreApiClient apiClient)
    {
        _mediator = mediator;
        _mapper = mapper;
        _apiClient = apiClient;
    }

    public async Task<Guid> Handle(CreateVersionCommand request, CancellationToken cancellationToken)
    {
        var inputModel = await _mediator.Send(new GetDcatDatasetInputModelCommand(request.Model.PreviousVersionId), cancellationToken);

        // Remove the ids from DistributionInputModels in order to create new objects
        var distributions = new List<DcatDistributionInputModel>();

        foreach (var item in inputModel.Distributions)
        {
            distributions.Add(item with { Id = null });
        }

        inputModel = inputModel with
        {
            Description = _mapper.Map<MultiLanguageModel>(request.Model.Description),
            Distributions = distributions,
            Identifiers = [request.Model.Identifier],
            PreviousVersion = new IdModel() { Id = request.Model.PreviousVersionId },
            Title = _mapper.Map<MultiLanguageModel>(request.Model.Title),
            Version = request.Model.Version,
            VersionNotes = request.Model.Title is not null
                ? _mapper.Map<MultiLanguageModel>(request.Model.Title)
                : default
        };

        return (await _apiClient.PostDatasetsByBodyAsync(inputModel, cancellationToken)).Result;
    }
}