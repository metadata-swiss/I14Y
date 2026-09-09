namespace Bfs.Iop.DataAccess.Abstractions.Exceptions;

public interface IAllowActionInfoException
{
    AllowActionMessageCode AllowActionMessageCode { get; }

    string Message { get; }
}
