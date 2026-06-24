using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Authorization;
using Bfs.Iop.Core.Authorization.Contracts;
using Bfs.Iop.Core.Common.Exceptions;
using Bfs.Iop.Core.Data;
using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.Core.Services;
using Bfs.Iop.Core.Services.Contracts;
using Bfs.Iop.Core.Tools;
using Bfs.Iop.Core.UnitTests.Helpers;
using Bfs.Iop.Core.Validation.Services;
using Bfs.Iop.Infrastructure.Security.Helpers;
using Bfs.Iop.Infrastructure.Security.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using System.Security.Claims;

namespace Bfs.Iop.Core.UnitTests.Services;

[TestFixture(TestOf = typeof(IopConceptsService))]
internal sealed class IopConceptsServiceTests
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

    [Test]
    public void Given_Id_from_concept_with_numeric_codeListEntries_sorted_by_code_by_default_When_GetConcept_Then_return_sorted_codeListEntries()
    {
        // Arrange
        var concept = EntitiesHelper.IopConcept;
        concept.ConceptType = ConceptType.CodeList;
        concept.CodeListEntryValueType = CodeListEntryValueType.Numeric;
        concept.CodeListEntryValueMaxLength = 10;
        concept.CodeListEntryDefaultSortProperty = CodeListEntrySortProperty.Code;
        concept.PublicationLevel = PublicationLevel.Public;

        var codeListEntry1 = EntitiesHelper.CodeListEntry;
        codeListEntry1.Code = "10";
        codeListEntry1.IopConceptId = concept.Id;

        var codeListEntry2 = EntitiesHelper.CodeListEntry;
        codeListEntry2.Code = "1";
        codeListEntry2.IopConceptId = concept.Id;

        var codeListEntry3 = EntitiesHelper.CodeListEntry;
        codeListEntry3.Code = "0.5";
        codeListEntry3.IopConceptId = concept.Id;

        var codeListEntries = new[]
        {
            codeListEntry1,
            codeListEntry2,
            codeListEntry3
        };

        _dbContext.IopConcepts.Add(concept);
        _dbContext.CodeListEntries.AddRange(codeListEntries);
        _dbContext.SaveChanges();

        var userContextService = TestHelper.CreateFakeUserContextService([]);
        var fakeService = CreateFakeService(_dbContext, userContextService);

        // Act
        var result = fakeService
            .GetIopConcept(concept.Id, includeCodeListEntries: true, CancellationToken.None)
            .GetAwaiter()
            .GetResult();

        // Assert
        using var _ = new AssertionScope();
        result.CodeListEntries.Should().HaveCount(codeListEntries.Length);
        result.CodeListEntries!.Select(x => double.Parse(x.Code)).Should().BeInAscendingOrder();

        _dbContext.Database.EnsureDeleted();
    }

    [Test]
    public void Given_user_with_no_token_When_GetConcepts_Then_return_only_public_concepts()
    {
        // Arrange
        var concept1 = EntitiesHelper.IopConcept;
        concept1.Identifiers = ["toto1"];
        concept1.PublicationLevel = PublicationLevel.Internal;

        var concept2 = EntitiesHelper.IopConcept;
        concept2.Identifiers = ["toto2"];
        concept2.PublicationLevel = PublicationLevel.Public;

        _dbContext.IopConcepts.Add(concept1);
        _dbContext.IopConcepts.Add(concept2);
        _dbContext.SaveChanges();

        var userContextService = TestHelper.CreateFakeUserContextService([]);
        var fakeService = CreateFakeService(_dbContext, userContextService);

        // Act
        var results = fakeService.GetIopConcepts(
            conceptIdentifier: null,
            publisherIdentifier: null,
            version: null,
            publicationLevel: null,
            registrationStatus: null,
            page: 1,
            pageSize: 25,
            default).GetAwaiter().GetResult();

        // Assert
        using var _ = new AssertionScope();
        results.Results.Count().Should().Be(1);
        results.Results.Single().Identifiers.First().Should().Be("toto2");
    }

    [Test]
    public void Given_user_with_no_token_and_filter_When_GetConcepts_Then_return_correct_concept()
    {
        // Arrange
        var concept1 = EntitiesHelper.IopConcept;
        concept1.Identifiers = ["toto1"];
        concept1.PublicationLevel = PublicationLevel.Public;

        var concept2 = EntitiesHelper.IopConcept;
        concept2.Identifiers = ["toto2"];
        concept2.PublicationLevel = PublicationLevel.Public;

        _dbContext.IopConcepts.Add(concept1);
        _dbContext.IopConcepts.Add(concept2);
        _dbContext.SaveChanges();

        var userContextService = TestHelper.CreateFakeUserContextService([]);
        var fakeService = CreateFakeService(_dbContext, userContextService);

        // Act
        var results = fakeService.GetIopConcepts(
            conceptIdentifier: "toto2",
            publisherIdentifier: null,
            version: null,
            publicationLevel: null,
            registrationStatus: null,
            page: 1,
            pageSize: 25,
            default).GetAwaiter().GetResult();

        // Assert
        using var _ = new AssertionScope();
        results.Results.Count().Should().Be(1);
        results.Results.Single().Identifiers.First().Should().Be("toto2");
    }

    [Test]
    public void Given_user_with_no_token_and_identifier_filter_When_GetConcepts_Then_return_all_concepts_with_same_identifier_and_sorted_by_version()
    {
        // Arrange
        var firstVersion = "2.0.0";
        var secondVersion = "2.0.1";
        var lastVersion = "10.0.0";

        var concept1 = EntitiesHelper.IopConcept;
        concept1.Identifiers = ["toto1"];
        concept1.PublicationLevel = PublicationLevel.Public;
        concept1.Version = firstVersion;

        var concept2 = EntitiesHelper.IopConcept;
        concept2.Identifiers = ["toto1"];
        concept2.PublicationLevel = PublicationLevel.Public;
        concept2.Version = secondVersion;

        var concept3 = EntitiesHelper.IopConcept;
        concept3.Identifiers = ["toto1"];
        concept3.PublicationLevel = PublicationLevel.Public;
        concept3.Version = lastVersion;

        _dbContext.IopConcepts.Add(concept1);
        _dbContext.IopConcepts.Add(concept2);
        _dbContext.IopConcepts.Add(concept3);
        _dbContext.SaveChanges();

        var userContextService = TestHelper.CreateFakeUserContextService([]);
        var fakeService = CreateFakeService(_dbContext, userContextService);

        // Act
        var results = fakeService.GetIopConcepts(
            conceptIdentifier: "toto1",
            publisherIdentifier: null,
            version: null,
            publicationLevel: null,
            registrationStatus: null,
            page: 1,
            pageSize: 25,
            default).GetAwaiter().GetResult();

        // Assert
        using var _ = new AssertionScope();
        results.Results.Count().Should().Be(3);
        results.TotalCount.Should().Be(3);
        results.Results.First().Version.Should().Be(lastVersion);
        results.Results.Last().Version.Should().Be(firstVersion);
    }

    [Test]
    public void Given_user_with_no_token_and_identifier_filter_with_paging_parameters_When_GetConcepts_Then_return_expected_concepts_with_same_identifier_and_sorted_by_version()
    {
        // Arrange
        var firstVersion = "2.0.0";
        var secondVersion = "2.0.1";
        var lastVersion = "10.0.0";

        var concept1 = EntitiesHelper.IopConcept;
        concept1.Identifiers = ["toto1"];
        concept1.PublicationLevel = PublicationLevel.Public;
        concept1.Version = firstVersion;

        var concept2 = EntitiesHelper.IopConcept;
        concept2.Identifiers = ["toto1"];
        concept2.PublicationLevel = PublicationLevel.Public;
        concept2.Version = secondVersion;

        var concept3 = EntitiesHelper.IopConcept;
        concept3.Identifiers = ["toto1"];
        concept3.PublicationLevel = PublicationLevel.Public;
        concept3.Version = lastVersion;

        _dbContext.IopConcepts.Add(concept1);
        _dbContext.IopConcepts.Add(concept2);
        _dbContext.IopConcepts.Add(concept3);
        _dbContext.SaveChanges();

        var userContextService = TestHelper.CreateFakeUserContextService([]);
        var fakeService = CreateFakeService(_dbContext, userContextService);

        // Act
        var results = fakeService.GetIopConcepts(
            conceptIdentifier: "toto1",
            publisherIdentifier: null,
            version: null,
            publicationLevel: null,
            registrationStatus: null,
            page: 2,
            pageSize: 2,
            default).GetAwaiter().GetResult();

        // Assert
        using var _ = new AssertionScope();
        results.Results.Count().Should().Be(1);
        results.Results.First().Version.Should().Be(firstVersion);
        results.TotalCount.Should().Be(3);
    }

    [Test]
    public void Given_ConceptInputModel_When_AddIopConcept_Then_added()
    {
        // Arrange
        var concept = ModelsHelper.IopConceptInputModel;

        var claims = new Claim[]
        {
            new(IopClaimsHelper.ClaimTypes.RoleClaimType, IopClaimsHelper.Roles.BusinessRoles.LocalDataSteward),
            new(IopClaimsHelper.ClaimTypes.AgenciesClaimType, concept.Publisher.Identifier)
        };

        var userContextService = TestHelper.CreateFakeUserContextService(claims);
        var fakeService = CreateFakeService(_dbContext, userContextService);

        // Act
        var action = () => fakeService.AddIopConcept(concept, default).GetAwaiter().GetResult();

        // Assert
        using var _ = new AssertionScope();
        action.Should().NotThrow();
        _dbContext.IopConcepts.Count().Should().Be(1);
    }

    [Test]
    public void Given_CodeListEntryInputModel_When_AddCodeListEntry_Then_added()
    {
        // Arrange
        var concept = EntitiesHelper.IopConcept;
        concept.ConceptType = ConceptType.CodeList;
        concept.CodeListEntryValueType = CodeListEntryValueType.String;
        _dbContext.IopConcepts.Add(concept);
        _dbContext.SaveChanges();

        var codeListEntry = ModelsHelper.CodeListEntryInputModel;

        var claims = new Claim[]
        {
            new(IopClaimsHelper.ClaimTypes.RoleClaimType, IopClaimsHelper.Roles.BusinessRoles.LocalDataSteward),
            new(IopClaimsHelper.ClaimTypes.AgenciesClaimType, concept.Publisher.Identifier)
        };

        var userContextService = TestHelper.CreateFakeUserContextService(claims);
        var fakeService = CreateFakeService(_dbContext, userContextService);

        // Act
        var action = () => fakeService.AddCodeListEntries(concept.Id, [codeListEntry], default).GetAwaiter().GetResult();

        // Assert
        using var _ = new AssertionScope();
        action.Should().NotThrow();
        _dbContext.CodeListEntries.Count().Should().Be(1);
        concept.ModifiedAt.Should().NotBeNull();
    }

    [Test]
    public void Given_updateModel_with_conceptType_changed_from_CodeList_to_Numeric_When_UpdateConcept_Then_expected()
    {
        // Arrange
        var concept = EntitiesHelper.IopConcept;
        concept.Id = Guid.NewGuid();
        concept.ConceptType = ConceptType.CodeList;
        concept.CodeListEntryValueType = CodeListEntryValueType.Numeric;
        concept.CodeListEntryValueMaxLength = 10;

        var codeListEntry = EntitiesHelper.CodeListEntry;
        codeListEntry.Code = "1";
        codeListEntry.IopConceptId = concept.Id;

        _dbContext.IopConcepts.Add(concept);
        _dbContext.CodeListEntries.Add(codeListEntry);

        _dbContext.SaveChanges();

        var claims = new Claim[]
        {
            new(IopClaimsHelper.ClaimTypes.RoleClaimType, IopClaimsHelper.Roles.BusinessRoles.LocalDataSteward),
            new(IopClaimsHelper.ClaimTypes.AgenciesClaimType, EntitiesHelper.Agent.Identifier)
        };

        var userContextService = TestHelper.CreateFakeUserContextService(claims);
        var subject = CreateFakeService(_dbContext, userContextService);

        var model = ModelsHelper.IopConceptInputModel with
        {
            ConceptType = ConceptType.Numeric,
            CodeListEntryValueMaxLength = 10,
            CodeListEntryValueType = CodeListEntryValueType.Numeric,
            Publisher = new IdentifierInputModel() { Identifier = EntitiesHelper.Agent.Identifier }
        };

        // Act
        var action = () => subject.UpdateConcept(concept.Id, model, default).GetAwaiter().GetResult();

        // Assert
        using var _ = new AssertionScope();
        action.Should().NotThrow();

        var entity = _dbContext.IopConcepts.First(x => x.Id == concept.Id);
        entity.ConceptType.Should().Be(ConceptType.Numeric);
        entity.CodeListEntryValueMaxLength.Should().BeNull();
        entity.CodeListEntryValueType.Should().BeNull();
        entity.CodeListEntryDefaultSortProperty.Should().BeNull();
        _dbContext.CodeListEntries.Count().Should().Be(0);
        entity.ModifiedAt.Should().NotBeNull();
    }

    [TestCase(IopClaimsHelper.Roles.BusinessRoles.StewardshipOrganizationViewer, true, true)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.Submitter, true, true)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.LocalDataSteward, true, false)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.LocalDataSteward, false, true)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.InteroperabilityService, true, false)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.SwissDataSteward, true, true)]
    public void Given_logged_user_from_same_agency_When_UpdateIsLocked_unlocked_concept_Then_expected(
        string userBusinessRole,
        bool userBelongsToConceptAgency,
        bool exceptionExpected)
    {
        // Arrange
        var concept = EntitiesHelper.IopConcept;
        _dbContext.IopConcepts.Add(concept);
        _dbContext.SaveChanges();

        var userAgency = userBelongsToConceptAgency
            ? concept.Publisher.Identifier
            : "some_toto_agency_that_is_unknown_to_everyone_here_and_everyone_else";

        var claims = new Claim[]
        {
            new(IopClaimsHelper.ClaimTypes.RoleClaimType, userBusinessRole),
            new(IopClaimsHelper.ClaimTypes.AgenciesClaimType, userAgency)
        };
        var userContextService = TestHelper.CreateFakeUserContextService(claims);

        var subject = CreateFakeService(_dbContext, userContextService);

        // Act
        var action = () => subject.UpdateIsLocked(concept.Id, true, default).GetAwaiter().GetResult();

        // Assert
        if (!exceptionExpected)
        {
            action.Should().NotThrow();
        }
        else
        {
            action.Should().ThrowExactly<ForbiddenException>();
        }
    }

    [TestCase(IopClaimsHelper.Roles.BusinessRoles.StewardshipOrganizationViewer, true, true)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.Submitter, true, true)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.LocalDataSteward, true, true)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.LocalDataSteward, false, true)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.InteroperabilityService, true, false)]
    [TestCase(IopClaimsHelper.Roles.BusinessRoles.SwissDataSteward, true, true)]
    public void Given_logged_user_from_same_agency_When_UpdateIsLocked_locked_concept_Then_expected(
        string userBusinessRole,
        bool userBelongsToConceptAgency,
        bool exceptionExpected)
    {
        // Arrange
        var concept = EntitiesHelper.IopConcept;
        concept.IsLocked = true;

        _dbContext.IopConcepts.Add(concept);
        _dbContext.SaveChanges();

        var userAgency = userBelongsToConceptAgency
            ? concept.Publisher.Identifier
            : "some_toto_agency_that_is_unknown_to_everyone_here_and_everyone_else";

        var claims = new Claim[]
        {
            new(IopClaimsHelper.ClaimTypes.RoleClaimType, userBusinessRole),
            new(IopClaimsHelper.ClaimTypes.AgenciesClaimType, userAgency)
        };
        var userContextService = TestHelper.CreateFakeUserContextService(claims);

        var subject = CreateFakeService(_dbContext, userContextService);

        // Act
        var action = () => subject.UpdateIsLocked(concept.Id, false, default).GetAwaiter().GetResult();

        // Assert
        if (!exceptionExpected)
        {
            action.Should().NotThrow();
        }
        else
        {
            action.Should().ThrowExactly<ForbiddenException>();
        }
    }

    [Test]
    public void Given_id_When_DeleteIopConcept_Then_entity_and_relations_are_deleted()
    {
        // Arrange
        var fakeCodeList = EntitiesHelper.IopConcept;
        fakeCodeList.ConceptType = ConceptType.CodeList;

        var fakeCodeListEntry = EntitiesHelper.CodeListEntry;
        fakeCodeListEntry.IopConceptId = fakeCodeList.Id;

        _dbContext.IopConcepts.Add(fakeCodeList);
        _dbContext.CodeListEntries.Add(fakeCodeListEntry);
        _dbContext.SaveChanges();

        var subject = CreateFakeService(_dbContext, TestHelper.CreateFakeUserContextServiceForInteroperabilityServiceUser());

        // Act
        var action = () => subject.DeleteIopConcept(fakeCodeList.Id, default).GetAwaiter().GetResult();

        // Assert
        using var _ = new AssertionScope();
        action.Should().NotThrow();
        _dbContext.IopConcepts.Count().Should().Be(0);
        _dbContext.CodeListEntries.Count().Should().Be(0);
        _dbContext.Annotations.Count().Should().Be(0);
    }

    [TestCase(ConceptType.CodeList)]
    [TestCase(ConceptType.String)]
    public async Task Given_ConceptVersionInputModel_When_AddIopConceptVersion_Then_added(ConceptType conceptType)
    {
        // Arrange
        var concept = ModelsHelper.IopConceptInputModels[conceptType];

        var claims = new Claim[]
        {
            new(IopClaimsHelper.ClaimTypes.RoleClaimType, IopClaimsHelper.Roles.BusinessRoles.LocalDataSteward),
            new(IopClaimsHelper.ClaimTypes.AgenciesClaimType, concept.Publisher.Identifier)
        };

        var userContextService = TestHelper.CreateFakeUserContextService(claims);
        var fakeService = CreateFakeService(_dbContext, userContextService);

        var conceptId = await fakeService.AddIopConcept(concept, default);

        if (conceptType == ConceptType.CodeList)
        {
            await fakeService.AddCodeListEntries(conceptId, [ModelsHelper.CodeListEntryInputModelWithAnnotation], default);
        }       

        var conceptVersion = new IopConceptInputModel()
        {
            CodeListEntryValueMaxLength = concept.CodeListEntryValueMaxLength,
            CodeListEntryValueType = concept.CodeListEntryValueType,
            ConceptType = concept.ConceptType,
            ConformsTo = concept.ConformsTo,
            Description = concept.Description with
            {
                De = "Description v2"
            },
            Identifiers = [.. concept.Identifiers],
            Keywords = concept.Keywords,
            MaxLength = concept.MaxLength,
            MaxValue = concept.MaxValue,
            MinLength = concept.MinLength,
            MinValue = concept.MinValue,
            Name = concept.Name with
            {
                De = "Name v2"
            },
            NumberDecimals = concept.NumberDecimals,
            Pattern = concept.Pattern,
            Publisher = concept.Publisher,
            ResponsibleDeputy = concept.ResponsibleDeputy,
            ResponsiblePerson = concept.ResponsiblePerson,
            Themes = concept.Themes,
            ValidFrom = concept.ValidFrom,
            ValidTo = concept.ValidTo,
            Version = "v2",
        };

        // Act
        Guid? id = null;
        var action = () => id = fakeService.AddIopConceptVersion(conceptId, conceptVersion, default).GetAwaiter().GetResult();

        // Assert
        using var _ = new AssertionScope();
        action.Should().NotThrow();
        _dbContext.IopConcepts.Count().Should().Be(2);

        if (conceptType == ConceptType.CodeList)
        {
            _dbContext.CodeListEntries.Count().Should().Be(2);
            _dbContext.Annotations.Count().Should().Be(2); 
        }

        var iopConcept = _dbContext.IopConcepts
            .Include(x => x.Description)
            .Include(x => x.Name)
            .Single(i => i.Id == id);

        //modified values
        iopConcept.Description.De.Should().BeEquivalentTo("Description v2");
        iopConcept.Name.De.Should().BeEquivalentTo("Name v2");
        iopConcept.Version.Should().BeEquivalentTo("v2");

        //existing values
        iopConcept.Description.En.Should().BeEquivalentTo(concept.Description.En);
        iopConcept.Description.Fr.Should().BeEquivalentTo(concept.Description.Fr);
        iopConcept.Description.It.Should().BeEquivalentTo(concept.Description.It);
        iopConcept.Description.Rm.Should().BeEquivalentTo(concept.Description.Rm);

        iopConcept.Name.En.Should().BeEquivalentTo(concept.Name.En);
        iopConcept.Name.Fr.Should().BeEquivalentTo(concept.Name.Fr);
        iopConcept.Name.It.Should().BeEquivalentTo(concept.Name.It);
        iopConcept.Name.Rm.Should().BeEquivalentTo(concept.Name.Rm);
    }

    private static IopConceptsService CreateFakeService(
        IopDbContext dbContext,
        IUserContextService userContextService)
    {
        var registrationStatusPolicyService = Substitute.For<IRegistrationStatusPolicyService>();

        var publicationLevelPolicyService = Substitute.For<IPublicationLevelPolicyService>();

        var publishableEntityAuthorizationService = new PublishableEntityAuthorizationService(
            userContextService,
            publicationLevelPolicyService,
            registrationStatusPolicyService);

        var vocabulariesService = Substitute.For<IVocabulariesService>();

        var agentsService = new AgentsService(
            dbContext, 
            vocabulariesService,
            publishableEntityAuthorizationService, 
            userContextService,
            new InlineValidator<AgentInputModel>());

        var personsService = new IopPersonsService(dbContext, publishableEntityAuthorizationService, userContextService);       

        return new(
            dbContext,
            Substitute.For<ICodeListEntryIndexService>(),
            agentsService,
            personsService,
            userContextService,
            Substitute.For<IVocabulariesService>(),
            Substitute.For<IIopConceptsValidationService>(),
            publicationLevelPolicyService,
            registrationStatusPolicyService,
            publishableEntityAuthorizationService,
            Substitute.For<ICatalogIndexService>(),
            Substitute.For<IIdentifierGenerator>());
    }
}