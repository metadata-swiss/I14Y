namespace Bfs.Iop.Core.Abstractions.Models.Search;

public abstract record SearchResultEntryModel<T> where T : class
{
    public required T Entry { get; init; }

    public required float Score { get; init; }    
}