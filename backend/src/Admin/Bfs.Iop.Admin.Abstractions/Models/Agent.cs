using Bfs.Iop.Core.Abstractions.Models;
using System;
using System.Collections.Generic;

namespace Bfs.Iop.Admin.Models;

public class Agent
{
    public VocabularyEntry? Classification { get; set; }

    public VCardModel? ContactPoint { get; set; }

    public MultiLanguageModel? Description { get; set; }

    public string? HomePage { get; set; }

    public Guid Id { get; set; }

    public string? Identifier { get; set; }

    public IEnumerable<ResourceModel> Images { get; init; } = [];

    public MultiLanguage Name { get; set; } = null!;

    public MultiLanguage PrefLabel { get; set; } = null!;

    public IEnumerable<string> Spatial { get; set; } = [];

    public IEnumerable<VocabularyEntry> SpatialCH { get; set; } = [];

    public IEnumerable<IdNameModel> SubAgents { get; set; } = [];

    public IEnumerable<IdNameModel>? SubAgentOf { get; set; }

    public SystemInfoModel? System { get; set; }

    public string? Uid { get; set; }
}