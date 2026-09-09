using Bfs.Iop.Common.Serialization.Json;
using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Core.Extensions;

internal static class IopSerializerExtensions
{
    extension(IopJsonSerializer)
    {
        public static ExportFile SerializeToFile<T>(string fileName, T data) where T : class
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(fileName, nameof(fileName));
            ArgumentNullException.ThrowIfNull(data, nameof(data));

            var stream = IopJsonSerializer.Serialize(data);

            return new ExportFile(stream, $"{fileName}.json", IopJsonSerializer.ContentType);
        }
    }
}
