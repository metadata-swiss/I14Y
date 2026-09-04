using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.Partner.Models.ConceptsInput;

namespace Bfs.Iop.Partner.Business.Examples;

public static class ConceptInputExamples
{
    private static readonly string IdentifierExample = "Concept_identifier";
    private static readonly string PatternDateExample = "yyyy-mm-dd";
    private static readonly DateTimeOffset ValidFromExample = new(year: 2024, month: 12, day: 25, hour: 0, minute: 0, second: 0, TimeSpan.Zero);
    private static readonly DateTimeOffset ValidToExample = DateTimeOffset.MaxValue;
    private static readonly string VersionExample = "1.0.0";

    private static readonly MultiLanguageModel MultiLanguageExample = new()
    {
        De = "Text in Deutsch",
        En = "Text in English",
        Fr = "Texte en Français",
        It = "Testo in Italiano",
        Rm = "Text in Rumantsch"
    };

    private static readonly KeywordModel KeywordExample = new()
    {
        Label = MultiLanguageExample,
        Uri = "http://www.example.com"
    };

    private static readonly CodeInputModel CodeExample = new()
    {
        Code = "101-AAA"
    };

    private static readonly ResourceModel ResourceExample = new()
    {
        Uri = "http://www.example.com",
        Label = MultiLanguageExample
    };

    private static readonly ConceptReferenceModel ReplacesExample = new()
    {
        Uri = "https://register.ld.admin.ch/i14y/concept/Concept_identifier/version/1.0.0",
        Name = MultiLanguageExample
    };

    private static readonly EmailInputModel PersonIdentifierExample = new()
    {
        Email = "name.123@domain.com"
    };

    private static readonly IdentifierInputModel PublisherIdentifierExample = new()
    {
        Identifier = "test-organization"
    };

    private static readonly CodeListConceptInput CodeListConceptInputExample = new()
    {
        CodeListEntryDefaultSortProperty = CodeListEntrySortProperty.Code,
        CodeListEntryValueType = CodeListEntryValueType.String,
        CodeListEntryValueMaxLength = 0,
        ConformsTo = Enumerable.Repeat(ResourceExample, 1).ToList(),
        Replaces = Enumerable.Repeat(ReplacesExample, 1).ToList(),
        Description = MultiLanguageExample,
        Identifiers = [IdentifierExample],
        Keywords = Enumerable.Repeat(KeywordExample, 1).ToList(),
        Name = MultiLanguageExample,
        Publisher = PublisherIdentifierExample,
        ResponsibleDeputy = PersonIdentifierExample,
        ResponsiblePerson = PersonIdentifierExample,
        Themes = Enumerable.Repeat(CodeExample, 1).ToList(),
        ValidFrom = ValidFromExample,
        ValidTo = ValidToExample,
        Version = VersionExample,
    };

    private static readonly DateConceptInput DateConceptInputExample = new()
    {
        Description = MultiLanguageExample,
        ConformsTo = Enumerable.Repeat(ResourceExample, 1).ToList(),
        Replaces = Enumerable.Repeat(ReplacesExample, 1).ToList(),
        Identifiers = [IdentifierExample],
        Keywords = Enumerable.Repeat(KeywordExample, 1).ToList(),
        Name = MultiLanguageExample,
        Pattern = PatternDateExample,
        Publisher = PublisherIdentifierExample,
        ResponsibleDeputy = PersonIdentifierExample,
        ResponsiblePerson = PersonIdentifierExample,
        Themes = Enumerable.Repeat(CodeExample, 1).ToList(),
        ValidFrom = ValidFromExample,
        ValidTo = ValidToExample,
        Version = VersionExample,
    };

    private static readonly NumericConceptInput NumericConceptInputExample = new()
    {
        Description = MultiLanguageExample,
        ConformsTo = Enumerable.Repeat(ResourceExample, 1).ToList(),
        Replaces = Enumerable.Repeat(ReplacesExample, 1).ToList(),
        Identifiers = [IdentifierExample],
        Keywords = Enumerable.Repeat(KeywordExample, 1).ToList(),
        MaxValue = 999,
        MeasurementUnit = "mm",
        MinValue = 0,
        Name = MultiLanguageExample,
        NumberDecimals = 0,
        Publisher = PublisherIdentifierExample,
        ResponsibleDeputy = PersonIdentifierExample,
        ResponsiblePerson = PersonIdentifierExample,
        Themes = Enumerable.Repeat(CodeExample, 1).ToList(),
        ValidFrom = ValidFromExample,
        ValidTo = ValidToExample,
        Version = VersionExample,
    };

    private static readonly StringConceptInput StringConceptInputExample = new()
    {
        ConformsTo = Enumerable.Repeat(ResourceExample, 1).ToList(),
        Replaces = Enumerable.Repeat(ReplacesExample, 1).ToList(),
        Description = MultiLanguageExample,
        Identifiers = [IdentifierExample],
        Keywords = Enumerable.Repeat(KeywordExample, 1).ToList(),
        MaxLength = 0,
        MinLength = 0,
        Name = MultiLanguageExample,
        Pattern = "string",
        Publisher = PublisherIdentifierExample,
        ResponsibleDeputy = PersonIdentifierExample,
        ResponsiblePerson = PersonIdentifierExample,
        Themes = Enumerable.Repeat(CodeExample, 1).ToList(),
        ValidFrom = ValidFromExample,
        ValidTo = ValidToExample,
        Version = VersionExample,
    };

    public static ConceptInputBase GetConceptInputExample(ConceptType conceptType) =>
        conceptType switch
        {
            ConceptType.CodeList => CodeListConceptInputExample,
            ConceptType.Date => DateConceptInputExample,
            ConceptType.Numeric => NumericConceptInputExample,
            ConceptType.String => StringConceptInputExample,
            _ => throw new NotSupportedException($"The concept type '{conceptType}' is not supported.")
        };
}
