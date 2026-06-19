using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bfs.Iop.Core.Common.Api.Attributes;

public sealed class CreatedAttribute : ProducesResponseTypeAttribute
{
    public CreatedAttribute() : base(StatusCodes.Status201Created)
    { }
}