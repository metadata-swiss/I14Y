using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Relational.Entities;

namespace Bfs.Iop.DataAccess.Relational.Samples;

internal static class PublicServiceSamples
{
    public static readonly Guid MetadataId = new("965e457f-bee9-4c0a-87ec-111122223333");

    public static IEnumerable<PublicService> Generate()
    {
        var metadataService = new PublicService()
        {
            PublisherId = AgentSamples.I14YTestId,
            Description = new MultiLanguage
            {
                De = "Metadata_de",
                Fr = "Metadata_fr",
                It = "Metadata_it",
                En = "Metadata_en",
                Rm = "Metadata_rm"
            },
            Id = MetadataId,
            Identifiers = ["Identifier_MetaData"],
            Language = ["de", "fr", "it", "rm"],
            PublicationLevel = PublicationLevel.Public,
            RegistrationStatus = RegistrationStatus.Recorded,
            Spatial = ["Schweiz", "Kantone", "Gemeinden"],
            Title = new MultiLanguage
            {
                De = "Metadata Title de",
                Fr = "Metadata Title fr",
                It = "Metadata Title it",
                En = "Metadata Title en",
                Rm = "Metadata Title rm"
            },
            Channels =
            [
                new Channel()
                {
                    Description = new MultiLanguage
                    {
                        De = "Metadata email de",
                        Fr = "Metadata email fr"
                    },
                    Identifier = "channel_m_email",
                    Type = "1fc1caefa8",
                    Email = "fake.address@example.ch"
                },
                new Channel()
                {
                    Description = new MultiLanguage
                    {
                        De = "Metadata fax de",
                        Fr = "Metadata fax fr"
                    },
                    Identifier = "channel_m_fax",
                    OpeningHours = "Mo-Fr, 08:00-12:00",
                    Type = "a1c5444664",
                    Fax = "+41581111111"
                }
            ]
        };

        return [metadataService];
    }
}
