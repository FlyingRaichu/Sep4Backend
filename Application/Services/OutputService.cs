using System.Text.Json;
using Application.ServiceInterfaces;
using DatabaseInterfacing.Domain.DTOs;
using IoTInterfacing.Interfaces;

namespace Application.Services;

public class OutputService : IOutputService
{
    private readonly IConnectionController _connectionController;
    public bool Enabled { get; set; }


    public OutputService(IConnectionController connectionController)
    {
        _connectionController = connectionController;
        Enabled = false;
    }

    public async Task<MonitoringResultDto> AlterPumpAsync(string requestType, int valueInPercent)
    {
        var outputRequest = new OutputRequestDto(requestType, valueInPercent);
        var outPutRequestJson = JsonSerializer.Serialize(outputRequest,  new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        var responseJson = await _connectionController.SendRequestToArduinoAsync(outPutRequestJson);
        Console.WriteLine($"Response in JSON is: {responseJson}");
        
        var responseDto = JsonSerializer.Deserialize<MonitoringResultDto>(responseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (responseDto is null)
        {
            throw new Exception("Arduino returned a null response.");
        }

        return responseDto;
    }
}