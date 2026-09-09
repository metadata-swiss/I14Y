namespace Bfs.Iop.DataAccess.Abstractions.Exceptions;

public sealed class MethodNotAllowedException : Exception, IAllowActionInfoException
{
    public AllowActionMessageCode AllowActionMessageCode { get; }

    public MethodNotAllowedException(string message)
    : base(message)
    { }

    public MethodNotAllowedException(string message, AllowActionMessageCode allowActionMessageCode)
        : base(message)
    {
        AllowActionMessageCode = allowActionMessageCode;
    }

    public MethodNotAllowedException(string message, Exception inner)
        : base(message, inner)
    { }
}
