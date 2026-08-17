using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Core.Common.Exceptions;

public class ConflictException : Exception
{
    public ConflictException(string message)
        : base(message)
    { }

    public ConflictException(string message, Exception inner)
        : base(message, inner)
    { }

    public ConflictException(string message, AllowActionMessageCode resourceReferenced)
        : base(message)
    { }
}
