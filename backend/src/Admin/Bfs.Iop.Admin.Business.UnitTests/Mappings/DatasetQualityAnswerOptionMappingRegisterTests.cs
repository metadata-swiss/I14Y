using AwesomeAssertions;
using Bfs.Iop.Admin.Business.Mappings;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.DataAccess.Abstractions;
using MapsterMapper;
using NUnit.Framework;

namespace Bfs.Iop.Admin.Business.UnitTests.Mappings;

[TestFixture(TestOf = typeof(DatasetQualityAnswerOptionMappingRegister))]
public class DatasetQualityAnswerOptionMappingRegisterTests
{
    private IMapper _mapper = null!;

    [Test]
    public void Map_QualityAnswer_EntityToModel_Ok()
    {
        var answerDcat = TestData.Core.AnswerOptionModel;
        var answerModel = TestData.Model.AnswerOption;

        var mappedAnswerModel = _mapper.Map<DatasetQualityAnswerOption>(answerDcat);
        mappedAnswerModel.Should().BeEquivalentTo(answerModel);
    }

    [Test]
    public void Map_QualityAnswer_ModelToDcat_Ok()
    {
        var answerDcat = TestData.Core.AnswerOptionModel;
        var answerModel = TestData.Model.AnswerOption;

        var mappedAnswerDcat = _mapper.Map<DatasetQualityAnswerOptionModel>(answerModel);
        mappedAnswerDcat.Should().BeEquivalentTo(answerDcat);
    }

    [SetUp]
    public void Setup() => _mapper = TestHelper.CreateMapper();
}