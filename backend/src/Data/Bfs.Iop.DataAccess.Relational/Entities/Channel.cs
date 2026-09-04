namespace Bfs.Iop.DataAccess.Relational.Entities;

internal class Channel : EntityBase
{
    public MultiLanguage? Address { get; set; }

    public MultiLanguage? Description { get; set; }

    public string? Email { get; set; }

    public string? Fax { get; set; }

    public string Identifier { get; set; } = null!;

    public string? Mobile { get; set; }

    public string? OpeningHours { get; set; }

    public List<ChannelOwnedBy> OwnedBy { get; set; } = null!;

    public string? Phone { get; set; }

    public PublicService PublicService { get; set; } = null!;

    public Guid PublicServiceId { get; set; }

    public string? Type { get; set; }

    public string? Url { get; set; }
}