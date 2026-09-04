using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.DataAccess.Relational.Entities;

internal abstract class PublishableEntityBase : EntityBase, IMainEntity, IOwnedEntity
{
    public PublicationLevel PublicationLevel { get; set; } = PublicationLevel.Internal;

    public PublicationLevel? PublicationLevelProposal { get; set; }

    public Agent Publisher { get; set; } = null!;

    public Guid PublisherId { get; set; }

    public RegistrationStatus RegistrationStatus { get; set; } = RegistrationStatus.Incomplete;

    public RegistrationStatus? RegistrationStatusProposal { get; set; }
}