using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Models.Search;
using System;

namespace Bfs.Iop.Admin.Business.UnitTests;

internal static class TestData
{
    internal static class Model
    {
        internal static VocabularyEntry AccessRights => VocabularyEntry;

        internal static AccessUrl AccessUrl
        {
            get
            {
                return new AccessUrl
                {
                    Href = "someHref",
                    IsDownload = true,
                    Label = new MultiLanguage
                    {
                        De = "someLabelDe",
                        En = "someLabelEn",
                        Fr = "someLabelFr",
                        It = "someLabelIt"
                    }
                };
            }
        }

        internal static Agent Agent
        {
            get
            {
                return new Agent
                {
                    Id = Guid.NewGuid(),
                    Name = new MultiLanguage
                    {
                        De = "someNameDe",
                        En = "someNameEn",
                        Fr = "someNameFr",
                        It = "someNameIt"
                    }
                };
            }
        }

        internal static DatasetQualityAnswerOption AnswerOption
        {
            get
            {
                return new DatasetQualityAnswerOption
                {
                    Detail = new MultiLanguage
                    {
                        De = "DetailDe",
                        Fr = "DetailFr",
                        It = "DetailIt",
                        En = "DetailEn"
                    },
                    Name = new MultiLanguage
                    {
                        De = "AnswerDe",
                        Fr = "AnswerFr",
                        It = "AnswerIt",
                        En = "AnswerEn"
                    },
                    DetailMandatory = true,
                    Id = new Guid("74fae553-2f6a-4d8f-9736-22ec69ceee27"),
                    Value = "OK"
                };
            }
        }

        internal static Resource ConformTos => Resource;

        internal static Vcard ContactPoint => Vcard;

        internal static DataServiceInput DataServiceInput
        {
            get
            {
                return new DataServiceInput
                {
                    AccessRightCode = "someAccessRightCode",
                    ConformTos = new[] { ConformTos },
                    ContactPoints = new[] { ContactPoint },
                    Description = new MultiLanguage
                    {
                        De = "someDescriptionDe",
                        En = "someDescriptionEn",
                        Fr = "someDescriptionFr",
                        It = "someDescriptionIt"
                    },
                    Documents = new[] { Documentation },
                    EndpointDescriptions = new[] { Resource },
                    EndpointUrls = new[] { Resource },
                    Id = Guid.NewGuid(),
                    Keywords = new[] { Keyword },
                    LandingPages = new[] { LandingPage },
                    Publisher = new IdentifierInputModel { Identifier = Agent.Identifier! },
                    Title = Title,
                    License = new VocabularyEntry { Code = "terms_open", Name = new MultiLanguage { En = "Opendata OPEN: Open use." } },
                };
            }
        }

        internal static MultiLanguage Description
        {
            get
            {
                return new MultiLanguage
                {
                    De = "someDescriptionDe",
                    En = "someDescriptionEn",
                    Fr = "someDescriptionFr",
                    It = "someDescriptionIt"
                };
            }
        }

        internal static Resource Documentation => Resource;

        internal static VocabularyEntry Format => VocabularyEntry;

        internal static KeywordModel Keyword => new()
        {
            Label = new MultiLanguageModel()
            {
                De = "someKeywordDe",
                En = "someKeywordEn",
                Fr = "someKeywordFr",
                It = "someKeywordIt"
            },
            Uri = "https://example.ch"
        };

        internal static Resource LandingPage => Resource;

        internal static MultiLanguageModel MultiLanguageModel => new()
        {
            De = "somethingDe",
            En = "somethingEn",
            Fr = "somethingFr",
            It = "somethingIt",
            Rm = "somethingRm",
        };

        internal static Models.Person Person
        {
            get
            {
                return new Models.Person
                {
                    Id = Guid.NewGuid(),
                    Identifier = "someIdentifier",
                    Name = "someFirstName someLastName",
                    FirstName = "someFirstName",
                    LastName = "someLastName"
                };
            }
        }

        internal static PublicServiceInput PublicServiceInput
        {
            get
            {
                return new PublicServiceInput
                {
                    BusinessEventsCodes = new[] { "toto", "tata" },
                    CompetentAuthority = new IdentifierInputModel { Identifier = Agent.Identifier! },
                    Description = Description,
                    Id = Guid.NewGuid(),
                    Keywords = new[] { Keyword },
                    LanguageCodes = new[] { "en", "de", "fr", "it" },
                    LifeEventsCodes = new[] { "tete", "titi" },
                    SectorCodes = new[] { "sector" },
                    Spatial = new[] { "someSpatial1", "someSpatial2" },
                    ThematicAreaCodes = new[] { "thematicArea" },
                    Title = Title
                };
            }
        }

