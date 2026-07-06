using Bfs.Iop.Core.Abstractions.Models;
using System;
using System.Collections.Generic;

namespace Bfs.Iop.Admin.Models;

public sealed class PublicServiceView
{
    public IEnumerable<VocabularyEntry> BusinessEvents { get; set; } = [];

    public IEnumerable<ChannelModel> Channels { get; set; } = [];

    public Agent CompetentAuthority { get; set; } = null!;

    public MultiLanguage Description { get; set; } = null!;

    public Guid Id { get; set; }

    public IEnumerable<string> Identifiers { get; set; } = [];

    public IEnumerable<IdLabel> IsDescribedAt { get; set; } = [];

    public IEnumerable<KeywordModel> Keywords { get; set; } = [];

    public IEnumerable<VocabularyEntry> Languages { get; set; } = [];

    public IEnumerable<VocabularyEntry> LifeEvents { get; set; } = [];

    public PublicationLevel PublicationLevel { get; set; }

    public PublicationLevel? PublicationLevelProposal { get; set; }

    public RegistrationStatus RegistrationStatus { get; set; }

    public RegistrationStatus? RegistrationStatusProposal { get; set; }

    public IEnumerable<IdLabel> Relation { get; set; } = [];

    public IEnumerable<IdLabel> Requires { get; set; } = [];

    public IopPersonModel? ResponsibleDeputy { get; set; }

    public IopPersonModel? ResponsiblePerson { get; set; }

    public IEnumerable<VocabularyEntry> Sectors { get; set; } = [];

    public string[] Spatial { get; set; } = null!;

    public SystemInfoModel? System { get; set; }

    public IEnumerable<VocabularyEntry> ThematicAreas { get; set; } = [];

    public MultiLanguage Title { get; set; } = null!;

    public IEnumerable<VocabularyEntry> SpatialCH { get; set; } = [];
}