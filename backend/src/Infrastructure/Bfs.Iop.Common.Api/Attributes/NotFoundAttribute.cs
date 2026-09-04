using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bfs.Iop.Common.Api.Attributes;

public sealed class NotFoundAttribute : ProducesResponseTypeAttribute
{
    public NotFoundAttribute() : base(StatusCodes.Status404NotFound)
    { }
}