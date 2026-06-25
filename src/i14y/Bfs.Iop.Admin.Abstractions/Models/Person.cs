using System;
using System.Text.Json.Serialization;

namespace Bfs.Iop.Admin.Models;

public class Person
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Guid Id { get; set; }

    public string Identifier { get; set; } = null!;

    public string? Name { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }
}