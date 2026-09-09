namespace Bfs.Iop.DataAccess.Abstractions.Exceptions;

public sealed class NotFoundException : Exception, IAllowActionInfoException
{
    public AllowActionMessageCode AllowActionMessageCode => AllowActionMessageCode.ResourceNotFound;

    public NotFoundException(string message)
        : base(message)
    { }

    public NotFoundException(string message, Exception inner)
        : base(message, inner)
    { }
}