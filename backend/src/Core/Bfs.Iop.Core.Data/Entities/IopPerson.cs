namespace Bfs.Iop.Core.Data.Entities;

internal class IopPerson : EntityBase, IMainEntity
{
    public required string GivenName { get; set; }

    public required string FamilyName { get; set; }

    public required string Email { get; set; }

    public DateOnly FirstLoginDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);

    public DateOnly LastLoginDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
}