using Microsoft.AspNetCore.Mvc;

namespace Bfs.Iop.Common.Api.Attributes;

public sealed class ProducesJsonAttribute : ProducesAttribute
{
    public ProducesJsonAttribute() : base("application/json")
    { }
}