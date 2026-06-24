using AutoMapper;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Partner.Business.Mappings;
using Bfs.Iop.Partner.Business.UnitTests.Helpers;
using Bfs.Iop.Partner.Models.ConceptsInput;

namespace Bfs.Iop.Partner.Business.UnitTests.Mappings;

[TestFixture(TestOf = typeof(ConceptInputProfiles))]
internal sealed class ConceptInputProfilesTests
{
    private static readonly IMapper _mapper = TestsHelper.GetFullMapperConfiguration().CreateMapper();

    [TestCaseSource(nameof(GetTestCases))]
    public void Given_base_model_When_Mapping_to_ConceptApiInput_Then_Mapping_Ok(ConceptInputBase subject)
    {
        // Act
        var result = _mapper.Map<IopConceptInputModel>(subject);

        // Assert
        using var _ = new AssertionScope();
        result.ConformsTo.Should().HaveCount(subject.ConformsTo.Count());
        result.Description.Should().BeEquivalentTo(subject.Description);
        result.Identifiers.Count().Should().Be(subject.Identifiers.Count());
        result.Identifiers.First().Should().Be(subject.Identifiers.First());
        result.Keywords.Should().HaveCount(subject.Keywords.Count());
        result.Name.Should().BeEquivalentTo(subject.Name);
        result.Publisher.Should().BeEquivalentTo(subject.Publisher);
        result.ResponsibleDeputy.Should().BeEquivalentTo(subject.ResponsibleDeputy);
        result.ResponsiblePerson.Should().BeEquivalentTo(subject.ResponsiblePerson);
        result.Themes.Should().HaveCount(subject.Themes.Count());
        result.ValidFrom.Should().Be(subject.ValidFrom);
        result.ValidTo.Should().Be(subject.ValidTo);
        result.Version.Should().Be(subject.Version);
    }

    [Test]
    public void Given_StringConcept_When_Mapping_to_ConceptApiInput_Then_Mapping_Ok()
    {
        // Arrange
        var subject = ModelsHelper.StringConceptInputExample;

        // Act
        var result = _mapper.Map<IopConceptInputModel>(subject);

        // Assert
        using var _ = new AssertionScope();
        result.MaxLength.Should().Be(subject.MaxLength);
        result.MinLength.Should().Be(subject.MinLength);
        result.Pattern.Should().Be(subject.Pattern);
        result.ConceptType.Should().Be(ConceptType.String);
    }

    [Test]
    public void Given_DateConcept_When_Mapping_to_ConceptApiInput_Then_Mapping_Ok()
    {
        // Arrange
        var subject = ModelsHelper.DateConceptInputExample;

        // Act
        var result = _mapper.Map<IopConceptInputModel>(subject);

        // Assert
        using var _ = new AssertionScope();
        result.Pattern.Should().Be(subject.Pattern);
        result.ConceptType.Should().Be(ConceptType.Date);
    }


    [Test]
    public void Given_NumericConcept_When_Mapping_to_ConceptApiInput_Then_Mapping_Ok()
    {
        // Arrange
        var subject = ModelsHelper.NumericConceptInputExample;

        // Act
        var result = _mapper.Map<IopConceptInputModel>(subject);

        // Assert
        using var _ = new AssertionScope();
        result.MaxValue.Should().Be(subject.MaxValue);
        result.MeasurementUnit.Should().Be(subject.MeasurementUnit);
        result.MinValue.Should().Be(subject.MinValue);
        result.NumberDecimals.Should().Be(subject.NumberDecimals);
        result.Pattern.Should().Be(subject.Pattern);
        result.ConceptType.Should().Be(ConceptType.Numeric);
    }


    [Test]
    public void Given_CodeListConcept_When_Mapping_to_ConceptApiInput_Then_Mapping_Ok()
    {
        // Arrange
        var subject = ModelsHelper.CodeListConceptInputExample;

        // Act
        var result = _mapper.Map<IopConceptInputModel>(subject);

        // Assert
        using var _ = new AssertionScope();
        result.CodeListEntryDefaultSortProperty.Should().Be(subject.CodeListEntryDefaultSortProperty);
        result.CodeListEntryValueMaxLength.Should().Be(subject.CodeListEntryValueMaxLength);
        result.CodeListEntryValueType.Should().Be(subject.CodeListEntryValueType);
        result.ConceptType.Should().Be(ConceptType.CodeList);
    }

    private static IEnumerable<TestCaseData> GetTestCases()
    {
        yield return new TestCaseData(ModelsHelper.CodeListConceptInputExample).SetArgDisplayNames("Test CodeList");
        yield return new TestCaseData(ModelsHelper.DateConceptInputExample).SetArgDisplayNames("Test Date");
        yield return new TestCaseData(ModelsHelper.StringConceptInputExample).SetArgDisplayNames("Test String");
        yield return new TestCaseData(ModelsHelper.NumericConceptInputExample).SetArgDisplayNames("Test Numeric");
    }
}
