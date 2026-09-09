namespace Bfs.Iop.Admin.Models;

public sealed record ActiveDirectoryUser
{
    public string DisplayName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Firstname {  get; init; } = string.Empty;

    public string Lastname { get; init; } = string.Empty;

    public string? OrgUnitName { get; init; }
}