        internal static DatasetQualityInformation QualityInformation
        {
            get
            {
                return new DatasetQualityInformation
                {
                    AnswerId = AnswerOption.Id,
                    QuestionId = Question.Id,
                    Detail = "Test",
                    Id = new Guid("74fae553-2f6a-4d8f-9736-22ec69ceee27"),
                };
            }
        }

        internal static DatasetQualityInformationData QualityInformationData
        {
            get
            {
                return new DatasetQualityInformationData
                {
                    QualityInformations = new DatasetQualityInformation[]
                    {
                        QualityInformation
                    },
                    Documentation = new DatasetQualityInformationLink[]
                    {
                        QualityInformationLink
                    }
                };
            }
        }

        internal static DatasetQualityInformationLink QualityInformationLink
        {
            get
            {
                return new DatasetQualityInformationLink
                {
                    Href = "link",
                    Label = new MultiLanguage
                    {
                        De = "LabelDe",
                        Fr = "LabelFr",
                        It = "LabelIt",
                        En = "LabelEn"
                    },
                    Id = new Guid("74fae553-2f6a-4d8f-9736-22ec69ceee27"),
                };
            }
        }

        internal static DatasetQualityQuestion Question
        {
            get
            {
                return new DatasetQualityQuestion
                {
                    AnswerOptions = new DatasetQualityAnswerOption[1] { AnswerOption },
                    Mandatory = true,
                    Order = 1,
                    Question = new MultiLanguage
                    {
                        De = "QuestionDe",
                        Fr = "QuestionFr",
                        It = "QuestionIt",
                        En = "QuestionEn"
                    },
                    Id = new Guid("74fae553-2f6a-4d8f-9736-22ec69ceee27"),
                };
            }
        }


        internal static PeriodOfTime TemporalCoverage => PeriodOfTime;

        internal static MultiLanguage Title
        {
            get
            {
                return new MultiLanguage
                {
                    De = "someTitleDe",
                    En = "someTitleEn",
                    Fr = "someTitleFr",
                    It = "someTitleIt"
                };
            }
        }

        private static PeriodOfTime PeriodOfTime
        {
            get
            {
                return new PeriodOfTime
                {
                    Start = DateTimeOffset.UtcNow.AddDays(-1),
                    End = DateTimeOffset.UtcNow
                };
            }
        }

        private static Resource Resource
        {
            get
            {
                return new Resource
                {
                    Href = "someHref",
                    Label = new MultiLanguage
                    {
                        De = "someLabelDe",
                        En = "someLabelEn",
                        Fr = "someLabelFr",
                        It = "someLabelIt"
                    }
                };
            }
        }

        private static Vcard Vcard
        {
            get
            {
                return new Vcard
                {
                    AdrWork = new MultiLanguage
                    {
                        De = "AdrWorkDe",
                        En = "AdrWorkEn",
                        Fr = "AdrWorkFr",
                        It = "AdrWorkIt"
                    },
                    EmailInternet = "someEmailInternet",
                    Fn = new MultiLanguage
                    {
                        De = "someFnDe",
                        En = "someFnEn",
                        Fr = "someFnFr",
                        It = "someFnIt"
                    },
                    Note = new MultiLanguage
                    {
                        De = "someNoteDe",
                        En = "someNoteEn",
                        Fr = "someNoteFr",
                        It = "someNoteIt"
                    },
                    Org = new MultiLanguage
                    {
                        De = "someOrgDe",
                        En = "someOrgEn",
                        Fr = "someOrgFr",
                        It = "someOrgIt"
                    },
                    TelWorkVoice = "someTelWorkVoice"
                };
            }
        }

        private static VocabularyEntry VocabularyEntry
        {
            get
            {
                return new VocabularyEntry
                {
                    Code = "someCode",
                    Name = new MultiLanguage
                    {
                        De = "someNameDe",
                        En = "someNameEn",
                        Fr = "someNameFr",
                        It = "someNameIt"
                    }
                };
            }
        }
    }

