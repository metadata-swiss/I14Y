namespace Bfs.Iop.Core.Abstractions.Models;

public sealed record IdNameModel : IdModel
{
    public MultiLanguageModel? Name { get; init; }
}
