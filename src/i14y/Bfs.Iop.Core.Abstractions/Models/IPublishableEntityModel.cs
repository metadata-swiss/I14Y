namespace Bfs.Iop.Core.Abstractions.Models;

public interface IPublishableEntityModel
{
    public MultiLanguageModel Description { get; }

    public Guid Id { get; }

    public IEnumerable<KeywordModel> Keywords { get; }

    public PublicationLevel PublicationLevel { get; }

    public PublicationLevel? PublicationLevelProposal { get; }

    public AgentModel Publisher { get; }

    public RegistrationStatus RegistrationStatus { get; }

    public RegistrationStatus? RegistrationStatusProposal { get; }

    public SystemInfoModel System { get; }
}
