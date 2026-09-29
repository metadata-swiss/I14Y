using AwesomeAssertions;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.DataAccess.Relational.Validation.Vocabularies;
using Bfs.Iop.DataAccess.Vocabularies;
using NSubstitute;

namespace Bfs.Iop.DataAccess.Relational.UnitTests.Validation.Vocabularies;

[TestFixture(TestOf = typeof(ThemeInputModelValidator))]
internal sealed class ThemeInputModelValidatorTests
{
    private const string SwissTaxonomy = "Concept_DATASET_THEME";
    private const string EuTaxonomy = "VOCAB_EU_DATA_THEME";
    private const string SwissUri = "https://register.ld.admin.ch/i14y/concept/DV_DCAT_DATASET_THEME/101";
    private const string EuUri = "http://publications.europa.eu/resource/authority/data-theme/AGRI";

    private IVocabulariesService _vocabulariesService = null!;

    // Two taxonomies, each with its own theme: the shape the feature actually runs in.
    [SetUp]
    public void SetUp()
    {
        _vocabulariesService = Substitute.For<IVocabulariesService>();

        _vocabulariesService.TryGetVocabulary<ThemeTaxonomiesVocabulary>(Arg.Any<CancellationToken>())
            .Returns(new ThemeTaxonomiesVocabulary
            {
                Entries =
                [
                    new VocabularyEntryModel { Code = SwissTaxonomy },
                    new VocabularyEntryModel { Code = EuTaxonomy }
                ]
            });

        _vocabulariesService.TryGetVocabulary(SwissTaxonomy, Arg.Any<CancellationToken>())
            .Returns(new VocabularyModel
            {
                Identifier = SwissTaxonomy,
                Entries = [new VocabularyEntryModel { Code = "101", Uri = SwissUri }]
            });

        _vocabulariesService.TryGetVocabulary(EuTaxonomy, Arg.Any<CancellationToken>())
            .Returns(new VocabularyModel
            {
                Identifier = EuTaxonomy,
                Entries = [new VocabularyEntryModel { Code = "AGRI", Uri = EuUri }]
            });
    }

    private ThemeInputModelValidator CreateValidator() => new(_vocabulariesService);

    [Test]
    public void Given_a_code_When_validating_Then_ok()
    {
        // Arrange
        var subject = CreateValidator();

        // Act
        var result = subject.Validate(new ThemeInputModel { Code = "101" });

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Test]
    public void Given_a_code_from_another_taxonomy_When_validating_Then_ok()
    {
        // Arrange
        var subject = CreateValidator();

        // Act
        var result = subject.Validate(new ThemeInputModel { Code = "AGRI" });

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Test]
    public void Given_a_uri_When_validating_Then_ok()
    {
        // Arrange
        var subject = CreateValidator();

        // Act
        var result = subject.Validate(new ThemeInputModel { Uri = EuUri });

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Test]
    public void Given_a_code_and_a_uri_of_the_same_theme_When_validating_Then_ok()
    {
        // Arrange
        var subject = CreateValidator();

        // Act
        var result = subject.Validate(new ThemeInputModel { Code = "101", Uri = SwissUri });

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Test]
    public void Given_a_code_and_a_uri_of_different_themes_When_validating_Then_throw_error()
    {
        // Arrange
        var subject = CreateValidator();

        // Act
        var result = subject.Validate(new ThemeInputModel { Code = "101", Uri = EuUri });

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.ErrorMessage.Contains("do not refer to the same theme"));
    }

    [Test]
    public void Given_neither_a_code_nor_a_uri_When_validating_Then_throw_error()
    {
        // Arrange
        var subject = CreateValidator();

        // Act
        var result = subject.Validate(new ThemeInputModel());

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.ErrorMessage.Contains("Either 'code' or 'uri' must be provided"));
    }

    [Test]
    public void Given_a_malformed_uri_When_validating_Then_throw_error()
    {
        // Arrange
        var subject = CreateValidator();

        // Act
        var result = subject.Validate(new ThemeInputModel { Uri = "not-a-uri" });

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.ErrorMessage.Contains("not a well-formed absolute URI"));
    }

    [Test]
    public void Given_an_unknown_code_When_validating_Then_throw_error()
    {
        // Arrange
        var subject = CreateValidator();

        // Act
        var result = subject.Validate(new ThemeInputModel { Code = "unknown" });

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.ErrorMessage.Contains("does not resolve to any theme"));
    }

    /// The two taxonomies are made to share the code "101", each pointing at its own theme.
    private void GivenACodeSharedByBothTaxonomies()
    {
        _vocabulariesService.TryGetVocabulary(EuTaxonomy, Arg.Any<CancellationToken>())
            .Returns(new VocabularyModel
            {
                Identifier = EuTaxonomy,
                Entries = [new VocabularyEntryModel { Code = "101", Uri = EuUri }]
            });
    }

    [Test]
    public void Given_a_code_shared_by_two_taxonomies_When_validating_Then_throw_error()
    {
        // Arrange
        GivenACodeSharedByBothTaxonomies();
        var subject = CreateValidator();

        // Act
        var result = subject.Validate(new ThemeInputModel { Code = "101" });

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.ErrorMessage.Contains("exists in several registered theme taxonomies"));
    }

    [Test]
    public void Given_a_code_shared_by_two_taxonomies_and_its_uri_When_validating_Then_ok()
    {
        // Arrange
        GivenACodeSharedByBothTaxonomies();
        var subject = CreateValidator();

        // Act
        var result = subject.Validate(new ThemeInputModel { Code = "101", Uri = EuUri });

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Test]
    public void Given_a_uri_outside_every_registered_taxonomy_When_validating_Then_throw_error()
    {
        // Arrange
        var subject = CreateValidator();

        // Act
        var result = subject.Validate(new ThemeInputModel { Uri = "https://example.org/not-a-theme" });

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.ErrorMessage.Contains("does not resolve to any entry"));
    }
}
