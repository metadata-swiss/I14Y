using AwesomeAssertions;
using Bfs.Iop.Admin.Business.Mappings;
using Bfs.Iop.DataAccess.Abstractions;
using MapsterMapper;
using NUnit.Framework;

namespace Bfs.Iop.Admin.Business.UnitTests.Mappings;

[TestFixture(TestOf = typeof(ResourceMappingRegister))]
public class ResourceMappingRegisterTests
{
    private IMapper _mapper = null!;

    [Test]
    public void Map_PeriodOfTime_IopAdminModelToDcat_Ok()
    {
        // Arrange
        var source = TestData.Model.ConformTos;

        // Act
        var result = _mapper.Map<ResourceModel>(source);

        // Assert
        result.Uri.Should().Be(source.Href);
        result.Label.De.Should().Be(source.Label.De);
    }

    [SetUp]
    public void Setup() => _mapper = TestHelper.CreateMapper();
}