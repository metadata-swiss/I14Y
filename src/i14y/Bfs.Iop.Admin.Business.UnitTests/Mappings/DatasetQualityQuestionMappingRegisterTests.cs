using AwesomeAssertions;
using Bfs.Iop.Admin.Business.Mappings;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.Abstractions.Models;
using MapsterMapper;
using NUnit.Framework;

namespace Bfs.Iop.Admin.Business.UnitTests.Mappings;

[TestFixture(TestOf = typeof(DatasetQualityQuestionMappingRegister))]
public class DatasetQualityQuestionMappingRegisterTests
{
    private IMapper _mapper = null!;

    [Test]
    public void Map_Question_EntityToModel_Ok()
    {
        var questionEntity = TestData.Core.QuestionModel;
        var questionModel = TestData.Model.Question;

        var mappedQuestionModel = _mapper.Map<DatasetQualityQuestion>(questionEntity);
        mappedQuestionModel.Should().BeEquivalentTo(questionModel);
    }

    [Test]
    public void Map_Question_ModelToEntity_Ok()
    {
        var questionEntity = TestData.Core.QuestionModel;
        var questionModel = TestData.Model.Question;

        var mappedQuestionEntity = _mapper.Map<DatasetQualityQuestionModel>(questionModel);
        mappedQuestionEntity.Should().BeEquivalentTo(questionEntity);
    }

    [SetUp]
    public void Setup() => _mapper = TestHelper.CreateMapper();
}