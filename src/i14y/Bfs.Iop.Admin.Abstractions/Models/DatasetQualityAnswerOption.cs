using System;

namespace Bfs.Iop.Admin.Models;

public class DatasetQualityAnswerOption
{
    public MultiLanguage? Detail { get; set; } = null!;

    public bool DetailMandatory { get; set; }

    public Guid Id { get; set; }

    public MultiLanguage Name { get; set; } = null!;

    public string Value { get; set; } = null!;
}