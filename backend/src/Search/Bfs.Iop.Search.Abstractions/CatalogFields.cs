namespace Bfs.Iop.Search.Abstractions;

/// <summary>
/// Field names of the catalog index document owned by the IndexSearch service.
/// The query builder and the facet-count mapping both key on these strings, so changing a
/// value changes the index document format.
/// </summary>
public static class CatalogFields
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

    public static class Types
    {
        public const string Dataset = "Dataset";
        public const string DataService = "DataService";
        public const string IopConcept = "Concept";
        public const string MappingTable = "MappingTable";
        public const string PublicService = "PublicService";
    }
}
