using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.AuditTrail.Business.Extensions;
using Bfs.Iop.AuditTrail.Business.Helpers;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Bfs.Iop.AuditTrail.Business.Services;

internal sealed class GitCommitProcessorService : IGitCommitProcessorService
{
    private readonly IGitWrapper _gitWrapper;
    private readonly IResourceDataReaderService _resourceReaderService;
    private readonly string _repositoryPath;

    private readonly ILogger<GitCommitProcessorService> _logger;

    public GitCommitProcessorService(
        IGitWrapper gitWrapper,
        IResourceDataReaderService resourceDataReaderService,
        ILogger<GitCommitProcessorService> logger)
    {
        _gitWrapper = gitWrapper;
        _resourceReaderService = resourceDataReaderService;
        _repositoryPath = gitWrapper.GitOptions.RepositoryPath;
        _logger = logger;
    }

    public async Task<RepositoryResponse> ProcessCommitAsync(CommitRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var commitMessage = new StringBuilder();

        foreach (var item in request.ResourceChanges)
        {
            var message = item.Operation switch
            {
                ResourceChangeOperation.Add => await ProcessAddOrUpdateResourceAsync(item, cancellationToken),
                ResourceChangeOperation.Update => await ProcessAddOrUpdateResourceAsync(item, cancellationToken),
                ResourceChangeOperation.Delete => ProcessDeleteResource(item),
                _ => throw new NotSupportedException($"The value '{item.Operation}' is not supported.")
            };

            commitMessage.Append(message);
        }        
        
        if (!string.IsNullOrWhiteSpace(request.CustomMessage))
        {
            commitMessage.Append(request.CustomMessage);
        }

        var response = await _gitWrapper.ExecuteAsync(["add", "."], cancellationToken);

        if (!response.Success)
        {
            return response;
        }

        var commitArgs = new List<string>()
        {
            "commit",
            "--author",
            $"{request.Author.Name} <{request.Author.Email}>"
        };

        commitArgs.AddRange(
            "--date",
            DateTime.UtcNow.ToString("O"));

        commitArgs.AddRange(
            "-m",
            $"{commitMessage}");


       return await _gitWrapper.ExecuteAsync([.. commitArgs], cancellationToken); 
    }

    private async Task<string> ProcessAddOrUpdateResourceAsync(ResourceChange resourceChange, CancellationToken cancellationToken)
    {
        var filePath = GetFilepath(resourceChange.ResourceMetadata);

        var operation = File.Exists(filePath)
            ? ResourceChangeOperation.Update
            : ResourceChangeOperation.Add;

        if (resourceChange.ResourceData is not null)
        {
            await File.WriteAllTextAsync(filePath, resourceChange.ResourceData, cancellationToken);
        }
        else
        {
            using var data = await _resourceReaderService.GetResourceDataAsync(
                resourceChange.ResourceMetadata.ResourceType,
                resourceChange.ResourceMetadata.Id,
                cancellationToken);

            using var file = File.Create(filePath);
            await data.CopyToAsync(file, cancellationToken);
        }

        return GitCommitHelper.GenerateCommitMessage(
            operation.ToString(),
            resourceChange.ResourceMetadata);
    }

    private string ProcessDeleteResource(ResourceChange resourceChange)
    {
        var filePath = GetFilepath(resourceChange.ResourceMetadata);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return GitCommitHelper.GenerateCommitMessage(
            ResourceChangeOperation.Delete.ToString(),
            resourceChange.ResourceMetadata);
    }

    private string GetFilepath(ResourceMetadata resourceMetadata) =>
        Path.Combine(_repositoryPath, resourceMetadata.GetFilename());
}
