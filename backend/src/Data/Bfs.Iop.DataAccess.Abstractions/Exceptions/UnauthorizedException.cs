namespace Bfs.Iop.DataAccess.Abstractions.Exceptions;

public sealed class UnauthorizedException : Exception, IAllowActionInfoException
{
    public AllowActionMessageCode AllowActionMessageCode => AllowActionMessageCode.NoValidToken;

    public UnauthorizedException(string message)
        : base(message)
    { }

    public UnauthorizedException(string message, Exception inner)
        : base(message, inner)
    { }
}