using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Common.Exceptions;
using Bfs.Iop.Core.Common.Extensions;
using Bfs.Iop.Core.Common.Utilities;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Bfs.Iop.Core.Common.Serialization.Json;

public sealed class IopJsonSerializer
{
    public const string ContentType = "application/json";

    public static ExportFile SerializeToFile<T>(string fileName, T data) where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName, nameof(fileName));
        ArgumentNullException.ThrowIfNull(data, nameof(data));

        var wrappedContent = data.Wrap();

        var content = JsonSerializer.Serialize(wrappedContent, GetDefaultOptions(ignoreGuidType: true));
        var bytes = Encoding.UTF8.GetBytes(content);

        return new ExportFile(new MemoryStream(bytes), $"{fileName}.json", "application/json");
    }

    public static T DeserializeStreamData<T>(
        Stream data,
        bool setRequiredPropertiesToDefaultValueIfNull = false)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(data, nameof(data));

        var wrappedEntries = JsonSerializer.Deserialize<DataWrapper<T>>(
            data,
            GetDefaultOptions(
                ignoreGuidType: false,
                setRequiredPropertiesToDefaultValueIfNull: setRequiredPropertiesToDefaultValueIfNull)) ??
            throw new BadRequestException("The content of the file could not be read.");

        return wrappedEntries.Data;
    }

    private static JsonSerializerOptions GetDefaultOptions(
        bool ignoreGuidType,
        bool setRequiredPropertiesToDefaultValueIfNull = false)
    {
        var options = new JsonSerializerOptions()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        if (ignoreGuidType || setRequiredPropertiesToDefaultValueIfNull)
        {
            options.TypeInfoResolver = new IopJsonTypeInfoResolver(
                ignoreGuidType,
                setRequiredPropertiesToDefaultValueIfNull);
        }

        options.Converters.Add(new JsonStringEnumConverter());

        return options;
    }

    private sealed class IopJsonTypeInfoResolver(
        bool ignoreGuidType,
        bool setRequiredPropertiesToDefaultValueIfNull) : DefaultJsonTypeInfoResolver
    {
        public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
        {
            JsonTypeInfo typeInfo = base.GetTypeInfo(type, options);

            if (typeInfo.Kind != JsonTypeInfoKind.Object)
            {
                return typeInfo;
            }

            if (ignoreGuidType)
            {
                JsonPropertyInfo[] properties = typeInfo.Properties
                    .Where(p => p.PropertyType == typeof(Guid))
                    .ToArray();

                foreach (JsonPropertyInfo property in properties)
                {
                    typeInfo.Properties.Remove(property);
                }
            }

            if (setRequiredPropertiesToDefaultValueIfNull)
            {
                foreach (JsonPropertyInfo property in typeInfo.Properties)
                {
                    property.IsRequired = false;
                }
            }

            return typeInfo;
        }
    }
}