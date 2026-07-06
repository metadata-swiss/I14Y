using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bfs.Iop.Core.Common.Api.Attributes;

public sealed class OkAttribute : ProducesResponseTypeAttribute
{
    public OkAttribute() : base(StatusCodes.Status200OK)
    { }

    public OkAttribute(Type responseType) : base(responseType, StatusCodes.Status200OK)
    { }
}