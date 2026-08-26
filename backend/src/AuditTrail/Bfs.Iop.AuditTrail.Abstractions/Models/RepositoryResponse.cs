namespace Bfs.Iop.AuditTrail.Abstractions.Models;

public sealed record RepositoryResponse(
    bool Success, 
    int ExitCode,
    string StdOut,
    string? StdErr = null)
{ }
