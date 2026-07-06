using System;

namespace Bfs.Iop.Admin.Models;

public class QualifiedRelation
{
    public Guid DatasetQualifiedRelationId { get; set; }

    public VocabularyEntry HadRole { get; set; } = new();

    public Guid Id { get; set; }

    public Resource Relation { get; set; } = null!;
}