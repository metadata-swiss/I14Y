using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Common.Extensions;
using System.Net;

namespace Bfs.Iop.Core.Common.Exceptions;

[Serializable]
public sealed class UnauthorizedException : Exception, IAllowActionInfoException
{
    public AllowActionMessageCode AllowActionMessageCode => AllowActionMessageCode.NoValidToken;

    public HttpStatusCode HttpStatusCode => HttpStatusCode.Unauthorized;

    public UnauthorizedException(string message)
        : base(message)
    { }

    public UnauthorizedException(string message, Exception inner)
        : base(message, inner)
    { }
}