using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bfs.Iop.Admin.Api.Extensions;

internal static class JsonSerializerExtensions
{
    private static readonly JsonSerializerOptions DefaultOptions = CreateDefaultOptions();

    public static T DeserializeFromStream<T>(this Stream data)
    {
        return JsonSerializer.Deserialize<T>(data, DefaultOptions)
            ?? throw new InvalidOperationException("Cannot deserialize source data!");
    }

    public static void SerializeToStream<T>(this Stream stream, T data)
    {
        JsonSerializer.Serialize(stream, data, DefaultOptions);
        stream.Position = 0;
    }

    private static JsonSerializerOptions CreateDefaultOptions()
    {
        var options = new JsonSerializerOptions()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        options.Converters.Add(new JsonStringEnumConverter());

        return options;
    }
}
