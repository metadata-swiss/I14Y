using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Partner.Models.ConceptsInput;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bfs.Iop.Partner.Json;

// In order to accept the discriminator property in any order in the body model,
// a JsonConverter must be implemented to handle the polymorphic serialization.
public sealed class JsonConceptInputConverter : JsonConverter<ConceptInputBase>
{
    private const string _discriminatorName = "conceptType";

    private static readonly JsonEncodedText _discriminatorProperty =
        JsonEncodedText.Encode(_discriminatorName);

    public override ConceptInputBase? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Utf8JsonReader typeReader = reader;

        if (typeReader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException($"Expected StartObject token, got '{typeReader.TokenType}'.");
        }

        while (typeReader.Read())
        {
            if (typeReader.TokenType == JsonTokenType.PropertyName &&
                typeReader.ValueTextEquals(_discriminatorProperty.EncodedUtf8Bytes))
            {
                typeReader.Read();

                if (typeReader.TokenType != JsonTokenType.String)
                {
                    throw new JsonException($"Expected string discriminator value, got '{typeReader.TokenType}'.");
                }

                var discriminator = typeReader.GetString();

                if (!Enum.TryParse(discriminator, out ConceptType conceptType))
                {
                    throw new JsonException($"Expected defined discriminator value, got '{discriminator}'.");
                }

                var type = GetDiscriminatorType(conceptType);

                return (ConceptInputBase)JsonSerializer.Deserialize(ref reader, type, options)!;
            }
            else if (typeReader.TokenType == JsonTokenType.StartObject || typeReader.TokenType == JsonTokenType.StartArray)
            {
                if (!typeReader.TrySkip())
                {
                    typeReader.Skip();
                }
            }
        }

        throw new JsonException($"Object has no discriminator '{_discriminatorName}' property.");
    }

    public override void Write(Utf8JsonWriter writer, ConceptInputBase value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        var (discriminator, document) = value switch
        {
            CodeListConceptInput codeList => (nameof(ConceptType.CodeList), JsonSerializer.SerializeToDocument(codeList, options)),
            DateConceptInput date => (nameof(ConceptType.Date), JsonSerializer.SerializeToDocument(date, options)),
            NumericConceptInput numeric => (nameof(ConceptType.Numeric), JsonSerializer.SerializeToDocument(numeric, options)),
            StringConceptInput strConcept => (nameof(ConceptType.String), JsonSerializer.SerializeToDocument(strConcept, options)),
            _ => throw new NotSupportedException($"The type '{value.GetType().Name}' is not supported.")
        };

        writer.WriteString(_discriminatorProperty, discriminator);

        foreach (var property in document.RootElement.EnumerateObject())
        {
            property.WriteTo(writer);
        }

        writer.WriteEndObject();
    }

    private static Type GetDiscriminatorType(ConceptType conceptType) =>
        conceptType switch
        {
            ConceptType.CodeList => typeof(CodeListConceptInput),
            ConceptType.Date => typeof(DateConceptInput),
            ConceptType.Numeric => typeof(NumericConceptInput),
            ConceptType.String => typeof(StringConceptInput),
            _ => throw new NotSupportedException($"The value '{conceptType}' is not supported.")
        };
}
