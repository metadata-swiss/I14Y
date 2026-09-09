using AwesomeAssertions;
using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.IndexSearch.Data.UnitTests;

// The mapping turns five different aggregate models into one document shape. A field it forgets and a
// field it fills for the wrong resource type are both invisible from outside — the resource simply
// stops being findable by that field.
[TestFixture]
internal sealed class ModelToDocumentMappingTests
{
    private static MultiLanguageModel Text(string value) => new() { De = value };

    private static SystemInfoModel System() =>
        new() { CreatedAt = DateTimeOffset.UnixEpoch, CreationType = CreationType.Automated };

    private static AgentModel Agent() => new()
    {
        Id = Guid.NewGuid(),
        Identifier = "CH-BFS",
        Name = Text("Bundesamt"),
        PrefLabel = Text("BFS"),
        System = System(),
    };

    private static IopPersonModel Person() =>
        new() { GivenName = "Ada", FamilyName = "Lovelace", Email = "ada@bfs.admin.ch" };

    [Test]
    public void A_dataset_carries_its_formats_access_rights_and_contact_points()
    {
        var model = new DcatDatasetModel
        {
            Id = Guid.NewGuid(),
            Identifiers = ["ds-1", "ds-2"],
            Title = Text("Bevoelkerung"),
            Description = Text("Beschreibung"),
            Version = "1.0",
            DataOwner = "BFS",
            AccessRights = new VocabularyEntryModel { Code = "PUBLIC" },
            Themes = [new VocabularyEntryModel { Code = "SOCI" }],
            Keywords = [new KeywordModel { Label = Text("Statistik") }],
            Distributions =
            [
                new DcatDistributionModel { Format = new VocabularyEntryModel { Code = "CSV" } },
                new DcatDistributionModel { Format = new VocabularyEntryModel { Code = "CSV" } },
            ],
            ContactPoints = [new VCardModel { HasEmail = "kontakt@bfs.admin.ch", Fn = Text("Auskunft") }],
            ResponsiblePerson = Person(),
            Publisher = Agent(),
            System = System(),
        };

        var document = model.ToIndexDocument();

        document.Type.Should().Be(SearchResourceType.Dataset);
        document.Identifiers.Should().Equal("ds-1", "ds-2");
        document.AccessRights.Should().Be("PUBLIC");
        document.Themes.Should().Equal("SOCI");
        document.DataOwner.Should().Be("BFS");
        document.Keywords.Should().ContainSingle();

        // Distinct: the facet counts a dataset once per format, not once per file.
        document.Formats.Should().Equal("CSV");

        document.ContactPoints.Should().ContainSingle();
        document.ResponsiblePerson!.Email.Should().Be("ada@bfs.admin.ch");

        // Never false — null means "leave whatever is indexed alone".
        document.HasStructure.Should().BeNull();
    }

    // VCardModel.HasEmail is required, and the relational mapper turns a missing address into "".
    // Indexed as-is that becomes a searchable empty term on every contact point without an e-mail.
    [Test]
    public void A_contact_point_without_an_email_maps_to_null_not_empty()
    {
        var model = new DcatDatasetModel
        {
            Id = Guid.NewGuid(),
            Title = Text("t"),
            Description = Text("d"),
            AccessRights = new VocabularyEntryModel { Code = "PUBLIC" },
            ContactPoints = [new VCardModel { HasEmail = string.Empty }],
            Publisher = Agent(),
            System = System(),
        };

        model.ToIndexDocument().ContactPoints.Single().HasEmail.Should().BeNull();
    }

