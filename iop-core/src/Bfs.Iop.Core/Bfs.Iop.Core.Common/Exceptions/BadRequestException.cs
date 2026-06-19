namespace Bfs.Iop.Core.Common.Exceptions;

[Serializable]
public sealed class BadRequestException : Exception
{
    public BadRequestException(string message)
        : base(message)
    { }

    public BadRequestException(string message, Exception inner)
        : base(message, inner)
    { }
}