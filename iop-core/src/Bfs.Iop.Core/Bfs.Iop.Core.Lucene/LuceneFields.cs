using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Core.Lucene;

public static class LuceneFields
{
    internal static string RawField(string fieldName) => $"{fieldName}_raw";
    internal static string NgramField(string fieldName) => $"{fieldName}_ngram";
    internal static string OriginalField(string fieldName) => $"{fieldName}_original";

    internal static class CodeListEntry
    {
        public const string Id = "codelistentry_id";
        public const string ConceptId = "codelistentry_concept_id";
        public const string Code = "codelistentry_code";
        public const string ParentCode = "codelistentry_parent_code";

        public static string Name(string lang) => $"codelistentry_name_{lang}";
        public static string Description(string lang) => $"codelistentry_description_{lang}";

        public static List<string> Fields = 
            [
            Id,
            ConceptId,
            Code,
            ParentCode,
            Name(MultiLanguageModel.GermanKey),
            Name(MultiLanguageModel.EnglishKey),
            Name(MultiLanguageModel.FrenchKey),
            Name(MultiLanguageModel.ItalianKey),
            Name(MultiLanguageModel.RomanshKey),
            Description(MultiLanguageModel.GermanKey),
            Description(MultiLanguageModel.EnglishKey),
            Description(MultiLanguageModel.FrenchKey),
            Description(MultiLanguageModel.ItalianKey),
            Description(MultiLanguageModel.RomanshKey),
            ];
    }

    internal static class CodeListEntryAnnotation
    {
        public const string AnnotationCodeListEntryId = "annotation_codelistentry_id"; 
        public const string AnnotationType = "annotation_type";
        public const string AnnotationIdentifier = "annotation_identifier";
        public const string AnnotationTitle = "annotation_title";
        public const string AnnotationUri = "annotation_uri";

        public static List<string> Fields =
            [
            AnnotationType,
            AnnotationIdentifier,
            AnnotationTitle,
            AnnotationUri,
            AnnotationText(MultiLanguageModel.GermanKey),
            AnnotationText(MultiLanguageModel.EnglishKey),
            AnnotationText(MultiLanguageModel.FrenchKey),
            AnnotationText(MultiLanguageModel.ItalianKey),
            AnnotationText(MultiLanguageModel.RomanshKey),
            ];

        public static string AnnotationText(string lang) => $"annotation_text_{lang}";
    }

    public static class  Catalog
    {
        public const string AccessRights = "AccessRights";
        public const string BusinessEvents = "BusinessEvents";
        public const string ConceptType = "ConceptType";
        public const string CreatedAt = "CreatedAt";
        public const string CreationType = "CreationType";
        public const string DataOwner = "DataOwner";
        public const string Description = "Description";
        public const string Distribution = "Distribution";
        public const string Id = "Id";
        public const string Email = "Email";
        public const string FamilyName = "FamilyName";
        public const string Formats = "Formats";
        public const string GivenName = "GivenName";
        public const string HasStructure = "HasStructure";
        public const string Identifier = "Identifier";
        public const string Keyword = "Keyword";
        public const string LifeEvents = "LifeEvents";
        public const string ModifiedAt = "ModifiedAt";
        public const string Name = "Name";
        public const string PublicationLevel = "PublicationLevel";
        public const string PublicationLevelProposal = "PublicationLevelProposal";
        public const string Publisher = "Publisher";
        public const string RegistrationStatus = "RegistrationStatus";
        public const string RegistrationStatusProposal = "RegistrationStatusProposal";
        public const string ResponsibleDeputy = "ResponsibleDeputy";
        public const string ResponsiblePerson = "ResponsiblePerson";
        public const string Themes = "Themes";
        public const string Title = "Title";
        public const string Type = "Type";
        public const string ValidFrom = "ValidFrom";
        public const string ValidTo = "ValidTo";
        public const string Version = "Version";

        public const string DataServiceIdentifier = Types.DataService + Identifier;
        public const string DistributionDescription = Distribution + Description;
        public const string DistributionIdentifier = Distribution + Identifier;
        public const string DistributionTitle = Distribution + Title;
        public const string PublisherIdentifier = Publisher + Identifier;
        public const string ResponsibleDeputyEmail = ResponsibleDeputy + Email;
        public const string ResponsibleDeputyFamilyName = ResponsibleDeputy + FamilyName;
        public const string ResponsibleDeputyGivenName = ResponsibleDeputy + GivenName;
        public const string ResponsiblePersonEmail = ResponsiblePerson + Email;
        public const string ResponsiblePersonFamilyName = ResponsiblePerson + FamilyName;
        public const string ResponsiblePersonGivenName = ResponsiblePerson + GivenName;

        public const string ContactPoint = "ContactPoint";
        public const string ContactPointFn = ContactPoint + "Fn";
        public const string ContactPointHasAddress = ContactPoint + "HasAddress";
        public const string ContactPointHasEmail = ContactPoint + "HasEmail";
        public const string ContactPointHasTelephone = ContactPoint + "HasTelephone";
        public const string ContactPointNote = ContactPoint + "Note";

        public const string RegistrationStatusWeight = "registrationStatusWeight";

        internal static class Types
        {
            public const string Dataset = "Dataset";
            public const string DataService = "DataService";
            public const string IopConcept = "Concept";
            public const string MappingTable = "MappingTable";
            public const string PublicService = "PublicService";
        }
    }
}
