using AutoMapper;
using AwesomeAssertions;
using Bfs.Iop.Core.Abstractions.Models;
using NUnit.Framework;

namespace Bfs.Iop.Admin.Business.UnitTests.Mappings;

public class ResourceMappingProfilesTests
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
    public void Setup()
    {
        _mapper = TestHelper.GetFullMapperConfiguration().CreateMapper();
    }
}