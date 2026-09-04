using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Abstractions.Exceptions;
using Bfs.Iop.DataAccess.Relational.Authorization;
using Bfs.Iop.DataAccess.Relational.Entities;
using Bfs.Iop.DataAccess.Relational.Services;
using Bfs.Iop.DataAccess.Relational.UnitTests.Helpers;
using Bfs.Iop.Infrastructure.Security.Helpers;
using Bfs.Iop.Infrastructure.Security.Services;
using NSubstitute;

namespace Bfs.Iop.DataAccess.Relational.UnitTests.Services;

[TestFixture(TestOf = typeof(IopPersonsService))]
public class IopPersonsServiceTests
{
    private IopDbContext _dbContext = null!;
    private IUserContextService _userContextService = null!;
    private IopPersonsService _iopPersonService = null!;

    [SetUp]
    public void SetUp()
    {
        _dbContext = TestHelper.CreateFakeIopDbContext();
        _userContextService = Substitute.For<IUserContextService>();
        _iopPersonService = new IopPersonsService(_dbContext, Substitute.For<IEntityAuthorizationService>(), _userContextService);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [Test]
    public async Task AddIopPerson_Should_Throw_If_Email_Already_Exists()
    {
        // Arrange
        var iopPersonModel = new IopPersonModel()
        {
            Email = "test@example.com",
            FamilyName = "Doe",
            GivenName = "Joe"
        };

        var existingPerson = new IopPerson
        {
            Email = "test@example.com",
            FamilyName = "Doe",
            GivenName = "John",
        };

        _dbContext.IopPersons.Add(existingPerson);
        await _dbContext.SaveChangesAsync();

        // Act
        var action = async () => await _iopPersonService.AddIopPerson(iopPersonModel, CancellationToken.None);

        // Assert
        await action.Should().ThrowExactlyAsync<ConflictException>();
    }

    [Test]
    public void AddOrUpdateCurrentIopPerson_Should_Throw_If_User_Does_Not_Have_Valid_Role()
    {
        // Arrange
        _userContextService.UserHasRole(Arg.Any<string>()).Returns(false);
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        Assert.ThrowsAsync<ForbiddenException>(async () => await _iopPersonService.AddOrUpdateCurrentIopPerson(cancellationToken));
    }

    [Test]
    public void AddOrUpdateCurrentIopPerson_Should_Return_If_Claims_Are_Missing()
    {
        // Arrange
        _userContextService.UserHasRole(IopClaimsHelper.Roles.General.Allow).Returns(true);
        _userContextService.TryGetUserClaimValue(Arg.Any<string>()).Returns((string)null);
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        Assert.ThrowsAsync<ForbiddenException>(async () => await _iopPersonService.AddOrUpdateCurrentIopPerson(cancellationToken));
    }

    [Test]
    public async Task AddOrUpdateCurrentIopPerson_Should_Add_New_Person_If_Not_Found()
    {
        // Arrange
        var originalCount = _dbContext.IopPersons.Count();

        _userContextService.UserHasRole(IopClaimsHelper.Roles.General.Allow).Returns(true);
        _userContextService.TryGetUserClaimValue(IopClaimsHelper.ClaimTypes.FirstNameClaimType).Returns("John");
        _userContextService.TryGetUserClaimValue(IopClaimsHelper.ClaimTypes.LastNameClaimType).Returns("Doe");
        _userContextService.TryGetUserClaimValue(IopClaimsHelper.ClaimTypes.EmailClaimType).Returns("test@example.com");

        var cancellationToken = CancellationToken.None;

        // Assert
        await _iopPersonService.AddOrUpdateCurrentIopPerson(cancellationToken);

        // Act
        using var _ = new AssertionScope();
        _dbContext.IopPersons.Count().Should().Be(originalCount + 1);
        var person = _dbContext.IopPersons.Skip(originalCount).Take(1).SingleOrDefault();
        person.Should().NotBeNull();
        person!.GivenName.Should().Be("John");
        person.FamilyName.Should().Be("Doe");
        person.Email.Should().Be("test@example.com");
    }

    [Test]
    public async Task AddOrUpdateCurrentIopPerson_Should_Update_LastLoginDate_If_Person_Found()
    {
        // Arrange
        _userContextService.UserHasRole(IopClaimsHelper.Roles.General.Allow).Returns(true);
        _userContextService.TryGetUserClaimValue(IopClaimsHelper.ClaimTypes.FirstNameClaimType).Returns("John");
        _userContextService.TryGetUserClaimValue(IopClaimsHelper.ClaimTypes.LastNameClaimType).Returns("Doe");
        _userContextService.TryGetUserClaimValue(IopClaimsHelper.ClaimTypes.EmailClaimType).Returns("test@example.com");

        var now = DateOnly.FromDateTime(DateTime.Now);

        var cancellationToken = CancellationToken.None;

        var existingPerson = new IopPerson
        {
            Email = "test@example.com",
            FamilyName = "Doe",
            GivenName = "John",
            LastLoginDate = now.AddDays(-1)
        };

        _dbContext.IopPersons.Add(existingPerson);
        await _dbContext.SaveChangesAsync();

        // Act
        await _iopPersonService.AddOrUpdateCurrentIopPerson(cancellationToken);

        // Assert
        var updatedPerson = _dbContext.IopPersons.Single(x => x.Email == "test@example.com");
        updatedPerson.LastLoginDate.Should().Be(now);
    }

    [TestCase("test@example.com", "test@example.com")]
    [TestCase("test@example.com", "TesT@example.com")]
    public async Task GetByEmail_Should_Return_Mapped_Model_If_Person_Found(
        string email,
        string argumentEmail)
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        var existingPerson = new IopPerson
        {
            Email = email,
            FamilyName = "Doe",
            GivenName = "John",
        };

        _dbContext.IopPersons.Add(existingPerson);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _iopPersonService.GetIopPersonByEmail(argumentEmail, cancellationToken);

        // Assert
        using var _ = new AssertionScope();
        result.Email.Should().Be(existingPerson.Email);
        result.FamilyName.Should().Be(existingPerson.FamilyName);
        result.GivenName.Should().Be(existingPerson.GivenName);
    }

    [TestCase("test@example.com", "test@example.com")]
    [TestCase("test@example.com", "TesT@example.com")]
    public async Task Given_existing_email_When_GetIdByEmail_Then_return_id(
        string email,
        string argumentEmail)
    {
        // Arrange
        var id = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        var existingPerson = new IopPerson
        {
            Email = email,
            Id = id,
            FamilyName = "Doe",
            GivenName = "John",
        };

        _dbContext.IopPersons.Add(existingPerson);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _iopPersonService.GetIopPersonIdByEmail(argumentEmail, cancellationToken);

        // Assert
        result.Should().Be(id);
    }
}
