using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bfs.Iop.Core.Common.Api.Attributes;

public sealed class UnauthorizedAttribute : ProducesResponseTypeAttribute
{
    public UnauthorizedAttribute() : base(StatusCodes.Status401Unauthorized)
    { }
}