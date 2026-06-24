using Bfs.Iop.Core.Data;
using Bfs.Iop.Core.Services.Contracts;
using Bfs.Iop.Core.Validation.Vocabularies;
using Bfs.Iop.Core.Vocabularies;
using Bfs.Iop.Infrastructure.Security.Helpers;
using Bfs.Iop.Infrastructure.Security.Services;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using System.Security.Claims;

namespace Bfs.Iop.Core.UnitTests.Helpers;

internal static class TestHelper
{
    public static IUserContextService CreateFakeUserContextService(IEnumerable<Claim> claims)
    {
        var provider = Substitute.For<IAuthorizationProvider>();
        var user = Substitute.For<ClaimsPrincipal>();
        user.Identity!.IsAuthenticated.Returns(true);
        user.Claims.Returns(claims);

        provider.GetUser().Returns(user);

        return new UserContextService(provider);
    }

    public static IUserContextService CreateFakeUserContextServiceForInteroperabilityServiceUser()
    {
        var claims = new Claim[]
        {
            new(IopClaimsHelper.ClaimTypes.RoleClaimType, IopClaimsHelper.Roles.BusinessRoles.InteroperabilityService),
            new(IopClaimsHelper.ClaimTypes.AgenciesClaimType, "*")
        };

        return CreateFakeUserContextService(claims);
    }

    public static VocabularyEntryCodeValidator<T> CreateFakeVocabularyEntryCodeValidatorWithoutFailures<T>() where T : IdentifiedVocabularyBase, new()
    {
        var service = Substitute.For<IVocabulariesService>();
        var vocabulary = new T();

        service.TryGetVocabulary<T>(Arg.Any<CancellationToken>()).Returns(vocabulary);
        var validator = Substitute.For<VocabularyEntryCodeValidator<T>>(service);

        validator.Validate(Arg.Any<ValidationContext<string>>()).Returns(new ValidationResult());

        return validator;
    }

    public static IopDbContext CreateFakeIopDbContext(IUserContextService? userContextService = null)
    {
        userContextService ??= CreateFakeUserContextService([]);

        var options = new DbContextOptionsBuilder<IopDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;
        
        var dbContext = new IopDbContext(options, userContextService);

        dbContext.Agents.Add(EntitiesHelper.Agent);

        dbContext.IopPersons.AddRange(
            EntitiesHelper.IopPerson,
            EntitiesHelper.IopPerson2
            );

        dbContext.SaveChanges();

        return dbContext;
    }
}
