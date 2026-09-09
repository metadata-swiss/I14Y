namespace Bfs.Iop.DataAccess.Abstractions.Exceptions;

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