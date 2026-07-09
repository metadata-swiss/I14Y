using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Common.Extensions;

namespace Bfs.Iop.Core.Common.Exceptions;

[Serializable]
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
        allowActionMessageCode.EnsureValueIsValid();
        AllowActionMessageCode = allowActionMessageCode;
    }

    public AllowActionMessageCode AllowActionMessageCode { get; }
}