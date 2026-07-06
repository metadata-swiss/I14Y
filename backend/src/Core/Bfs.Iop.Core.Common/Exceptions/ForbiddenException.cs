using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Core.Common.Exceptions;

[Serializable]
public sealed class ForbiddenException : Exception, IAllowActionInfoException
{
    public AllowActionMessageCode AllowActionMessageCode => AllowActionMessageCode.UserHasNotEnoughRights;

    public ForbiddenException(string message)
        : base(message)
    { }

    public ForbiddenException(string message, Exception inner)
        : base(message, inner)
    { }
}