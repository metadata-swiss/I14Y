using System.Collections.Generic;

namespace Bfs.Iop.Admin.Models;

public sealed class FilterCountResult
{
    public List<FilterCountResultItem> AccessRights { get; set; } = [];

    public List<FilterCountResultItem> AttributedAgents { get; set; } = [];

    public List<FilterCountResultItem> BusinessEvents { get; set; } = [];

    public List<FilterCountResultItem> ConceptValueTypes { get; set; } = [];

    public List<FilterCountResultItem> Formats { get; set; } = [];

    public List<FilterCountResultItem> LifeEvents { get; set; } = [];

    public List<FilterCountResultItem> PublicationLevelProposals { get; set; } = [];

    public List<FilterCountResultItem> PublicationLevels { get; set; } = [];

    public List<FilterCountResultItem> Publishers { get; set; } = [];

    public List<FilterCountResultItem> RegistrationStatuses { get; set; } = [];

    public List<FilterCountResultItem> RegistrationStatusProposals { get; set; } = [];

    public List<FilterCountResultItem> Structures { get; set; } = [];

    public List<FilterCountResultItem> Themes { get; set; } = [];

    public uint TotalDocCount { get; set; }

    public List<FilterCountResultItem> Types { get; set; } = [];
}