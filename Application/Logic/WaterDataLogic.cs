using System.Text.Json;
using Application.LogicInterfaces;
using Application.ServiceInterfaces;
using DatabaseInterfacing;
using DatabaseInterfacing.Context;
using DatabaseInterfacing.Domain.DTOs;
using DatabaseInterfacing.Domain.EntityFramework;
using IoTInterfacing.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Logic
{
    public class WaterDataLogic : IWaterDataLogic
    {
        private readonly IConnectionController _connectionController;
        private readonly IThresholdConfigurationService _configurationService;
        private readonly IAlertNotificationService _alertNotificationService;

        public WaterDataLogic(IConnectionController connectionController, IThresholdConfigurationService configurationService, IAlertNotificationService alertNotificationService)
        {
            _connectionController = connectionController;
            _configurationService = configurationService;
            _alertNotificationService = alertNotificationService;
        }

        public async Task<IEnumerable<PlantData>> GetWaterDataAsync(SearchPlantDataDto searchDto)
        {
            await using var dbContext = new PlantDbContext(DatabaseUtils.BuildConnectionOptions());

            var query = dbContext.PlantData.AsQueryable();

            if (!string.IsNullOrEmpty(searchDto.PlantName))
            {
                query = query.Where(plant => plant.PlantName.Contains(searchDto.PlantName));
            }

            if (searchDto.WaterTemperature != null)
            {
                query = query.Where(plant => plant.WaterTemperature.Equals(searchDto.WaterTemperature));
            }

            if (searchDto.PhLevel != null)
            {
                query = query.Where(plant => plant.PhLevel.Equals(searchDto.PhLevel));
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

        public async Task<DisplayPlantTemperatureDto?> CheckWaterTemperatureAsync()
        {
            var jsonString = await _connectionController.SendRequestToArduinoAsync(ApiParameters.DataRequest);
            var plantData = JsonSerializer.Deserialize<MonitoringResultDto>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (plantData == null) throw new Exception("Plant Data object is null or empty.");

            var configuration = await _configurationService.GetConfigurationAsync();
            var status = DetermineStatus("water_temperature", plantData.Readings.FirstOrDefault()?.WaterTemperature, configuration);

            return new DisplayPlantTemperatureDto(plantData.Readings.FirstOrDefault()?.WaterTemperature, status);
        }

        public async Task<DisplayPlantPhDto> GetPhLevelAsync()
        {
            var jsonString = await _connectionController.SendRequestToArduinoAsync(ApiParameters.DataRequest);
            var plantData = JsonSerializer.Deserialize<MonitoringResultDto>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (plantData == null) throw new Exception("Plant Data object is null or empty.");

            var configuration = await _configurationService.GetConfigurationAsync();
            var status = DetermineStatus("water_ph", plantData.Readings.FirstOrDefault()?.WaterPhLevel, configuration);

            return new DisplayPlantPhDto { Status = status, PhLevel = plantData.Readings.FirstOrDefault()?.WaterPhLevel };
        }

        public Task<DisplayPlantECDto?> CheckECAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<DisplayPlantECDto?> CheckWaterECAsync()
        {
            var jsonString = await _connectionController.SendRequestToArduinoAsync(ApiParameters.DataRequest);
            var plantData = JsonSerializer.Deserialize<MonitoringResultDto>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (plantData == null) throw new Exception("Plant Data object is null or empty.");

            var configuration = await _configurationService.GetConfigurationAsync();
            var status = DetermineStatus("water_conductivity", plantData.Readings.FirstOrDefault()?.WaterConductivity, configuration);

            return new DisplayPlantECDto(plantData.Readings.FirstOrDefault()?.WaterConductivity, status);
        }

        public async Task<DisplayPlantWaterFlowDto> CheckWaterFlowAsync()
        {
            var jsonString = await _connectionController.SendRequestToArduinoAsync(ApiParameters.DataRequest);
            var plantData = JsonSerializer.Deserialize<MonitoringResultDto>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (plantData == null) throw new Exception("Plant Data object is null or empty.");

            var configuration = await _configurationService.GetConfigurationAsync();
            var status = DetermineStatus("water_flow", plantData.Readings.FirstOrDefault()?.WaterFlow, configuration);

            return new DisplayPlantWaterFlowDto { Status = status, WaterFlow = (float)plantData.Readings.FirstOrDefault()?.WaterFlow! };
        }

        public async Task<DisplayPlantWaterLevelDto> CheckWaterLevelAsync()
        {
            var jsonString = await _connectionController.SendRequestToArduinoAsync(ApiParameters.DataRequest);
            var plantData = JsonSerializer.Deserialize<MonitoringResultDto>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (plantData == null) throw new Exception("Plant Data object is null or empty.");

            var configuration = await _configurationService.GetConfigurationAsync();
            var status = DetermineStatus("water_level", plantData.Readings.FirstOrDefault()?.WaterLevel, configuration);

            return new DisplayPlantWaterLevelDto { Status = status, WaterLevelInMillimeters = (float)plantData.Readings.FirstOrDefault()?.WaterLevel! };
        }

        private string DetermineStatus(string type, float? reading, ThresholdConfigurationDto config)
        {
            var threshold = config.Thresholds.FirstOrDefault(dto => dto.Type.Equals(type));
            if (threshold == null) throw new Exception("Threshold does not exist!");

            var isWarningRange =
                (reading <= threshold.WarningMin && reading > threshold.Min) ||
                (reading >= threshold.WarningMax && reading < threshold.Max);

            var isDangerRange = reading >= threshold.Max || reading <= threshold.Min;
            if (isDangerRange) return "Danger";

            return isWarningRange ? "Warning" : "Normal";
        }
    }
}