    internal static class Core
    {
        public static AgentModel AgentModel => new()
        {
            Classification = VocabularyEntryModel,
            ContactPoint = VCardModel,
            Description = MultiLanguageModel,
            HomePage = "yes",
            Id = Guid.NewGuid(),
            Identifier = "agent",
            Name = MultiLanguageModel,
            PrefLabel = MultiLanguageModel,
            Spatial = ["spatial"],
            SpatialCH = [VocabularyEntryModel],
            SubAgents = [IdNameModel],
            System = SystemInfoModel,
            Uid = "uid"
        };

        internal static SearchResultModel CatalogSearchResultItem => new()
        {
            Id = Guid.NewGuid(),
            AccessRights = VocabularyEntryModel,
            Description = MultiLanguageModel,
            Identifier = "someIdentifier",
            PublicationLevel = PublicationLevel.Public,
            PublicationLevelProposal = null,
            Publisher = AgentModel,
            RegistrationStatusProposal = null,
            RegistrationStatus = RegistrationStatus.Recorded,
            System = new SystemInfoModel()
            {
                 CreatedAt = DateTime.Now,
                 CreationType = CreationType.Automated,
                 ModifiedAt = DateTime.MaxValue
            },
            Type = SearchResourceType.Dataset,
            Title = MultiLanguageModel
        };

        internal static SearchCountResultModel CatalogSearchCountResult => new()
        {
            PublicationLevels =
                    [
                        new SearchCountResultItem<PublicationLevel>{ Count = 4, Value = PublicationLevel.Internal },
                        new SearchCountResultItem<PublicationLevel>{ Count = 7, Value = PublicationLevel.Public },
                    ],
            Publishers =
                    [
                        new SearchCountResultItem<AgentModel>{ Count = 11, Value = AgentModel },
                    ],
            RegistrationStatuses =
                    [
                        new SearchCountResultItem<RegistrationStatus>{ Count = 1, Value = RegistrationStatus.Incomplete },
                        new SearchCountResultItem<RegistrationStatus>{ Count = 2, Value = RegistrationStatus.Candidate },
                        new SearchCountResultItem<RegistrationStatus>{ Count = 8, Value = RegistrationStatus.Qualified },
                    ],
            Types =
                    [
                        new SearchCountResultItem<string>{ Count = 9, Value = SearchResourceType.Dataset.ToString() },
                        new SearchCountResultItem<string>{ Count = 2, Value = SearchResourceType.DataService.ToString() },
                    ],
        };

        internal static ChannelModel ChannelModel =>
            new()
            {
                Id = Guid.NewGuid(),
                Identifier = "channel_identifier",
                Address = MultiLanguageModel,
                Description = MultiLanguageModel,
                Email = "email@email.ch",
                Fax = "some_fax",
                Mobile = "+41123456789",
                OpeningHours = "24h open",
                Phone = "+41987654321",
                Type = VocabularyEntryModel,
            };

        internal static DataServiceModel DataServiceModel
        {
            get
            {
                return new DataServiceModel
                {
                    AccessRights = VocabularyEntryModel,
                    ConformsTo = [ResourceModel],
                    ContactPoints = [VCardModel],
                    Description = new MultiLanguageModel { De = "DescriptionDE", En = "DescriptionEN", Fr = "DescriptionFR", It = "DescriptionIT", Rm = "DescriptionRM" },
                    Documentation = [ResourceModel],
                    EndpointDescriptions = [],
                    EndpointUrls = [],
                    Id = Guid.NewGuid(),
                    Keywords = [KeywordModel],
                    LandingPages = [ResourceModel],
                    Themes = [VocabularyEntryModel],
                    Title = new MultiLanguageModel { De = "TitleDE", En = "TitleEN", Fr = "TitleFR", It = "TitleIT", Rm = "TitleRM" },
                    Publisher = AgentModel,
                    Version = "1.0",
                    License = new VocabularyEntryModel { Code = "terms_open", Name = new MultiLanguageModel { En = "Opendata OPEN: Open use." } },
                    System = SystemInfoModel,
                    VersionNotes = MultiLanguageModel
                };
            }
        }

        public static DatasetQualityAnswerOptionModel AnswerOptionModel
        {
            get
            {
                return new DatasetQualityAnswerOptionModel
                {
                    Detail = new MultiLanguageModel
                    {
                        De = "DetailDe",
                        Fr = "DetailFr",
                        It = "DetailIt",
                        En = "DetailEn"
                    },
                    Name = new MultiLanguageModel
                    {
                        De = "AnswerDe",
                        Fr = "AnswerFr",
                        It = "AnswerIt",
                        En = "AnswerEn"
                    },
                    IsDetailMandatory = true,
                    Id = new Guid("74fae553-2f6a-4d8f-9736-22ec69ceee27"),
                    Value = "OK"
                };
            }
        }

