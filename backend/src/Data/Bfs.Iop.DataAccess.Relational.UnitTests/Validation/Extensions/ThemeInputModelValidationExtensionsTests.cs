using AwesomeAssertions;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.DataAccess.Relational.Validation.Extensions;
using Bfs.Iop.DataAccess.Vocabularies;
using FluentValidation;
using NSubstitute;

namespace Bfs.Iop.DataAccess.Relational.UnitTests.Validation.Extensions;

[TestFixture(TestOf = typeof(ThemeInputModelValidationExtensions))]
internal sealed class ThemeInputModelValidationExtensionsTests
{
    private const string SwissTaxonomy = "Concept_DATASET_THEME";
    private const string EuTaxonomy = "VOCAB_EU_DATA_THEME";
    private const string SwissUri = "https://register.ld.admin.ch/i14y/concept/DV_DCAT_DATASET_THEME/101";
    private const string EuUri = "http://publications.europa.eu/resource/authority/data-theme/101";
    private const string AgriUri = "http://publications.europa.eu/resource/authority/data-theme/AGRI";

    private InlineValidator<IEnumerable<ThemeInputModel>> _validator = null!;

    [SetUp]
    public void SetUp()
    {
        var vocabulariesService = Substitute.For<IVocabulariesService>();

        vocabulariesService.TryGetVocabulary<ThemeTaxonomiesVocabulary>(Arg.Any<CancellationToken>())
            .Returns(new ThemeTaxonomiesVocabulary
            {
                Entries = [new VocabularyEntryModel { Code = SwissTaxonomy }, new VocabularyEntryModel { Code = EuTaxonomy }]
            });

        vocabulariesService.TryGetVocabulary(SwissTaxonomy, Arg.Any<CancellationToken>())
            .Returns(new VocabularyModel { Identifier = SwissTaxonomy, Entries = [new VocabularyEntryModel { Code = "101", Uri = SwissUri }] });

        vocabulariesService.TryGetVocabulary(EuTaxonomy, Arg.Any<CancellationToken>())
            .Returns(new VocabularyModel
            {
                Identifier = EuTaxonomy,
                Entries = [new VocabularyEntryModel { Code = "101", Uri = EuUri }, new VocabularyEntryModel { Code = "AGRI", Uri = AgriUri }]
            });

        _validator = [];
        _validator.RuleFor(x => x).MustContainOnlyDistinctThemes(vocabulariesService);
    }

    private bool WhenValidating(params ThemeInputModel[] themes) => _validator.Validate(themes).IsValid;

    [Test]
    public void Given_two_themes_sharing_a_code_When_each_gives_its_own_uri_Then_both_are_accepted()
    {
        // Act & Assert
        WhenValidating(
            new() { Code = "101", Uri = SwissUri },
            new() { Code = "101", Uri = EuUri }).Should().BeTrue();
    }

    [Test]
    public void Given_one_theme_named_once_by_code_and_once_by_uri_When_validating_Then_it_is_refused()
    {
        // Act & Assert
        WhenValidating(new() { Code = "AGRI" }, new() { Uri = AgriUri }).Should().BeFalse();
    }
}
