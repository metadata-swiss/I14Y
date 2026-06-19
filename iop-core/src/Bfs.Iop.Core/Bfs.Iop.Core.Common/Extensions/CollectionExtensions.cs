namespace Bfs.Iop.Core.Common.Extensions;

public static class CollectionExtensions
{
    /// <summary>
    /// Performs the specified action on each element <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="values"></param>
    /// <param name="action"></param>
    public static void ForEach<T>(this ICollection<T> values, Action<T> action) where T : class
    {
        ArgumentNullException.ThrowIfNull(values, nameof(values));
        ArgumentNullException.ThrowIfNull(action, nameof(action));

        foreach (var item in values)
        {
            action(item);
        }
    }
}
