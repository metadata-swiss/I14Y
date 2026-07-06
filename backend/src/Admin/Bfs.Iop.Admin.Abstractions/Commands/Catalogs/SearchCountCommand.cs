using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.Abstractions.Models;
using MediatR;

namespace Bfs.Iop.Admin.Commands.Catalogs;

public sealed class SearchCountCommand : IRequest<FilterCountResult>
{
    public string[] AccessRights { get; set; } = [];

    public string[] BusinessEvents { get; set; } = [];

    public string[] Formats { get; set; } = [];

    public string[] LifeEvents { get; set; } = [];

    public PublicationLevel?[] PublicationLevelProposals { get; set; } = [];

    public PublicationLevel[] PublicationLevels { get; set; } = [];

    public string[] Publishers { get; set; } = [];

    public string? Query { get; set; }

    public RegistrationStatus[] RegistrationStatuses { get; set; } = [];

    public RegistrationStatus?[] RegistrationStatusProposals { get; set; } = [];

    public SearchStructureOption? Structure { get; set; }

    public string[] Themes { get; set; } = [];

    public SearchResourceType[] Types { get; set; } = [];

    public ConceptType[] ConceptValueTypes { get; set; } = [];
}