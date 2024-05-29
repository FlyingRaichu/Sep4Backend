namespace DatabaseInterfacing.Domain.DTOs;

public class OutputRequestDto
{
    public string RequestType { get; set; }
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