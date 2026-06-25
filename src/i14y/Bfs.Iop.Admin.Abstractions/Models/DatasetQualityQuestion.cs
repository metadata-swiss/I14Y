using System;
using System.Collections.Generic;

namespace Bfs.Iop.Admin.Models;

public class DatasetQualityQuestion
{
    public DatasetQualityQuestion()
        => AnswerOptions = new List<DatasetQualityAnswerOption>();

    public IEnumerable<DatasetQualityAnswerOption> AnswerOptions { get; set; }

    public Guid Id { get; set; }

    public bool Mandatory { get; set; }

    public int Order { get; set; }

    public MultiLanguage Question { get; set; } = null!;
}