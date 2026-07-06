using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bfs.Iop.Core.Common.Api.Attributes;

public sealed class ConflictAttribute : ProducesResponseTypeAttribute
{
    public ConflictAttribute() : base(StatusCodes.Status409Conflict)
    { }
}