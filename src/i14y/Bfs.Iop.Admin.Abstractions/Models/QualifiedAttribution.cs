using System;

namespace Bfs.Iop.Admin.Models;

public class QualifiedAttribution
{
    public Agent Agent { get; set; } = null!;

    public Guid DatasetQualifiedAttributionId { get; set; }

    public VocabularyEntry HadRole { get; set; } = new();

    public Guid Id { get; set; }
}