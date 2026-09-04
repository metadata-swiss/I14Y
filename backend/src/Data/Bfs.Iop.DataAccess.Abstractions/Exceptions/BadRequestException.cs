namespace Bfs.Iop.DataAccess.Abstractions.Exceptions;

public sealed class BadRequestException : Exception, IAllowActionInfoException
{
    public BadRequestException(string message)
        : base(message)
    { }

    public BadRequestException(string message, Exception inner)
        : base(message, inner)
    { }

    public BadRequestException(string message, AllowActionMessageCode allowActionMessageCode)
        : base(message)
    {
        AllowActionMessageCode = allowActionMessageCode;
    }

    public AllowActionMessageCode AllowActionMessageCode { get; }
}