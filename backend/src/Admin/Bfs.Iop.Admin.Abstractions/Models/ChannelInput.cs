using System;

namespace Bfs.Iop.Admin.Models;

public class ChannelInput
{
    public MultiLanguage? Address { get; set; }

    public MultiLanguage? Description { get; set; }

    public string? Email { get; set; }

    public string? Fax { get; set; }

    public Guid? Id { get; set; }

    public required string Identifier { get; set; }

    public string? Mobile { get; set; }

    public string? OpeningHours { get; set; }

    public string? Phone { get; set; }

    public Guid PublicServiceId { get; set; }

    public VocabularyEntry? Type { get; set; }

    public string? Url { get; set; }
}