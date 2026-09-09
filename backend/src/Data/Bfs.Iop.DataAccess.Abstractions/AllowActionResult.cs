namespace Bfs.Iop.DataAccess.Abstractions;

public record AllowActionResult
{
    public AllowActionType ActionType { get; init; }

    public bool Value { get; init; }

    public string? Message { get; init; }

    public int? MessageDetailsCode { get; init; }

    public AllowActionResourceType? ResourceType { get; init; }
}