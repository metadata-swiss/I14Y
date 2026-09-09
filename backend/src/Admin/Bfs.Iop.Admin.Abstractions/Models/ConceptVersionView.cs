using Bfs.Iop.DataAccess.Abstractions;
using System;

namespace Bfs.Iop.Admin.Models;

public sealed class ConceptVersionView
{
    public Guid ConceptId { get; set; }

    public MultiLanguage Name { get; set; } = new();

    public ConceptType ConceptType { get; set; }

    public string Version { get; set; } = string.Empty;

    public DateTimeOffset? ValidFrom { get; set; }

    public DateTimeOffset? ValidTo { get; set; }

    public RegistrationStatus RegistrationStatus { get; set; }

    public PublicationLevel PublicationLevel { get; set; }
}
