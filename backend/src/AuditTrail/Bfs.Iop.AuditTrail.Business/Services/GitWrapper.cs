using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.AuditTrail.Business.Configuration;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace Bfs.Iop.AuditTrail.Business.Services;

internal sealed class GitWrapper
{
    public GitOptions GitOptions { get; }

    public GitWrapper(IOptions<GitOptions> gitOptions) => GitOptions = gitOptions.Value;

    public async Task<RepositoryResponse> ExecuteAsync(string[] arguments, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(arguments, nameof(arguments));

        var psi = new ProcessStartInfo
        {
            FileName = "git",
            WorkingDirectory = GitOptions.RepositoryPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        foreach (var arg in arguments.Where(x => !string.IsNullOrWhiteSpace(x)))
        {
            psi.ArgumentList.Add(arg);
        }

        using var process = Process.Start(psi)!;

        var stdout = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderr = await process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        return new RepositoryResponse(
            process.ExitCode == 0,
            process.ExitCode,
            stdout,
            stderr
        );
    }

    public Task<bool> RepositoryExistsAsync(CancellationToken _)
    {
        var gitPath = Path.Combine(GitOptions.RepositoryPath, ".git");
        return Task.FromResult(Directory.Exists(gitPath));
    }

    public async Task<RepositoryResponse> InitializeRepositoryAsync(CancellationToken cancellationToken)
    {
        var exists = await RepositoryExistsAsync(cancellationToken);

        if (exists)
        {
            return new RepositoryResponse(false, ExitCode: 409, "Repository already exists.");
        }

        return await ExecuteAsync(["init"], cancellationToken);
    }
}
