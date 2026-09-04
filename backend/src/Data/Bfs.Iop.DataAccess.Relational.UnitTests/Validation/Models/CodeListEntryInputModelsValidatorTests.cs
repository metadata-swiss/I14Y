using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Bfs.Iop.Core.UnitTests.Helpers;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Relational.Entities;
using Bfs.Iop.DataAccess.Relational.Validation.Models;
using FluentValidation;

namespace Bfs.Iop.DataAccess.Relational.UnitTests.Validation.Models;

[TestFixture(TestOf = typeof(CodeListEntryInputModelsValidator))]
internal sealed class CodeListEntryInputModelsValidatorTests
{
    [Test]
    public void Given_codeListEntryInputModels_without_repeated_codes_and_database_codes_missing_in_validationContext_When_Validate_Then_throw_KeyNotFoundException()
    {
        // Arrange
        var subject = new CodeListEntryInputModelsValidator(new InlineValidator<CodeListEntryInputModel>());

        var models = new[]
        {
            ModelsHelper.CodeListEntryInputModel with { Code = "toto" },
        };

        // Act
        var context = new ValidationContext<IEnumerable<CodeListEntryInputModel>>(models);
        var action = () => subject.Validate(context);

        // Assert
        action.Should().ThrowExactly<KeyNotFoundException>();
    }

    [Test]
    public void Given_codeListEntryInputModels_with_repeated_codes_When_Validate_Then_expected()
    {
        // Arrange
        var subject = new CodeListEntryInputModelsValidator(new InlineValidator<CodeListEntryInputModel>());

        var models = new[]
        { 
            ModelsHelper.CodeListEntryInputModel with { Code = "toto" },
            ModelsHelper.CodeListEntryInputModel with { Code = "toto" }
        };

        // Act
        var context = new ValidationContext<IEnumerable<CodeListEntryInputModel>>(models);
        context.RootContextData[ValidationContextDataKeys.AllCodeListEntriesCodesKey] = models.Select(x => x.Code);
        var result = subject.Validate(context);

        // Assert
        using var _ = new AssertionScope();
        result.IsValid.Should().Be(false);
        result.Errors.Should().HaveCount(1);
    }

    [Test]
    public void Given_codeListEntryInputModels_with_repeated_codes_in_database_When_Validate_Then_expected()
    {
        // Arrange
        var subject = new CodeListEntryInputModelsValidator(new InlineValidator<CodeListEntryInputModel>());

        var models = new[]
        {
            ModelsHelper.CodeListEntryInputModel with { Code = "toto" },
            ModelsHelper.CodeListEntryInputModel with { Code = "tata" }
        };

        var entities = new[]
        {
            new CodeListEntry() { Code = "toto"}
        };

        // Act
        var context = new ValidationContext<IEnumerable<CodeListEntryInputModel>>(models);
        context.RootContextData[ValidationContextDataKeys.AllCodeListEntriesCodesKey] = models.Select(x => x.Code).Concat(entities.Select(e => e.Code));
        var result = subject.Validate(context);

        // Assert
        using var _ = new AssertionScope();
        result.IsValid.Should().Be(false);
        result.Errors.Should().HaveCount(1);
    }
}
