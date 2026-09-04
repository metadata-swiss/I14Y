using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.AuditTrail.Business.Extensions;
using Bfs.Iop.AuditTrail.Business.Helpers;

namespace Bfs.Iop.AuditTrail.Business.Services;

internal sealed class GitResourceTrackerService : IResourceTrackerService
{
    private readonly IGitWrapper _gitWrapper;
    private readonly string _repositoryPath;

    private readonly GitCommitProcessorService _gitCommitService;

    public GitResourceTrackerService(IGitWrapper gitWrapper, GitCommitProcessorService gitCommitService)
    {
        _gitWrapper = gitWrapper;
        _repositoryPath = gitWrapper.GitOptions.RepositoryPath;
        _gitCommitService = gitCommitService;
    }

    public Task<RepositoryResponse> InitRepositoryAsync(CancellationToken cancellationToken) => 
        _gitWrapper.InitializeRepositoryAsync(cancellationToken);

    public Task<bool> IsRepositoryInitializedAsync(CancellationToken cancellationToken) => 
        _gitWrapper.RepositoryExistsAsync(cancellationToken);

    public async Task<bool> IsResourceTrackedAsync(ResourceMetadata metadata, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(metadata, nameof(metadata));

        var filepath = GetFilepath(metadata);

        var exists = File.Exists(filepath);

        var args = new string[] 
        {
            "ls-files", 
            "--error-unmatch", 
            filepath
        };

        var tracked = await _gitWrapper.ExecuteAsync(args, cancellationToken);

        return exists && tracked.ExitCode is 0;
    }

    public async Task CommitAsync(CommitRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        await _gitCommitService.EnqueueAsync(request, cancellationToken);
    }

    public async Task<IEnumerable<Commit>> GetCommitsAsync(CommitSearchFilters filters, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filters, nameof(filters));

        var args = GitCommitHelper.GenerateSearchExpressionArguments(filters);

        var response = await _gitWrapper.ExecuteAsync(args, cancellationToken);

        var commits = GitCommitHelper.GenerateCommitsFromGitResponse(response);

        // Options -since and --until seem to not work properly,
        // so we need to filter the commits after getting them from Git.
        if (filters.From.HasValue)
        {
            commits = commits.Where(c => c.TimeStamp >= filters.From.Value);
        }

        if (filters.To.HasValue)
        {
            commits = commits.Where(c => c.TimeStamp <= filters.To.Value);
        }

        return commits;
    }

    private string GetFilepath(ResourceMetadata resourceMetadata) => 
        Path.Combine(_repositoryPath, resourceMetadata.GetFilename());
}
