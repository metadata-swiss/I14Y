using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bfs.Iop.Common.Api.Attributes;

public sealed class ForbiddenAttribute : ProducesResponseTypeAttribute
{
    public ForbiddenAttribute() : base(StatusCodes.Status403Forbidden)
    { }
}