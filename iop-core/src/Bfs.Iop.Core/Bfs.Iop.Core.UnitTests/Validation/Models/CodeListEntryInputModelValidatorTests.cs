using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Entities;
using Bfs.Iop.Core.UnitTests.Helpers;
using Bfs.Iop.Core.Validation.Models;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using FluentValidation;

namespace Bfs.Iop.Core.UnitTests.Validation.Models;

[TestFixture(TestOf = typeof(CodeListEntryInputModelValidator))]
internal sealed class CodeListEntryInputModelValidatorTests
{
    [Test]
    public void Given_codeListEntryInputModel_and_missing_stuff_in_validationContext_When_Validate_Then_throw_KeyNotFoundException()
    {
        // Arrange
        var subject = CreateValidator();
        var model = ModelsHelper.CodeListEntryInputModel with { Code = "toto" };

        // Act
        var context1 = new ValidationContext<CodeListEntryInputModel>(model);
        var action1 = () => subject.Validate(context1);

        var context2 = new ValidationContext<CodeListEntryInputModel>(model);
        context2.RootContextData[ValidationContextDataKeys.ConceptEntityKey] = new IopConcept();
        context2.RootContextData[ValidationContextDataKeys.AllCodeListEntriesCodesKey] = Enumerable.Empty<string>();
        var action2 = () => subject.Validate(context2);

        // Assert
        using var _ = new AssertionScope();
        action1.Should().ThrowExactly<KeyNotFoundException>();
        action2.Should().NotThrow();
    }

    [TestCase("abc", CodeListEntryValueType.Numeric, 10, false)]
    [TestCase("", CodeListEntryValueType.Numeric, 10, false)]
    [TestCase("11", CodeListEntryValueType.Numeric, 10, true)]
    [TestCase("1 1", CodeListEntryValueType.Numeric, 10, false)]
    [TestCase("1.23", CodeListEntryValueType.Numeric, 10, true)]
    [TestCase("1.230", CodeListEntryValueType.Numeric, 10, false)]
    [TestCase("011", CodeListEntryValueType.Numeric, 10, false)]
    [TestCase("1.23.45", CodeListEntryValueType.Numeric, 10, false)]
    [TestCase("0.3", CodeListEntryValueType.Numeric, 10, true)]
    [TestCase("0", CodeListEntryValueType.Numeric, 10, true)]
    [TestCase("0.123456789", CodeListEntryValueType.Numeric, 5, false)]
    [TestCase("abc", CodeListEntryValueType.String, 10, true)]
    [TestCase("ab c", CodeListEntryValueType.String, 10, false)]
    [TestCase(" abc", CodeListEntryValueType.String, 10, false)]
    [TestCase("abc ", CodeListEntryValueType.String, 10, false)]
    [TestCase("", CodeListEntryValueType.String, 10, false)]
    [TestCase("OneBigCodeWithNoReason", CodeListEntryValueType.String, 5, false)]
    public void Given_codeListEntryInputModel_When_Validate_Code_Then_expected(
        string code, 
        CodeListEntryValueType codeListEntryValueType,
        int codeListEntryMaxLength,
        bool expected)
    {
        // Arrange
        var concept = new IopConcept() 
        { 
            ConceptType = ConceptType.CodeList,
            CodeListEntryValueType = codeListEntryValueType,
            CodeListEntryValueMaxLength = codeListEntryMaxLength
        };

        var model = ModelsHelper.CodeListEntryInputModel with { Code = code };

        var validationContext = new ValidationContext<CodeListEntryInputModel>(model);
        validationContext.RootContextData[ValidationContextDataKeys.ConceptEntityKey] = concept;
        validationContext.RootContextData[ValidationContextDataKeys.AllCodeListEntriesCodesKey] = Enumerable.Empty<string>();
        var subject = CreateValidator();

        // Act
        var result = subject.Validate(validationContext);

        // Assert
        result.IsValid.Should().Be(expected);
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase(" abc")]
    [TestCase("a bc")]
    [TestCase("abc ")]
    [TestCase("NonExistingCode")]
    public void Given_CodeListEntryInputModel_with_invalid_ParentCode_When_Validate_ParentCode_Then_fails(string parentCode)
    {
        // Arrange
        var concept = new IopConcept()
        {
            ConceptType = ConceptType.CodeList,
        };

        var model = ModelsHelper.CodeListEntryInputModel with { Code = "toto", ParentCode = parentCode };

        var validationContext = new ValidationContext<CodeListEntryInputModel>(model);
        validationContext.RootContextData[ValidationContextDataKeys.ConceptEntityKey] = concept;
        validationContext.RootContextData[ValidationContextDataKeys.AllCodeListEntriesCodesKey] = Enumerable.Empty<string>();
        var subject = CreateValidator();

        // Act
        var result = subject.Validate(validationContext);

        // Assert
        result.IsValid.Should().Be(false);
    }

    private static CodeListEntryInputModelValidator CreateValidator() =>
        new(new InlineValidator<AnnotationInputModel>());
}
