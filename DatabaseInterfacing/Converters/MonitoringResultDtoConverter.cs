using System.Text.Json;
using System.Text.Json.Serialization;
using DatabaseInterfacing.Domain.DTOs;

namespace DatabaseInterfacing.Converters
{
    public class MonitoringResultDtoConverter : JsonConverter<MonitoringResultDto>
    {
        public override MonitoringResultDto Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            var name = "";
            List<Dictionary<string, int>> readings = null;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    if (readings == null)
                    {
                        throw new JsonException("Missing 'readings' property.");
                    }

                    var consolidatedReadings = new Dictionary<string, int>();
                    foreach (var kvp in readings.SelectMany(dict => dict))
                    {
                        consolidatedReadings[kvp.Key] = kvp.Value;
                    }

                    var readingDto = new ReadingDto
                    {
                        Measurements = consolidatedReadings.ToDictionary(
                            kv => kv.Key,
                            kv => JsonDocument.Parse(JsonSerializer.Serialize(kv.Value)).RootElement)
                    };

                    return new MonitoringResultDto(name, new List<ReadingDto> { readingDto });
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }

                var propertyName = reader.GetString();

                switch (propertyName)
                {
                    case "name":
                        reader.Read();
                        name = reader.GetString() ?? throw new InvalidOperationException();
                        break;
                    case "readings":
                        reader.Read();
                        readings = JsonSerializer.Deserialize<List<Dictionary<string, int>>>(ref reader, options) ?? throw new InvalidOperationException();
                        break;
                    default:
                        throw new JsonException($"Unknown property: {propertyName}");
                }
            }

            throw new JsonException();
        }

        //No need for implementation, required for the interface implementation, however
        public override void Write(Utf8JsonWriter writer, MonitoringResultDto value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
