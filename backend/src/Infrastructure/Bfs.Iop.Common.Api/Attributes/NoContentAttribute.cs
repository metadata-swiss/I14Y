using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bfs.Iop.Common.Api.Attributes;

public sealed class NoContentAttribute : ProducesResponseTypeAttribute
{
    public NoContentAttribute() : base(StatusCodes.Status204NoContent)
    { }
}