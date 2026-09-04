namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record MappingRelationInputModel
{
    public required UriInputModel Source { get; set; }

    public required UriInputModel Target { get; set; }

    public required CodeInputModel RelationType { get; set; }
}
