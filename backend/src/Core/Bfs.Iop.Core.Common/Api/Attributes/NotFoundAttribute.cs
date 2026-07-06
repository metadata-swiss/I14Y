using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bfs.Iop.Core.Common.Api.Attributes;

public sealed class NotFoundAttribute : ProducesResponseTypeAttribute
{
    public NotFoundAttribute() : base(StatusCodes.Status404NotFound)
    { }
}