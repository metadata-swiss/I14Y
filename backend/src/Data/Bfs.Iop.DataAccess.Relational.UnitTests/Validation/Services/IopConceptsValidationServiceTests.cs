using AwesomeAssertions;
using Bfs.Iop.Core.UnitTests.Helpers;
using Bfs.Iop.Core.Validation.Services;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Relational.Entities;
using Bfs.Iop.DataAccess.Relational.UnitTests.Helpers;
using FluentValidation;

namespace Bfs.Iop.DataAccess.Relational.UnitTests.Validation.Services;

[TestFixture(TestOf = typeof(IopConceptsValidationService))]
internal sealed class IopConceptsValidationServiceTests
{
    private IopDbContext _dbContext = null!;

    [SetUp]
    public void Setup() => _dbContext = TestHelper.CreateFakeIopDbContext();

    [TearDown]
    public void Teardown()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [TestCaseSource(nameof(CreateCodeListEntriesCircularDependencyTestCases))]
    public void Given_codeListEntryInputModels_When_EnsureCodeListEntriesCanBeAdded_Then_circular_dependencies_test_result_expected(
        IEnumerable<CodeListEntryInputModel> inputModels,
        IEnumerable<CodeListEntry> entities,
        bool shouldThrowValidationException)
    {
        // Arrange
        var fakeIopConcept = EntitiesHelper.IopConcept;
        var subject = CreateService(_dbContext);

        // Act
        var action = () => subject.EnsureCodeListEntriesCanBeAdded(inputModels, fakeIopConcept, entities);

        // Assert
        if (shouldThrowValidationException)
        {
            action.Should().ThrowExactly<ValidationException>();
        }
        else
        {
            action.Should().NotThrow();
        }
    }

    [TestCase("123", CodeListEntryValueType.String, CodeListEntryValueType.Numeric, true)]
    [TestCase("123", CodeListEntryValueType.Numeric, CodeListEntryValueType.String, true)]
    [TestCase("abc", CodeListEntryValueType.String, CodeListEntryValueType.Numeric, false)]
    public void Given_iopConceptInputModel_with_CodeListEntryType_changed_When_EnsureConceptCanBeUpdated_Then_expected(
        string codeListEntryCode,
        CodeListEntryValueType oldType,
        CodeListEntryValueType newType,
        bool shouldBeValid)
    {
        // Arrange
        var fakeIdentifier = "fake";
        var fakeVersion = "0.0.1";
        var id = Guid.NewGuid();

        var iopConcept = new IopConcept
        {
            Identifiers = [fakeIdentifier],
            Version = fakeVersion,
            Id = id,
            ConceptType = ConceptType.CodeList,
            CodeListEntryValueType = oldType
        };

        var codeListEntry = new CodeListEntry()
        {
            IopConceptId = iopConcept.Id,
            Code = codeListEntryCode
        };

        var model = ModelsHelper.IopConceptInputModel with
        {
            Identifiers = [fakeIdentifier],
            Version = fakeVersion,
            ConceptType = ConceptType.CodeList,
            CodeListEntryValueType = newType
        };

        var subject = CreateService(_dbContext);

        // Act
        var action = () => subject.EnsureConceptCanBeUpdated(id, model, iopConcept, [codeListEntry]);

        // Assert
        if (shouldBeValid)
        {
            action.Should().NotThrow();
        }
        else
        {
            action.Should().ThrowExactly<ValidationException>();
        }
    }

