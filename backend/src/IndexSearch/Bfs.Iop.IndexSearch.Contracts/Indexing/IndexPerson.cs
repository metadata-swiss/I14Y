namespace Bfs.Iop.IndexSearch.Contracts.Indexing;

public sealed record IndexPerson
{
    public string? GivenName { get; init; }

    public string? FamilyName { get; init; }

    public string? Email { get; init; }
}
