using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bfs.Iop.Common.Api.Attributes;

public sealed class CreatedAttribute : ProducesResponseTypeAttribute
{
    public CreatedAttribute() : base(StatusCodes.Status201Created)
    { }
}