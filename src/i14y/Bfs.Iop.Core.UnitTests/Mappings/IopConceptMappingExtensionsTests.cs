using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Entities;
using Bfs.Iop.Core.Mappings;
using Bfs.Iop.Core.UnitTests.Helpers;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using NSubstitute;
using Bfs.Iop.Core.Services.Contracts;

namespace Bfs.Iop.Core.UnitTests.Mappings;

[TestFixture(TestOf = typeof(IopConceptMappingExtensions))]
internal sealed class IopConceptMappingExtensionsTests
{
    [Test]
    public void Given_IopConcept_When_MapToIopConceptModel_Then_Mapping_Ok()
    {
        // Arrange
        var subject = CreateNonsenseConcept();

        var vocabulariesService = Substitute.For<IVocabulariesService>();

        // Act
        var result = subject.MapToIopConceptModel(vocabulariesService);

        // Assert
        using var _ = new AssertionScope();
        result.CodeListEntries.Should().HaveCount(subject.CodeListEntries!.Count);
        result.CodeListEntries!.First().Id.Should().Be(subject.CodeListEntries!.First().Id);
        result.CodeListEntryValueMaxLength.Should().Be(subject.CodeListEntryValueMaxLength);
        result.CodeListEntryValueType.Should().Be(subject.CodeListEntryValueType);
        result.ConceptType.Should().Be(subject.ConceptType);
        result.ConformsTo.Should().HaveCount(subject.ConformsTo.Count);
        result.ConformsTo.First().Uri.Should().Be(subject.ConformsTo.First().Href);
        result.Description.En.Should().Be(subject.Description.En);
        result.Id.Should().Be(subject.Id);
        result.Identifiers.Count().Should().Be(subject.Identifiers.Count());
        result.Identifiers.First().Should().Be(subject.Identifiers.First());
        result.IsLocked.Should().Be(subject.IsLocked);
        result.Keywords.Should().HaveCount(subject.Keywords.Count);
        result.Keywords.First().Label!.De.Should().Be(subject.Keywords.First().Text.De);
        result.MaxLength.Should().Be(subject.MaxLength);
        result.MaxValue.Should().Be(subject.MaxValue);
        result.MinLength.Should().Be(subject.MinLength);
        result.MinValue.Should().Be(subject.MinValue);
        result.Name.En.Should().Be(subject.Name.En);
        result.NumberDecimals.Should().Be(subject.NumberDecimals);
        result.Pattern.Should().Be(subject.Pattern);
        result.Replaces.Should().HaveCount(subject.Replaces.Count);
        result.Replaces.First().Uri.Should().Be(subject.Replaces.First().Href);
        result.IsReplacedBy.Should().BeEmpty();
        result.Publisher.Id.Should().Be(subject.Publisher.Id);
        result.PublicationLevel.Should().Be(subject.PublicationLevel);
        result.PublicationLevelProposal.Should().Be(subject.PublicationLevelProposal);
        result.RegistrationStatus.Should().Be(subject.RegistrationStatus);
        result.RegistrationStatusProposal.Should().Be(subject.RegistrationStatusProposal);
        result.ResponsibleDeputy?.Email.Should().Be(subject.ResponsibleDeputy?.Email);
        result.ResponsiblePerson?.Email.Should().Be(subject.ResponsiblePerson?.Email);
        result.Themes.Should().HaveCount(subject.Themes.Count);
        result.Themes.First().Code.Should().Be(subject.Themes.First());
        result.ValidFrom.Should().Be(subject.ValidFrom);
        result.ValidTo.Should().Be(subject.ValidTo);
        result.Version.Should().Be(subject.Version);
    }

    [Test]
    public void Given_isReplacedBy_When_MapToIopConceptModel_Then_PassedThrough()
    {
        // Arrange
        var subject = CreateNonsenseConcept();
        var vocabulariesService = Substitute.For<IVocabulariesService>();
        var isReplacedBy = new[]
        {
            new ConceptReferenceModel { Uri = "https://register.ld.admin.ch/i14y/concept/successor/version/1.0.0" }
        };

        // Act
        var result = subject.MapToIopConceptModel(vocabulariesService, isReplacedBy: isReplacedBy);

        // Assert
        using var _ = new AssertionScope();
        result.IsReplacedBy.Should().HaveCount(1);
        result.IsReplacedBy.First().Uri.Should().Be(isReplacedBy[0].Uri);
    }

