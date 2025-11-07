using System.Text.Json;
using System.Text.Json.Serialization;

namespace TodoList.Api.Converters;

/// <summary>
/// Custom JSON converter for DateTime to format as dd/MM/yyyy HH:mm:ss
/// </summary>
public class CustomDateTimeConverter : JsonConverter<DateTime>
{
    private const string DateFormat = "dd/MM/yyyy HH:mm:ss";

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dateString = reader.GetString();
        if (string.IsNullOrEmpty(dateString))
        {
            throw new JsonException("Invalid date format");
        }

        if (DateTime.TryParseExact(dateString, DateFormat, null, System.Globalization.DateTimeStyles.None, out var date))
        {
            return date;
        }

        // Fallback to default parsing
        if (DateTime.TryParse(dateString, out var fallbackDate))
        {
            return fallbackDate;
        }

        throw new JsonException($"Unable to parse '{dateString}' as DateTime");
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(DateFormat));
    }
}