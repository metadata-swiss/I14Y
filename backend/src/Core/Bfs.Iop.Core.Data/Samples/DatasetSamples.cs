using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Entities;

namespace Bfs.Iop.Core.Data.Samples;

internal static class DatasetSamples
{
    public static readonly Guid ExampleDatasetId = new("4f71b53b-8e67-46ca-90bf-c5fef19a9db0");
    public static readonly Guid ExampleDistributionId = new("d648590f-37cc-45aa-a1bf-99007efc45cc");

    public static IEnumerable<Dataset> Generate()
    {
        var exampleDataset = new Dataset
        {
            Id = ExampleDatasetId,
            Identifier = ["EXAMPLE_DATASET"],
            Issued = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Language = ["de", "fr", "it", "en"],
            Version = "1.0.0",
            VersionNotes = new MultiLanguage
            {
                De = "Beispieldatensatz für Seed-Daten.",
                Fr = "Jeu de données d’exemple pour les données de seed.",
                It = "Set di dati di esempio per dati seed.",
                En = "Example dataset for seed data."
            },
            PublisherId = AgentSamples.I14YTestId,
            AccessRights = "PUBLIC",
            Frequency = "ANNUAL",
            Description = new MultiLanguage
            {
                De = "Dieser Datensatz enthält fiktive Daten für Tests und Beispiele.",
                Fr = "Ce jeu de données contient des données fictives pour les tests et les exemples.",
                It = "Questo set di dati contiene dati fittizi per test ed esempi.",
                En = "This dataset contains fictional data for tests and examples."
            },
            Title = new MultiLanguage
            {
                De = "Beispieldatensatz",
                Fr = "Jeu de données d’exemple",
                It = "Set di dati di esempio",
                En = "Example dataset"
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
                        De = "Seed-Daten",
                        Fr = "Données de seed",
                        It = "Dati seed",
                        En = "Seed data"
                    }
                }
            ],
            ContactPoint =
            [
                new VCard
                {
                    Kind = VCardKind.Organization,
                    TelWorkVoice = "+41 58 000 00 00",
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
            ConformsTo =
            [
                new Resource
                {
                    Href = "https://example.admin.ch/standard",
                    Label = new MultiLanguage
                    {
                        En = "Example standard"
                    }
                }
            ],
            IsReferencedBy =
            [
                new Resource
                {
                    Href = "https://example.admin.ch/reference",
                    Label = new MultiLanguage
                    {
                        En = "Example reference"
                    }
                }
            ],
            Image =
            [
                new Resource
                {
                    Href = "https://example.admin.ch/image.png",
                    Label = new MultiLanguage
                    {
                        En = "Example image"
                    }
                }
            ],
            Relation =
            [
                new Resource
                {
                    Href = "https://example.admin.ch/related-dataset",
                    Label = new MultiLanguage
                    {
                        En = "Related example dataset"
                    }
                }
            ],
            Documentation =
            [
                new Resource
                {
                    Href = "https://example.admin.ch/documentation",
                    Label = new MultiLanguage
                    {
                        De = "Beispieldokumentation",
                        Fr = "Documentation d’exemple",
                        It = "Documentazione di esempio",
                        En = "Example documentation"
                    }
                }
            ],
            LandingPage =
            [
                new Resource
                {
                    Href = "https://example.admin.ch/dataset"
                }
            ],
            PublicationLevel = PublicationLevel.Public,
            RegistrationStatus = RegistrationStatus.Recorded,
            Theme =
            [
                "115",
                "109",
                "116"
            ],
            Distributions =
            [
                new Distribution
                {
                    Id = ExampleDistributionId,
                    DatasetId = ExampleDatasetId,
                    Format = "CSV",
                    Language = ["de", "fr", "it", "en"],
                    MediaType = "text/csv",
                    AccessUrl =
                    [
                        new Resource
                        {
                            Href = "https://example.admin.ch/dataset/example.csv"
                        }
                    ],
                    Documentation =
                    [
                        new Resource
                        {
                            Href = "https://example.admin.ch/dataset/example-csv-documentation",
                            Label = new MultiLanguage
                            {
                                De = "Dokumentation der CSV-Distribution",
                                Fr = "Documentation de la distribution CSV",
                                It = "Documentazione della distribuzione CSV",
                                En = "CSV distribution documentation"
                            }
                        }
                    ],
                    Description = new MultiLanguage
                    {
                        De = "CSV-Distribution des Beispieldatensatzes.",
                        Fr = "Distribution CSV du jeu de données d’exemple.",
                        It = "Distribuzione CSV del set di dati di esempio.",
                        En = "CSV distribution of the example dataset."
                    },
                    Rights = "PUBLIC",
                    Title = new MultiLanguage
                    {
                        De = "Beispieldatensatz als CSV",
                        Fr = "Jeu de données d’exemple en CSV",
                        It = "Set di dati di esempio in CSV",
                        En = "Example dataset as CSV"
                    }
                }
            ]
        };

        return [exampleDataset];
    }
}