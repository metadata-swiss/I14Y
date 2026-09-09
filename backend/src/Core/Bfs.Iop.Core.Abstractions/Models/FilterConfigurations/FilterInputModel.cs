namespace Bfs.Iop.Core.Abstractions.Models.FilterConfigurations;

public sealed record FilterInputModel
{
    public required string FilterIdentifier { get; init; }

    public required List<string> Values { get; init; }
}