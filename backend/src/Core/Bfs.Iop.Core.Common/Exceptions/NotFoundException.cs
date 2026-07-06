using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Core.Common.Exceptions;

[Serializable]
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