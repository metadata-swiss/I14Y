using System;

namespace Bfs.Iop.Admin.Models;

public class DatasetQualityInformation
{
    public Guid AnswerId { get; set; }

    public string? Detail { get; set; }

    public Guid Id { get; set; }

    public Guid QuestionId { get; set; }
}