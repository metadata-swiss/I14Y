using System;

using System.Collections.Generic;

namespace Bfs.Iop.Admin.Models;

public class ChannelView
{
    public MultiLanguage Address { get; set; } = null!;

    public MultiLanguage Description { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Fax { get; set; } = null!;

    public Guid Id { get; set; }

    public string Identifier { get; set; } = null!;

    public string Mobile { get; set; } = null!;

    public string OpeningHours { get; set; } = null!;

    public IEnumerable<Agent> OwnedBy { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public VocabularyEntry? Type { get; set; } = null!;

    public string Url { get; set; } = null!;
}