namespace Bfs.Iop.Common.Extensions;

public static class DictionaryExtensions
{
    public static TValue GetValueOrDefault<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary,
        TKey key,
        TValue defaultValue = default!)
    {
        ArgumentNullException.ThrowIfNull(dictionary, nameof(dictionary));

        return dictionary.TryGetValue(key, out var value) ? value : defaultValue;
    }
}
