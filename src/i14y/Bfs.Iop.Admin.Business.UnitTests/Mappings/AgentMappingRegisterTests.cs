using AwesomeAssertions;
using Bfs.Iop.Admin.Business.Mappings;
using MapsterMapper;
using NUnit.Framework;

namespace Bfs.Iop.Admin.Business.UnitTests.Mappings;

[TestFixture(TestOf = typeof(AgentMappingRegister))]
public class AgentMappingRegisterTests
{
    private IMapper _mapper = null!;

    [Test]
    public void Map_Agent_DcatToIopAdminModel_Ok()
    {
        // Arrange
        var source = TestData.Core.AgentModel;

        // Act
        var result = _mapper.Map<Models.Agent>(source);

        // Assert
        result.Id.Should().Be(source.Id);
        result.Name.De.Should().Be(source.Name.De);
    }

    [SetUp]
    public void Setup() => _mapper = TestHelper.CreateMapper();
}