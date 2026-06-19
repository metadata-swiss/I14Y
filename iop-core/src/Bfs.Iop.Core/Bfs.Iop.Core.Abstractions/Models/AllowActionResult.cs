using System.Net;

namespace Bfs.Iop.Core.Abstractions.Models;

public record AllowActionResult
{
    public AllowActionType ActionType { get; init; }

    public bool Value { get; init; }

    public string? Message { get; init; }

    public int? MessageDetailsCode { get; init; }

    public AllowActionResourceType? ResourceType { get; init; }
}