    [TestCase("abcde", 5, 10, true)]
    [TestCase("abcdefgh", 10, 5, false)]
    public void Given_iopConceptInputModel_with_CodeListEntryMaxLength_changed_When_EnsureConceptCanBeUpdated_Then_expected(
        string codeListEntryCode,
        int oldMaxLengthValue,
        int newMaxLengthValue,
        bool shouldBeValid)
    {
        // Arrange
        var fakeIdentifier = "fake";
        var fakeVersion = "0.0.1";
        var id = Guid.NewGuid();

        var iopConcept = new IopConcept
        {
            Identifiers = [fakeIdentifier],
            Version = fakeVersion,
            Id = id,
            ConceptType = ConceptType.CodeList,
            CodeListEntryValueType = CodeListEntryValueType.String,
            CodeListEntryValueMaxLength = oldMaxLengthValue,
        };

        var codeListEntry = new CodeListEntry()
        {
            IopConceptId = iopConcept.Id,
            Code = codeListEntryCode
        };

        var model = ModelsHelper.IopConceptInputModel with
        {
            Identifiers = [fakeIdentifier],
            Version = fakeVersion,
            ConceptType = ConceptType.CodeList,
            CodeListEntryValueType = CodeListEntryValueType.String,
            CodeListEntryValueMaxLength = newMaxLengthValue,
        };

        var subject = CreateService(_dbContext);

        // Act
        var action = () => subject.EnsureConceptCanBeUpdated(id, model, iopConcept, [codeListEntry]);

        // Assert
        if (shouldBeValid)
        {
            action.Should().NotThrow();
        }
        else
        {
            action.Should().ThrowExactly<ValidationException>();
        }
    }

    [Test]
    public async Task Given_iopConceptVersionInputModel_with_inexisting_Identifier_and_new_Version_When_EnsureConceptCanBeAdded_Then_fails()
    {
        // Arrange
        var fakeIdentifier = "fake";
        var fakeVersion = "0.0.1";
        var fakeIdentifierNew = "fake-new";
        var fakeVersionNew = "0.0.2";

        var iopConcept = new IopConcept
        {
            Identifiers = [fakeIdentifier],
            Version = fakeVersion
        };

        _dbContext.IopConcepts.Add(iopConcept);
        _dbContext.SaveChanges();

        var subject = CreateService(_dbContext);
        var model = ModelsHelper.IopConceptInputModel with
        {
            Identifiers = [fakeIdentifierNew],
            Version = fakeVersionNew 
        };

        // Act
        var action = () => subject.EnsureConceptVersionCanBeAdded(iopConcept.Id, model);

        // Assert
        await action.Should().ThrowAsync<ValidationException>();
    }

    private static IEnumerable<TestCaseData> CreateCodeListEntriesCircularDependencyTestCases()
    {
        yield return new TestCaseData(
            new[]
            {
                ModelsHelper.CodeListEntryInputModel with { Code = "1", ParentCode = "111" },
                ModelsHelper.CodeListEntryInputModel with { Code = "11", ParentCode = "1" },
                ModelsHelper.CodeListEntryInputModel with { Code = "111", ParentCode = "11" },
            },
            Enumerable.Empty<CodeListEntry>(),
            true).SetArgDisplayNames("Test 1");

        yield return new TestCaseData(
            new[]
            {
                ModelsHelper.CodeListEntryInputModel with { Code = "1" },
                ModelsHelper.CodeListEntryInputModel with { Code = "11", ParentCode = "1" },
                ModelsHelper.CodeListEntryInputModel with { Code = "111", ParentCode = "11" },
            },
            Enumerable.Empty<CodeListEntry>(),
            false).SetArgDisplayNames("Test 2");

        yield return new TestCaseData(
            new[]
            {
                ModelsHelper.CodeListEntryInputModel with { Code = "1", ParentCode = "111" },
            },
            new[]
            {
                new CodeListEntry() { Code = "111", ParentCodeListEntry = new CodeListEntry() { Code = "1" } },
                new CodeListEntry() { Code = "1111" }
            },
            true).SetArgDisplayNames("Test 3 - test of an edit case");
    }

    private static IopConceptsValidationService CreateService(IopDbContext dbContext) =>
        new(dbContext,
            new InlineValidator<IopConceptInputModel>(),
            new InlineValidator<CodeListEntryInputModel>(),
            new InlineValidator<IEnumerable<CodeListEntryInputModel>>(),
            new InlineValidator<IEnumerable<AnnotationInputModel>>());
}
    