    [Test]
    public void Given_IopConcept_When_MapToIopConceptModel_WithInvalidThemeCode_Then_Mapping_Ok()
    {
        // Arrange
        var subject = CreateNonsenseConcept();

        var vocabulariesService = Substitute.For<IVocabulariesService>();

        // Act
        var result = subject.MapToIopConceptModel(vocabulariesService);

        // Assert
        using var _ = new AssertionScope();
        result.CodeListEntries.Should().HaveCount(subject.CodeListEntries!.Count);
        result.CodeListEntries!.First().Id.Should().Be(subject.CodeListEntries!.First().Id);
        result.CodeListEntryValueMaxLength.Should().Be(subject.CodeListEntryValueMaxLength);
        result.CodeListEntryValueType.Should().Be(subject.CodeListEntryValueType);
        result.ConceptType.Should().Be(subject.ConceptType);
        result.ConformsTo.Should().HaveCount(subject.ConformsTo.Count);
        result.ConformsTo.First().Uri.Should().Be(subject.ConformsTo.First().Href);
        result.Description.En.Should().Be(subject.Description.En);
        result.Id.Should().Be(subject.Id);
        result.Identifiers.Count().Should().Be(subject.Identifiers.Count());
        result.Identifiers.First().Should().Be(subject.Identifiers.First());
        result.IsLocked.Should().Be(subject.IsLocked);
        result.Keywords.Should().HaveCount(subject.Keywords.Count);
        result.Keywords.First().Label!.De.Should().Be(subject.Keywords.First().Text.De);
        result.MaxLength.Should().Be(subject.MaxLength);
        result.MaxValue.Should().Be(subject.MaxValue);
        result.MinLength.Should().Be(subject.MinLength);
        result.MinValue.Should().Be(subject.MinValue);
        result.Name.En.Should().Be(subject.Name.En);
        result.NumberDecimals.Should().Be(subject.NumberDecimals);
        result.Pattern.Should().Be(subject.Pattern);
        result.Publisher.Id.Should().Be(subject.Publisher.Id);
        result.PublicationLevel.Should().Be(subject.PublicationLevel);
        result.PublicationLevelProposal.Should().Be(subject.PublicationLevelProposal);
        result.RegistrationStatus.Should().Be(subject.RegistrationStatus);
        result.RegistrationStatusProposal.Should().Be(subject.RegistrationStatusProposal);
        result.ResponsibleDeputy?.Email.Should().Be(subject.ResponsibleDeputy?.Email);
        result.ResponsiblePerson?.Email.Should().Be(subject.ResponsiblePerson?.Email);
        result.Themes.Should().HaveCount(subject.Themes.Count);
        result.Themes.First().Code.Should().Be(subject.Themes.First());
        result.ValidFrom.Should().Be(subject.ValidFrom);
        result.ValidTo.Should().Be(subject.ValidTo);
        result.Version.Should().Be(subject.Version);
    }

    private static IopConcept CreateNonsenseConcept() =>
        new()
        {
            CodeListEntries = [EntitiesHelper.CodeListEntry],
            CodeListEntryValueMaxLength = 100,
            CodeListEntryValueType = CodeListEntryValueType.Numeric,
            ConceptType = ConceptType.Date,
            ConformsTo = [EntitiesHelper.Resource],
            Description = EntitiesHelper.MultiLanguage,
            Id = Guid.NewGuid(),
            Identifiers = ["Iop_concept_identifier"],
            IsLocked = true,
            Keywords = [EntitiesHelper.Keyword],
            MaxLength = 200,
            MaxValue = 1000,
            MinLength = 2,
            MinValue = 12,
            Name = EntitiesHelper.MultiLanguage,
            NumberDecimals = 13,
            Pattern = "dd/mm/yyyy",
            Replaces = [EntitiesHelper.Resource],
            PublicationLevel = PublicationLevel.Public,
            PublicationLevelProposal = PublicationLevel.Internal,
            Publisher = EntitiesHelper.Agent,
            PublisherId = EntitiesHelper.Agent.Id,
            RegistrationStatus = RegistrationStatus.Retired,
            RegistrationStatusProposal = RegistrationStatus.PreferredStandard,
            ResponsibleDeputy = EntitiesHelper.IopPerson,
            ResponsibleDeputyId = EntitiesHelper.IopPerson.Id,
            ResponsiblePerson = EntitiesHelper.IopPerson,
            ResponsiblePersonId = EntitiesHelper.IopPerson.Id,
            Themes = ["100"],
            ValidFrom = new DateTimeOffset(2024, 12, 12, 0, 0, 0, TimeSpan.Zero),
            ValidTo = new DateTimeOffset(2030, 12, 12, 0, 0, 0, TimeSpan.Zero),
            Version = "1.0.0"
        };
}
