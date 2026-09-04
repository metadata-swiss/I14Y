using Bfs.Iop.Common.Serialization.Json;

namespace Bfs.Iop.Common.Extensions;

public static class DataWrapperExtensions
{
    public static DataWrapper<T> Wrap<T>(this T data) where T : class
    {
        ArgumentNullException.ThrowIfNull(data, nameof(data));

        return new DataWrapper<T>(data);
    }
}
