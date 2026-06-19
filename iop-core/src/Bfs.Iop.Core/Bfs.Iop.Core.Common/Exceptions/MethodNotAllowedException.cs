using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Common.Extensions;

namespace Bfs.Iop.Core.Common.Exceptions;

[Serializable]
public sealed class MethodNotAllowedException : Exception, IAllowActionInfoException
{
    public AllowActionMessageCode AllowActionMessageCode { get; }

    public MethodNotAllowedException(string message)
    : base(message)
    { }

    public MethodNotAllowedException(string message, AllowActionMessageCode allowActionMessageCode)
        : base(message)
    {
        allowActionMessageCode.EnsureValueIsValid();
        AllowActionMessageCode = allowActionMessageCode;
    }

    public MethodNotAllowedException(string message, Exception inner)
        : base(message, inner)
    { }
}
