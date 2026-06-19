using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Core.Common.Exceptions;

public interface IAllowActionInfoException
{
    AllowActionMessageCode AllowActionMessageCode { get; }

    string Message { get; }
}
