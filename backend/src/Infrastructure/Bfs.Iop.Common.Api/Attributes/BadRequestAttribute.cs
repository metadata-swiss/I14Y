using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bfs.Iop.Common.Api.Attributes;

public sealed class BadRequestAttribute : ProducesResponseTypeAttribute
{
    public BadRequestAttribute() : base(StatusCodes.Status400BadRequest)
    { }
}