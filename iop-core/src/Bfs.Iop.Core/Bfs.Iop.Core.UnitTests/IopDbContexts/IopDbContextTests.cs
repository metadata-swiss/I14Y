using AwesomeAssertions;
using Bfs.Iop.Core.Data;
using Bfs.Iop.Core.Data.Entities;
using Bfs.Iop.Core.UnitTests.Helpers;
using Microsoft.EntityFrameworkCore;


namespace Bfs.Iop.Core.UnitTests.IopDbContexts;

[TestFixture]
public class IopDbContextTests
{
    private IopDbContext _dbContext;
    private IopConcept _conceptForCodeList;

    public enum PersonRole
    {
        ResponsiblePerson = 1,
        DeputyPerson = 2,
        ResponsiblePersonAndDeputyPerson = ResponsiblePerson | DeputyPerson
    }

    [SetUp]
    public void Setup()
    {
        _dbContext = TestHelper.CreateFakeIopDbContext();

        _conceptForCodeList = new IopConcept
        {
            Id = Guid.NewGuid(),
            Identifiers = ["concept-001"],
            Version = "1.0"
        };

        _dbContext.IopConcepts.Add(_conceptForCodeList);
        _dbContext.SaveChanges();

        var codeListEntry = new CodeListEntry
        {
            Id = Guid.NewGuid(),
            Code = "AgentType",
            Name = new MultiLanguage { De = "her majesty's agent 007" },
            IopConcept = _conceptForCodeList,
            IopConceptId = _conceptForCodeList.Id,
            Position = 1
        };

        _dbContext.CodeListEntries.Add(codeListEntry);
        _dbContext.SaveChanges();
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [TestCase(PersonRole.ResponsiblePerson)]
    [TestCase(PersonRole.DeputyPerson)]
    [TestCase(PersonRole.ResponsiblePersonAndDeputyPerson)]
    public void ShouldNotDeleteDatasetWhenDeputyForeignKeyIsRemoved(PersonRole removePropertyValue)
    {
        // Arrange 
        var responsiblePerson = CreatePerson("Max", "Mustermann", "max.mustermann@example.com");
        var deputyPerson = CreatePerson("Maria", "Musterfrau", "maria.musterfrau@example.com");
        var publisher = CreatePublisher();
        var dataset = CreateDataset(responsiblePerson, deputyPerson, publisher);

        _dbContext.IopPersons.Add(responsiblePerson);
        _dbContext.IopPersons.Add(deputyPerson);
        _dbContext.Agents.Add(publisher);
        _dbContext.Datasets.Add(dataset);
        _dbContext.SaveChanges();

        // Act
        var tmpDataset = _dbContext.Datasets.Find(dataset.Id);

        switch(removePropertyValue)
        {
            case PersonRole.ResponsiblePerson:
                tmpDataset.ResponsiblePerson = null;
                tmpDataset.ResponsiblePersonId = null;
                break;
            case PersonRole.DeputyPerson:
                tmpDataset.ResponsibleDeputy = null;
                tmpDataset.ResponsibleDeputyId = null;
                break;
            case PersonRole.ResponsiblePersonAndDeputyPerson:
                tmpDataset.ResponsiblePerson = null;
                tmpDataset.ResponsiblePersonId = null;
                tmpDataset.ResponsibleDeputy = null;
                tmpDataset.ResponsibleDeputyId = null;
                break;
        }
       
        _dbContext.Update(tmpDataset);

        _dbContext.SaveChanges();

        // Assert
        _dbContext.Entry(tmpDataset).State.Should().NotBe(EntityState.Deleted,
            "Dataset was incorrectly deleted although it is a protected entity");

        var foundDataset = _dbContext.Datasets.Find(dataset.Id);
        foundDataset.Should().NotBeNull("Dataset could not be found after the operation");
    }

    [Test]
    public void ShouldDeleteOnlyNonProtectedEntitiesWhenForeignKeysAreRemoved()
    {
        // Arrange
        var publisher = CreatePublisher();
        var responsiblePerson = CreatePerson("Max", "Mustermann", "max.mustermann@example.com");
        var deputyPerson = CreatePerson("Maria", "Musterfrau", "maria.musterfrau@example.com");
        var dataset = CreateDataset(responsiblePerson, deputyPerson, publisher);
        var iopConcept = CreateIopConcept();
        var publicService = CreatePublicService();
        var dataService = CreateDataService(publisher);
        var relation = CreateQualifiedRelation(dataset);
        var distribution = CreateDistribution(dataset);

        _dbContext.IopPersons.Add(responsiblePerson);
        _dbContext.IopPersons.Add(deputyPerson);
        _dbContext.Agents.Add(publisher);
        _dbContext.Datasets.Add(dataset);
        _dbContext.IopConcepts.Add(iopConcept);
        _dbContext.PublicServices.Add(publicService);
        _dbContext.DataServices.Add(dataService);
        _dbContext.QualifiedRelations.Add(relation);
        _dbContext.Distributions.Add(distribution);
        _dbContext.SaveChanges();

        // Act 
        var tmpDataset = _dbContext.Datasets.Find(dataset.Id);
        tmpDataset.ResponsibleDeputy = null;
        tmpDataset.ResponsibleDeputyId = null;
        _dbContext.Update(tmpDataset);

        var tmpRelation = _dbContext.QualifiedRelations.Find(relation.Id);
        tmpRelation.Dataset = null;
        tmpRelation.DatasetId = null;
        _dbContext.Update(tmpRelation);

        var tmpDistribution = _dbContext.Distributions.Find(distribution.Id);
        tmpDistribution.Dataset = null;
        tmpDistribution.DatasetId = null;
        _dbContext.Update(tmpDistribution);

        _dbContext.SaveChanges();

        // Assert
        _dbContext.Datasets.Find(dataset.Id).Should().NotBeNull("Dataset was deleted by mistake");
        _dbContext.IopConcepts.Find(iopConcept.Id).Should().NotBeNull("IopConcept was deleted by mistake");
        _dbContext.PublicServices.Find(publicService.Id).Should().NotBeNull("PublicService was deleted by mistake");
        _dbContext.DataServices.Find(dataService.Id).Should().NotBeNull("DataService was deleted by mistake");
        _dbContext.QualifiedRelations.Find(relation.Id).Should().BeNull("QualifiedRelation was not deleted");
        _dbContext.Distributions.Find(distribution.Id).Should().BeNull("Distribution was not deleted");
    }


    Dataset CreateDataset(IopPerson responsiblePerson, IopPerson deputyPerson, Agent publisher)
    {
        return new Dataset
        {
            Id = Guid.NewGuid(),
            Title = new MultiLanguage { De = "Test Dataset" },
            Description = new MultiLanguage { De = "Test Beschreibung" },
            ResponsiblePerson = responsiblePerson,
            ResponsiblePersonId = responsiblePerson.Id,
            ResponsibleDeputy = deputyPerson,
            ResponsibleDeputyId = deputyPerson.Id,
            Publisher = publisher,
            PublisherId = publisher.Id
        };
    }

    private Agent CreatePublisher()
    {
        return new Agent
        {
            Id = Guid.NewGuid(),
            Identifier = "Jeames-Bond-007",
            Name = new MultiLanguage { De = "Test Publisher" },
        };
    }

    private IopPerson CreatePerson(string givenName, string familyName, string email)
    {
        return new IopPerson
        {
            Id = Guid.NewGuid(),
            GivenName = givenName,
            FamilyName = familyName,
            Email = email
        };
    }

    private DataService CreateDataService(Agent publisher)
    {
        return new DataService
        {
            Id = Guid.NewGuid(),
            Title = new MultiLanguage { De = "Test Service" },
            Description = new MultiLanguage { De = "Beschreibung" },
            Publisher = publisher,
            PublisherId = publisher.Id,
            AccessRights = "Public",
            ConformsTo = new List<Resource>(),
            ContactPoint = new List<VCard>(),
            Datasets = new List<DataServiceDataset>(),
            Documentation = new List<Resource>(),
            EndpointDescription = new List<Resource>(),
            EndpointUrl = new List<Resource>(),
            Keyword = new List<Keyword>(),
            LandingPage = new List<Resource>(),
            Theme = new string[] { "Test" }
        };
    }

    private IopConcept CreateIopConcept()
    {
        return new IopConcept
        {
            Id = Guid.NewGuid(),
            Identifiers = ["concept-002"],
            Version = "1.0"
        };
    }

    private QualifiedRelation CreateQualifiedRelation(Dataset dataset)
    {
        return new QualifiedRelation
        {
            Id = Guid.NewGuid(),
            Dataset = dataset,
            DatasetId = dataset.Id,
            HadRole = "TestRole",
            Relation = new Resource { Id = Guid.NewGuid(), Href = "https://example.com" }
        };
    }

    private Distribution CreateDistribution(Dataset dataset)
    {
        return new Distribution
        {
            Id = Guid.NewGuid(),
            Dataset = dataset,
            DatasetId = dataset.Id,
            Title = new MultiLanguage { De = "Test Distribution" },
            Description = new MultiLanguage { De = "Beschreibung" }
        };
    }

    private PublicService CreatePublicService()
    {
        return new PublicService
        {
            Id = Guid.NewGuid(),
            Sector = [],
            ThematicArea =[],
            Identifiers = ["PublicServicet-003"],
        };
    }
}