    [Test]
    public void A_public_service_folds_thematic_areas_and_sectors_into_themes()
    {
        var model = new PublicServiceModel
        {
            Id = Guid.NewGuid(),
            Name = Text("Dienst"),
            Description = Text("Beschreibung"),
            ThematicAreas = [new VocabularyEntryModel { Code = "HEALTH" }],
            Sectors = [new VocabularyEntryModel { Code = "PUBLIC" }],
            BusinessEvents = [new VocabularyEntryModel { Code = "BE" }],
            LifeEvents = [new VocabularyEntryModel { Code = "LE" }],
            Channels = [new ChannelModel { Id = Guid.NewGuid(), Identifier = "c1", Email = " amt@bfs.admin.ch " }],
            Publisher = Agent(),
            System = System(),
        };

        var document = model.ToIndexDocument();

        document.Type.Should().Be(SearchResourceType.PublicService);
        document.Themes.Should().BeEquivalentTo(["HEALTH", "PUBLIC"]);
        document.BusinessEvents.Should().Equal("BE");
        document.LifeEvents.Should().Equal("LE");

        // Trimmed, because the channel e-mail is indexed as a keyword and matched exactly.
        document.ChannelEmails.Should().Equal("amt@bfs.admin.ch");

        // A public service has no title of its own; the name fills both so free text reaches it.
        document.Name.Should().NotBeNull();
        document.Title.Should().NotBeNull();
    }

    [Test]
    public void A_concept_fills_both_name_and_title_and_carries_its_type()
    {
        var model = new IopConceptModel
        {
            Id = Guid.NewGuid(),
            Name = Text("Konzept"),
            Description = Text("Beschreibung"),
            Version = "2",
            ConceptType = ConceptType.CodeList,
            ValidFrom = DateTimeOffset.UnixEpoch,
            ResponsiblePerson = Person(),
            Publisher = Agent(),
            System = System(),
        };

        var document = model.ToIndexDocument();

        document.Type.Should().Be(SearchResourceType.Concept);
        document.ConceptType.Should().Be(ConceptType.CodeList);
        document.Name.Should().NotBeNull();
        document.Title.Should().NotBeNull();
        document.ValidFrom.Should().Be(DateTimeOffset.UnixEpoch);
    }

    [Test]
    public void Publishable_fields_come_from_the_publisher_and_the_system_block()
    {
        var publisher = Agent();

        var model = new DcatDatasetModel
        {
            Id = Guid.NewGuid(),
            Title = Text("t"),
            Description = Text("d"),
            AccessRights = new VocabularyEntryModel { Code = "PUBLIC" },
            PublicationLevel = PublicationLevel.Public,
            RegistrationStatus = RegistrationStatus.Standard,
            Publisher = publisher,
            System = System(),
        };

        var document = model.ToIndexDocument();

        document.PublisherId.Should().Be(publisher.Id);
        document.PublisherIdentifier.Should().Be("CH-BFS");
        document.PublicationLevel.Should().Be(PublicationLevel.Public);
        document.RegistrationStatus.Should().Be(RegistrationStatus.Standard);
        document.CreatedAt.Should().Be(DateTimeOffset.UnixEpoch);
        document.CreationType.Should().Be(CreationType.Automated);
    }

    [Test]
    public void An_empty_multi_language_value_becomes_null_rather_than_an_empty_object()
    {
        var model = new DcatDatasetModel
        {
            Id = Guid.NewGuid(),
            Title = new MultiLanguageModel { De = "   " },
            Description = Text("d"),
            AccessRights = new VocabularyEntryModel { Code = "PUBLIC" },
            Publisher = Agent(),
            System = System(),
        };

        model.ToIndexDocument().Title.Should().BeNull();
    }

    [Test]
    public void A_code_list_entry_carries_its_ancestors_and_annotations()
    {
        var model = new CodeListEntryModel
        {
            Id = Guid.NewGuid(),
            ConceptId = Guid.NewGuid(),
            Code = "11",
            ParentCode = "1",
            Name = Text("Eintrag"),
            Annotations =
            [
                new AnnotationModel { CodeListEntryId = Guid.NewGuid(), Type = "note", Title = "T" },
            ],
        };

        var document = model.ToIndexDocument(["1", "0"]);

        document.Code.Should().Be("11");
        document.ParentCode.Should().Be("1");
        document.AncestorCodes.Should().Equal("1", "0");
        document.Annotations.Should().ContainSingle().Which.Type.Should().Be("note");
    }

    [Test]
    public void A_code_list_entry_without_annotations_maps_to_an_empty_list()
    {
        var model = new CodeListEntryModel
        {
            Id = Guid.NewGuid(),
            ConceptId = Guid.NewGuid(),
            Code = "1",
            Name = Text("Eintrag"),
            Annotations = null,
        };

        model.ToIndexDocument([]).Annotations.Should().BeEmpty();
    }
}