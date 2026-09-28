namespace Bfs.Iop.Core.Mappings;

/// <summary>
///     Crosses an enum between this solution's types and the generated IndexSearch client's copies.
///     <para>
///         Always by name, and only for values both sides actually declare. The client re-declares
///         every enum it needs with ordinals derived from its own declaration order, and none of them
///         line up: <c>PublicationLevel.Public</c> is 2 here and 1 there,
///         <c>SearchResourceType.Dataset</c> is 1 here and 0 there,
///         <c>SearchStructureOption.WithStructure</c> is 1 here and 0 there. A cast compiles, reads
///         naturally and is wrong — on fields that decide what a caller is allowed to see.
///     </para>
/// </summary>
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
