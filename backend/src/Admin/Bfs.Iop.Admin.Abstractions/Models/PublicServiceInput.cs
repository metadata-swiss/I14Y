using Bfs.Iop.DataAccess.Abstractions;
using System;
using System.Collections.Generic;

namespace Bfs.Iop.Admin.Models;

public sealed class PublicServiceInput
{
    public IEnumerable<string> BusinessEventsCodes { get; set; } = [];

    public IEnumerable<ChannelInputModel> Channels { get; set; } = [];

    public IdentifierInputModel CompetentAuthority { get; set; } = null!;

    public MultiLanguage Description { get; set; } = null!;

    public Guid Id { get; set; }

    public IEnumerable<string> Identifiers { get; set; } = [];

    public IEnumerable<IdModel> IsDescribedAt { get; set; } = [];

    public IEnumerable<KeywordModel> Keywords { get; set; } = [];

    public IEnumerable<string> LanguageCodes { get; set; } = [];

    public IEnumerable<string> LifeEventsCodes { get; set; } = [];

    public IEnumerable<IdModel> Relations { get; set; } = [];

    public IEnumerable<IdModel> Requires { get; set; } = [];

    public EmailInputModel? ResponsibleDeputy { get; set; }

    public EmailInputModel? ResponsiblePerson { get; set; }

    public IEnumerable<string> SectorCodes { get; set; } = [];

    public string[] Spatial { get; set; } = [];

    public IEnumerable<string> ThematicAreaCodes { get; set; } = [];

    public MultiLanguage Title { get; set; } = null!;

    public IEnumerable<VocabularyEntry> SpatialCH { get; set; } = [];
}