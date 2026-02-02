using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Reflection;

namespace VNVTStore.Domain.Common;

public class SmartEnumJsonConverter<TEnum> : JsonConverter<TEnum>
    where TEnum : SmartEnum<TEnum>
{
    public override TEnum? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var name = reader.GetString();
            return SmartEnum<TEnum>.FromName(name!) ?? throw new JsonException($"Invalid value '{name}' for {typeof(TEnum).Name}");
        }

        if (reader.TokenType == JsonTokenType.Number)
        {
            var value = reader.GetInt32();
            return SmartEnum<TEnum>.FromValue(value) ?? throw new JsonException($"Invalid value '{value}' for {typeof(TEnum).Name}");
        }

        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        throw new JsonException($"Unexpected token {reader.TokenType} when parsing {typeof(TEnum).Name}");
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
        }
        else
        {
            writer.WriteStringValue(value.Name);
        }
    }
}
