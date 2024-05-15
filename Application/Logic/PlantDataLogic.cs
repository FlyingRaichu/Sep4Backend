using System.Text.Json;
using Application.LogicInterfaces;
using Application.ServiceInterfaces;
using DatabaseInterfacing;
using DatabaseInterfacing.Context;
using DatabaseInterfacing.Domain.DTOs;
using DatabaseInterfacing.Domain.EntityFramework;
using IoTInterfacing.Interfaces;
using IoTInterfacing.Util;
using IoTInterfacing.Implementations;
using IoTInterfacing.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace Application.Logic;

public class PlantDataLogic : IPlantDataLogic
{
    private readonly IConnectionController _connectionController;
    private readonly IThresholdConfigurationService _configurationService;

    public PlantDataLogic(IConnectionController connectionController,
        IThresholdConfigurationService configurationService)
    {
        _connectionController = connectionController;
        _configurationService = configurationService;
    }

    public async Task<IEnumerable<PlantData>> GetAsync(SearchPlantDataDto searchDto)
    {
        await using var dbContext = new PlantDbContext(DatabaseUtils.BuildConnectionOptions());

        var query = dbContext.PlantData.AsQueryable();

        if (!string.IsNullOrEmpty(searchDto.PlantName))
        {
            query = query.Where(
                plant => plant.PlantName.Contains(searchDto.PlantName));
        }

        if (searchDto.WaterTemperature != null)
        {
            query = query.Where(plant => plant.WaterTemperature.Equals(searchDto.WaterTemperature));
        }

        if (searchDto.PHLevel != null)
        {
            query = query.Where(plant => plant.PhLevel.Equals(searchDto.PHLevel));
        }

        if (searchDto.WaterFlow != null)
        {
            query = query.Where(plant => plant.WaterFlow.Equals(searchDto.WaterFlow));
        }

        if (searchDto.WaterEC != null)
        {
            query = query.Where(plant => plant.WaterEC.Equals(searchDto.WaterEC));
        }

        return await query.ToListAsync();
    }
    public async Task<DisplayPlantTemperatureDto?> CheckTemperatureAsync(int id) //Not sure Ids are supposed to be here
    {
        var jsonString =
            await _connectionController
                .SendRequestToArduinoAsync(ApiParameters.DataRequest);
//     var jsonString = @"
// {
//     ""name"" : ""monitoring_results"",
//     ""readings"": [{
//         ""water_conductivity"": 2622,
//         ""water_temperature"" : 23.5
//     }]
// }";


        //Deserialize the JSON string into a MonitoringResultDto object
        var plantData = JsonSerializer.Deserialize<MonitoringResultDto>(jsonString,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        if (plantData == null) throw new Exception("Plant Data object is null or empty.");

        //Calling the object created from the JSON config
        var configuration = await _configurationService.GetConfigurationAsync();

        var status = DetermineTemperatureStatus(plantData.Readings.FirstOrDefault()?.WaterTemperature, configuration);

    Console.WriteLine($"Plant water temp is: {plantData!.Readings.FirstOrDefault()?.WaterTemperature}");
    return new DisplayPlantTemperatureDto(plantData!.Readings.FirstOrDefault()?.WaterTemperature, status);
}
    
    public async Task<DisplayPlantPhDto> GetPhLevelAsync()
    {
        // For Testing: Remove Comments from json, and comment out "string response = ..."
        
        /* var response = @"
        {
            ""name"" : ""monitoring_results"",
            ""readings"": [{
                ""water_conductivity"": 2622,
                ""water_temperature"" : 23.5,
                ""water_ph"" : 6.4
            }]
        }"; */
        
        string response = await _connectionController.SendRequestToArduinoAsync(ApiParameters.DataRequest);
        var plantData = JsonSerializer.Deserialize<MonitoringResultDto>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        if (plantData == null) throw new Exception("Plant Data object is null or empty.");
        float? phLevel = plantData.Readings.FirstOrDefault()?.WaterPhLevel;
        string status = phLevel >= 6.8f || phLevel <= 6.2f ? "Warn" : "Norm";
        return new DisplayPlantPhDto() { Status = status, PhLevel = phLevel };
    }

    public async Task<DisplayPlantECDto?> CheckECAsync(int id)
    {
        var jsonString =
            await _connectionController.SendRequestToArduinoAsync(ApiParameters.DataRequest);


        var plantData = JsonSerializer.Deserialize<MonitoringResultDto>(jsonString,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        if (plantData == null) throw new Exception("Plant Data object is null or empty.");

        var status = plantData?.Readings.FirstOrDefault()?.WaterConductivity switch
        {
            //The thresholds are placeholders, we need to figure out how to feed new placeholders up in this
            >= 50 and <= 75 => "Warn",
            > 75 => "Dang",
            _ => "Norm"
        };

        Console.WriteLine($"Plant water temp is: {plantData!.Readings.FirstOrDefault()?.WaterConductivity}");
        return new DisplayPlantECDto(plantData!.Readings.FirstOrDefault()?.WaterConductivity, status);
    }
    
    public async Task<DisplayPlantWaterFlowDto> CheckWaterFlowAsync()
    {
        var response = @"
        {
            ""name"" : ""monitoring_results"",
            ""readings"": [{
                ""water_conductivity"": 2622,
                ""water_temperature"" : 23.5,
                ""water_ph"" : 6.4,
                ""water_flow"" : 3.3,
            }]
        }";
        var plantData = JsonSerializer.Deserialize<MonitoringResultDto>(response, 
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        if (plantData == null) throw new Exception("Plant Data object is null or empty.");
        
        var status = plantData?.Readings?.FirstOrDefault()?.WaterFlow switch
        {
            //Placeholder
            >= (float) 6.8 or <= (float) 6.2 => "Warn",
            _ => "Norm"
        };
        DisplayPlantWaterFlowDto dto = new DisplayPlantWaterFlowDto()
        {
            Status = status,
            WaterFlow = (float) plantData?.Readings?.FirstOrDefault()?.WaterFlow!
        };
        return dto;
    }

    private static string DetermineTemperatureStatus(float? waterTemperature, ThresholdConfigurationDto config)
    {
        var thresholdDto = config.Thresholds.FirstOrDefault(dto => dto.Type.Equals("water_temperature"));
        if (thresholdDto == null) throw new Exception("Threshold does not exist!");
        var isWarningRange =
            (waterTemperature <= thresholdDto.WarningMin && waterTemperature > thresholdDto.PerfectMin) ||
            (waterTemperature >= thresholdDto.WarningMax && waterTemperature < thresholdDto.PerfectMax);
        var isDangerRange = waterTemperature >= thresholdDto.PerfectMax || waterTemperature <= thresholdDto.PerfectMin;

        if (isDangerRange)
        {
            return "Dang";
        }

        return isWarningRange ? "Warn" : "Norm";
    }
}