namespace Bfs.Iop.Core.Mappings;

/// 
internal static class IndexSearchEnumCrossing
{
    public static TTarget? Cross<TSource, TTarget>(TSource source)
        where TSource : struct, Enum
        where TTarget : struct, Enum
    {
        if (!Enum.IsDefined(source))
        {
            return null;
        }

        return Enum.TryParse<TTarget>(source.ToString(), ignoreCase: false, out var parsed)
            && Enum.IsDefined(parsed)
                ? parsed
                : null;
    }

    public static TTarget? Cross<TSource, TTarget>(TSource? source)
        where TSource : struct, Enum
        where TTarget : struct, Enum =>
        source.HasValue ? Cross<TSource, TTarget>(source.Value) : null;
}