        public static DatasetQualityInformationModel QualityInformationModel
        {
            get
            {
                return new DatasetQualityInformationModel
                {
                    AnswerId = AnswerOptionModel.Id,
                    QuestionId = QuestionModel.Id,
                    Detail = "Test",
                    Id = new Guid("74fae553-2f6a-4d8f-9736-22ec69ceee27"),
                };
            }
        }

        public static DatasetQualityInformationDataModel QualityInformationDataModel
        {
            get
            {
                return new DatasetQualityInformationDataModel
                {
                    QualityInformations =
                    [
                        QualityInformationModel
                    ],
                    Documentation =
                    [
                        QualityInformationLinkModel
                    ]
                };
            }
        }

        public static DatasetQualityInformationLinkModel QualityInformationLinkModel
        {
            get
            {
                return new DatasetQualityInformationLinkModel
                {
                    Href = "link",
                    Label = new MultiLanguageModel
                    {
                        De = "LabelDe",
                        Fr = "LabelFr",
                        It = "LabelIt",
                        En = "LabelEn"
                    },
                    Id = new Guid("74fae553-2f6a-4d8f-9736-22ec69ceee27"),
                };
            }
        }

        public static DatasetQualityQuestionModel QuestionModel
        {
            get
            {
                return new DatasetQualityQuestionModel
                {
                    AnswerOptions = new DatasetQualityAnswerOptionModel[1] { AnswerOptionModel },
                    IsMandatory = true,
                    Order = 1,
                    Question = new MultiLanguageModel
                    {
                        De = "QuestionDe",
                        Fr = "QuestionFr",
                        It = "QuestionIt",
                        En = "QuestionEn"
                    },
                    Id = new Guid("74fae553-2f6a-4d8f-9736-22ec69ceee27"),
                };
            }
        }

        public static DcatDatasetModel DcatDatasetModel => new()
        {
            AccessRights = VocabularyEntryModel,
            ConfidentialityPerson = VocabularyEntryModel,
            ConformsTo = [ResourceModel],
            ContactPoints = [VCardModel],
            DataOwner = "owner",
            Description = MultiLanguageModel,
            Distributions = [],
            Documentation = [ResourceModel],
            Frequency = VocabularyEntryModel,
            GeoIvIds = [VocabularyEntryModel],
            Images = [ResourceModel],
            IsReferencedBy = [ResourceModel],
            Issued = DateTimeOffset.Now,
            Keywords = [KeywordModel],
            LandingPages = [ResourceModel],
            Languages = [VocabularyEntryModel],
            Modified = DateTimeOffset.Now,
            PreviousVersion = IdModel,
            ProcessId = "toto",
            PublicationLevelProposal = PublicationLevel.Public,
            Publisher = AgentModel,
            QualifiedAttributionComplement = MultiLanguageModel,
            QualifiedAttributions = [DcatQualifiedAttributionModel],
            QualifiedRelations = [DcatQualifiedRelationModel],
            RegistrationStatusProposal = RegistrationStatus.Standard,
            Relations = [ResourceModel],
            ResponsibleDeputy = IopPersonModel,
            ResponsiblePerson = IopPersonModel,
            RetentionPeriod = DateTime.Now,
            RetentionPeriodComplement = MultiLanguageModel,
            Spatial = ["CH"],
            TemporalCoverage = [PeriodOfTimeModel],
            Themes = [VocabularyEntryModel],
            VersionNotes = MultiLanguageModel,
            Id = Guid.NewGuid(),
            Identifiers = new[] { "toto", "tata" },
            PublicationLevel = PublicationLevel.Public,
            RegistrationStatus = RegistrationStatus.Recorded,
            Title = MultiLanguageModel,
            System = SystemInfoModel,
            Version = "1.0.0"
        };

        public static DcatQualifiedAttributionModel DcatQualifiedAttributionModel => new()
        {
            Agent = AgentModel,
            HadRole = VocabularyEntryModel
        };

        public static DcatQualifiedRelationModel DcatQualifiedRelationModel => new()
        {
            HadRole = VocabularyEntryModel,
            Relation = ResourceModel
        };

