using System.Text.Json.Serialization;

namespace DatabaseInterfacing.Domain.DTOs;

public class OutputRequestDto
{
    [JsonPropertyName("requestType")]
    public string RequestType { get; set; }
    
    [JsonPropertyName("value")]
    public int Value { get; set; }


    public OutputRequestDto()
    {
        RequestType = "waterFlowCorrection";
        Value = 0;
    }

    public OutputRequestDto(string requestType, int value)
    {
        RequestType = requestType;
        Value = value;
    }
}