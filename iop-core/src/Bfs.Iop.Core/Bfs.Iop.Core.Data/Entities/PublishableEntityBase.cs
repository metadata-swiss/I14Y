using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Core.Data.Entities;

internal abstract class PublishableEntityBase : EntityBase, IMainEntity, IOwnedEntity
{
    public PublicationLevel PublicationLevel { get; set; } = PublicationLevel.Internal;

    public PublicationLevel? PublicationLevelProposal { get; set; }

    public Agent Publisher { get; set; } = null!;

    public Guid PublisherId { get; set; }

    public RegistrationStatus RegistrationStatus { get; set; } = RegistrationStatus.Incomplete;

    public RegistrationStatus? RegistrationStatusProposal { get; set; }
}