        internal static DcatDistributionModel Distribution
        {
            get
            {
                return new DcatDistributionModel
                {
                    AccessUrl = ResourceModel,                   
                    Availability = VocabularyEntryModel,
                    ByteSize = 123456789m,
                    Checksum = new ChecksumModel { Algorithm = new VocabularyEntryModel { Code = "checksumAlgorithm_md5", Name = new MultiLanguageModel { De = "MD5" } }, ChecksumValue = "SomeValue" },
                    ConformsTo = [ResourceModel],
                    Coverage = [],
                    Description = new MultiLanguageModel
                    {
                        De = "someDescriptionDe",
                        En = "someDescriptionEn",
                        Fr = "someDescriptionFr",
                        It = "someDescriptionIt"
                    },
                    Documentation = [ResourceModel],
                    DownloadUrl = ResourceModel,
                    Format = VocabularyEntryModel,
                    Id = Guid.NewGuid(),
                    Languages = [VocabularyEntryModel],
                    MediaType = new VocabularyEntryModel
                    {
                        Code = "text/csv",
                        Name = new MultiLanguageModel
                        {
                            De = "csv",
                        },
                    },
                    Modified = DateTimeOffset.UtcNow,
                    Issued = DateTimeOffset.UtcNow.AddDays(-1),
                    Rights = "Public",
                    SpatialResolution = 2,
                    Title = new MultiLanguageModel
                    {
                        De = "someDistributionTitleDe",
                        En = "someDistributionTitleEn",
                        Fr = "someDistributionTitleFr",
                        It = "someDistributionTitleIt"
                    },
                    License = new VocabularyEntryModel { Code = "terms_open", Name = new MultiLanguageModel { En = "Opendata OPEN: Open use." } },
                    Identifier = "myIdentifier",
                    PackagingFormat = new VocabularyEntryModel { Code = "terms_open", Name = new MultiLanguageModel { En = "Opendata OPEN: Open use." } },
                    TemporalResolution = "myResolution",
                };
            }
        }

        public static IdModel IdModel => new()
        {
            Id = Guid.NewGuid(),
        };

        public static IdNameModel IdNameModel => new()
        {
            Id = Guid.NewGuid(),
            Name = MultiLanguageModel
        };

        public static IopPersonModel IopPersonModel => new()
        {
            Email = "email@email.com",
            FamilyName = "Tata",
            GivenName = "Toto"
        };

        internal static KeywordModel KeywordModel => new()
        {
            Label = new MultiLanguageModel()
            {
                De = "someKeywordDe",
                En = "someKeywordEn",
                Fr = "someKeywordFr",
                It = "someKeywordIt"
            },
            Uri = "https://example.ch"
        };

        public static MultiLanguageModel MultiLanguageModel => new()
        { 
            De = "De", 
            En = "En",
            Fr = "Fr", 
            It = "It",
            Rm = "Rm " 
        };

        public static PeriodOfTimeModel PeriodOfTimeModel => new()
        {
            Start = DateTimeOffset.Now.AddDays(-2),
            End = DateTimeOffset.Now
        };

        public static PublicServiceModel PublicServiceModel => new()
        {
            BusinessEvents = [VocabularyEntryModel],
            Publisher = AgentModel,
            Description = MultiLanguageModel,
            Id = Guid.NewGuid(),
            Identifiers = ["publicService_Identifier"],
            Keywords = [KeywordModel],
            Languages = [VocabularyEntryModel],
            LifeEvents = [VocabularyEntryModel],
            Sectors = [VocabularyEntryModel],
            Spatial = ["someSpatial1", "someSpatial2"],
            ThematicAreas = [VocabularyEntryModel],
            Name = MultiLanguageModel,
            System = SystemInfoModel
        };

        public static ResourceModel ResourceModel => new()
        {
            Label = MultiLanguageModel,
            Uri = "uri"
        };

        public static SystemInfoModel SystemInfoModel => new()
        {
            CreatedAt = DateTimeOffset.Now,
            CreationType = CreationType.Automated
        };

        public static VCardModel VCardModel => new()
        {
            Fn = MultiLanguageModel,
            HasAddress = MultiLanguageModel,
            HasEmail = "yes",
            HasTelephone = "no",
            Kind = VCardKind.Organization,
            Note = MultiLanguageModel,
        };

        public static VocabularyEntryModel VocabularyEntryModel => new()
        {
            Code = "code", 
            Name = MultiLanguageModel,
            Uri = "uri"
        };
    }
}