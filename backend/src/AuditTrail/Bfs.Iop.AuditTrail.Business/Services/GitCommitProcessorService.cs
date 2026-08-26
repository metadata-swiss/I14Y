using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.AuditTrail.Business.Extensions;
using Bfs.Iop.AuditTrail.Business.Helpers;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Threading.Channels;

namespace Bfs.Iop.AuditTrail.Business.Services;

internal sealed class GitCommitProcessorService
{
    private readonly IGitWrapper _gitWrapper;
    private readonly string _repositoryPath;
    private readonly Channel<CommitRequest> _commitQueue;

    private readonly ILogger<GitCommitProcessorService> _logger;

    public GitCommitProcessorService(IGitWrapper gitWrapper, ILogger<GitCommitProcessorService> logger)
    {
        _gitWrapper = gitWrapper;
        _repositoryPath = gitWrapper.GitOptions.RepositoryPath;
        _logger = logger;

        _commitQueue = Channel.CreateBounded<CommitRequest>(new BoundedChannelOptions(1000)
        {
            SingleReader = true,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.Wait
        });
    }

    public ValueTask EnqueueAsync(
        CommitRequest commit,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(commit, nameof(commit));

        return _commitQueue.Writer.WriteAsync(commit, cancellationToken);
    }

    public async Task ProcessQueueAsync(CancellationToken cancellationToken = default)
    {
        await foreach (var commit in _commitQueue.Reader.ReadAllAsync(cancellationToken))
        {
            try
            {
                var response = await ProcessCommitAsync(commit, cancellationToken);

                if (!response.Success)
                {
                    _logger.LogWarning("Something unexpected happened while processing commit: {Message}", response.StdErr);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing commit: {Message}", ex.Message);
            }
        }
    }

    private async Task<RepositoryResponse> ProcessCommitAsync(CommitRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var commitMessage = new StringBuilder();

        foreach (var item in request.ResourceChanges)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var message = item.Operation switch
            {
                ResourceChangeOperation.AddOrUpdate => ProcessAddOrUpdateResource(item),
                ResourceChangeOperation.Delete => ProcessDeleteResource(item),
                _ => throw new NotSupportedException($"The value '{item.Operation}' is not supported.")
            };

            commitMessage.Append(message);
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

        if (request.TimeStamp.HasValue)
        {
            commitArgs.AddRange(
                "--date",
                request.TimeStamp.Value.ToString("O"));
        }

        commitArgs.AddRange(
            "-m",
            $"{commitMessage}");


       return await _gitWrapper.ExecuteAsync([.. commitArgs], cancellationToken); 
    }

    private string ProcessAddOrUpdateResource(ResourceChange resourceChange)
    {
        var filePath = GetFilepath(resourceChange.ResourceMetadata);

        var operation = File.Exists(filePath)
            ? GitCommitHelper.OperationMessageTags.Update
            : GitCommitHelper.OperationMessageTags.Add;

        File.WriteAllText(filePath, resourceChange.Data);

        return GitCommitHelper.GenerateCommitMessage(
            operation,
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
            GitCommitHelper.OperationMessageTags.Delete,
            resourceChange.ResourceMetadata);
    }

    private string GetFilepath(ResourceMetadata resourceMetadata) =>
        Path.Combine(_repositoryPath, resourceMetadata.GetFilename());
}
