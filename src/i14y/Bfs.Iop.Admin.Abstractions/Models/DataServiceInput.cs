using Bfs.Iop.Core.Abstractions.Models;
using System;
using System.Collections.Generic;

namespace Bfs.Iop.Admin.Models;

public class DataServiceInput
{
    public string AccessRightCode { get; set; } = null!;

    public IEnumerable<Resource> ConformTos { get; set; } = [];

    public IEnumerable<Vcard> ContactPoints { get; set; } = [];

    public MultiLanguage Description { get; set; } = null!;

    public IEnumerable<Resource> Documents { get; set; } = [];

    public IEnumerable<Resource> EndpointDescriptions { get; set; } = [];

    public IEnumerable<Resource> EndpointUrls { get; set; } = [];

    public Guid Id { get; set; }

    public IEnumerable<string> Identifiers { get; init; } = [];

    public DateTimeOffset? Issued { get; init; }

    public IEnumerable<KeywordModel> Keywords { get; set; } = [];

    public IEnumerable<Resource> LandingPages { get; set; } = [];

    public VocabularyEntry? License { get; set; }

    public DateTimeOffset? Modified { get; init; }

    public IdModel? PreviousVersion { get; init; }

    public Agent Publisher { get; set; } = null!;

    public EmailInputModel? ResponsibleDeputy { get; init; }

    public EmailInputModel? ResponsiblePerson { get; init; }

    public IEnumerable<IdModel> ServesDatasets { get; init; } = [];

    public IEnumerable<string> ThemeCodes { get; set; } = [];

    public MultiLanguage Title { get; set; } = null!;

    public string? Version { get; set; }

    public MultiLanguage? VersionNotes { get; set; }
}