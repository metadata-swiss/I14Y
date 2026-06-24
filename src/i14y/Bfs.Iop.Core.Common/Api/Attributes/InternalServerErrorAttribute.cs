using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bfs.Iop.Core.Common.Api.Attributes;

public sealed class InternalServerErrorAttribute : ProducesResponseTypeAttribute
{
    public InternalServerErrorAttribute() : base(StatusCodes.Status500InternalServerError)
    { }
}