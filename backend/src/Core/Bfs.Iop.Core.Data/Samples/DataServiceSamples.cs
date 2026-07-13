using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Entities;

namespace Bfs.Iop.Core.Data.Samples;

internal static class DataServiceSamples
{
    public static readonly Guid ExampleDataService2024Id = new("965e457f-bee9-4c0a-87ec-7c0a621afc42");
    public static readonly Guid ExampleDataService2025Id = new("59c99d1e-1111-4d03-9574-07bbb748b5b7");

    public static IEnumerable<DataService> Generate()
    {
        var exampleDataService2024 = new DataService
        {
            Id = ExampleDataService2024Id,
            Identifiers = [ExampleDataService2024Id.ToString()],
            Version = "2024.1",
            PublisherId = AgentSamples.I14YTestId,
            AccessRights = "PUBLIC",
            Description = new MultiLanguage
            {
                De = "Beispiel-Datendienst für Test- und Seed-Daten.",
                Fr = "Service de données d’exemple pour les tests et les données de seed.",
                It = "Servizio dati di esempio per test e dati seed.",
                En = "Example data service for tests and seed data."
            },
            Theme =
            [
                "115",
                "109",
                "116"
            ],
            Title = new MultiLanguage
            {
                De = "Beispiel-Datendienst",
                Fr = "Service de données d’exemple",
                It = "Servizio dati di esempio",
                En = "Example data service"
            },
            Keyword =
            [
                new Keyword
                {
                    Text = new MultiLanguage
                    {
                        De = "Beispiel",
                        Fr = "Exemple",
                        It = "Esempio",
                        En = "Example"
                    }
                },
                new Keyword
                {
                    Text = new MultiLanguage
                    {
                        De = "Datendienst",
                        Fr = "Service de données",
                        It = "Servizio dati",
                        En = "Data service"
                    }
                }
            ],
            ContactPoint =
            [
                new VCard
                {
                    Kind = VCardKind.Organization,
                    EmailInternet = "example@bfs.admin.ch",
                    AdrWork = new MultiLanguage
                    {
                        De = "Bundesamt für Statistik\nEspace de l'Europe 10\nCH-2010 Neuchâtel\nSchweiz",
                        Fr = "Office fédéral de la statistique\nEspace de l'Europe 10\nCH-2010 Neuchâtel\nSuisse",
                        It = "Ufficio federale di statistica\nEspace de l'Europe 10\nCH-2010 Neuchâtel\nSvizzera",
                        En = "Federal Statistical Office\nEspace de l'Europe 10\nCH-2010 Neuchâtel\nSwitzerland"
                    },
                    Fn = new MultiLanguage
                    {
                        De = "Bundesamt für Statistik",
                        Fr = "Office fédéral de la statistique",
                        It = "Ufficio federale di statistica",
                        En = "Federal Statistical Office"
                    },
                    Note = new MultiLanguage
                    {
                        De = "Beispielkontakt",
                        Fr = "Contact d’exemple",
                        It = "Contatto di esempio",
                        En = "Example contact"
                    }
                }
            ],
            EndpointUrl =
            [
                new Resource
                {
                    Href = "https://example.admin.ch/api/data-service"
                }
            ],
            LandingPage =
            [
                new Resource
                {
                    Href = "https://example.admin.ch/data-service"
                }
            ],
            PublicationLevel = PublicationLevel.Public,
            RegistrationStatus = RegistrationStatus.Recorded,
        };

        return [exampleDataService2024];
    }
}