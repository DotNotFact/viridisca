using System.Text.Json;
using System.Text.Json.Serialization;

namespace Viridisca.Api.Serialization;

/// <summary>
/// PostgreSQL's "timestamp with time zone" columns (the Npgsql default mapping for
/// DateTime) reject any value whose Kind isn't Utc. A JSON date string without a "Z"/offset
/// suffix deserializes as Kind=Unspecified, which then throws deep inside SaveChanges
/// instead of at the API boundary. These converters normalize every incoming DateTime to
/// Utc so a client-omitted offset fails fast and clearly rather than as a 500 from Npgsql.
/// </summary>
public sealed class UtcDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetDateTime();
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
        };
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        => writer.WriteStringValue(DateTime.SpecifyKind(value, DateTimeKind.Utc));
}

public sealed class UtcNullableDateTimeConverter : JsonConverter<DateTime?>
{
    private static readonly UtcDateTimeConverter Inner = new();

    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType == JsonTokenType.Null ? null : Inner.Read(ref reader, typeToConvert, options);

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            Inner.Write(writer, value.Value, options);
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}
