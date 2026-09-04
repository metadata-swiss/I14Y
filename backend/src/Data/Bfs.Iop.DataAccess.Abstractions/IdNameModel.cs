namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record IdNameModel : IdModel
{
    public MultiLanguageModel? Name { get; init; }
}
