using AutoMapper;
using AwesomeAssertions;
using NUnit.Framework;
using System;

namespace Bfs.Iop.Admin.Business.UnitTests.Mappings;

public class MappingProfileValidationTests
{
    [Test]
    public void AutoMapper_Configuration_IsValid()
    {
        // Arrange
        var mappingConfig = TestHelper.GetFullMapperConfiguration();

        // Act
        Action assertConfig = () => mappingConfig.AssertConfigurationIsValid();

        // Assert
        assertConfig.Should().NotThrow<AutoMapperConfigurationException>();
    }
}