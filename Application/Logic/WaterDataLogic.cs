using System.Globalization;
using System.Text.Json;
using Application.LogicInterfaces;
using Application.ServiceInterfaces;
using Application.Services;
using DatabaseInterfacing;
using DatabaseInterfacing.Context;
using DatabaseInterfacing.Domain.DTOs;
using DatabaseInterfacing.Domain.EntityFramework;
using IoTInterfacing.Interfaces;
using IoTInterfacing.Util;
using IoTInterfacing.Implementations;
using IoTInterfacing.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Logic
{
    public class WaterDataLogic : IWaterDataLogic
    {
        private readonly IConnectionController _connectionController;
        private readonly IThresholdConfigurationService _configurationService;
        private readonly IAlertNotificationService _alertNotificationService;
        private readonly IOutputService _outputService;
        public bool WaterFlowCorrectionEnabled { get; set; } = false;

        private static string TEST = @"
        {
        ""name"": ""monitoring_results"",
        ""readings"":     [{
                        ""waterConductivity"":    2622
                }, {
                        ""waterPh"":      6
                }, {
                        ""waterTemperature"":     15
                }, {
                        ""waterFlow"":    0
                }, {
                        ""waterLevel"":   16
                }]
}";

        public WaterDataLogic(IConnectionController connectionController,
            IThresholdConfigurationService configurationService,
            IAlertNotificationService alertNotificationService)
        {
            _connectionController = connectionController;
            _configurationService = configurationService;
            _alertNotificationService = alertNotificationService;
            _outputService = new OutputService(_connectionController);
        }

        public async Task<DisplayPlantTemperatureDto?> CheckWaterTemperatureAsync()
        {
            var jsonString = await _connectionController.SendRequestToArduinoAsync(ApiParameters.DataRequest);
            var plantData = JsonSerializer.Deserialize<MonitoringResultDto>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (plantData == null) throw new Exception("Plant Data object is null or empty.");

            var configuration = await _configurationService.GetConfigurationAsync();
            var status = DetermineStatus("waterTemperature", plantData.Readings.FirstOrDefault()?.Measurements["waterTemperature"].GetSingle(), configuration);

            return new DisplayPlantTemperatureDto(plantData.Readings.FirstOrDefault()?.Measurements["waterTemperature"].GetSingle(), status);
        }

        public async Task<DisplayPlantPhDto> GetPhLevelAsync()
        {
            var jsonString = await _connectionController.SendRequestToArduinoAsync(ApiParameters.DataRequest);
            var plantData = JsonSerializer.Deserialize<MonitoringResultDto>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (plantData == null) throw new Exception("Plant Data object is null or empty.");

            var configuration = await _configurationService.GetConfigurationAsync();
            var status = DetermineStatus("waterPh", plantData.Readings.FirstOrDefault()?.Measurements["waterPh"].GetSingle(), configuration);

            return new DisplayPlantPhDto { Status = status, PhLevel = plantData.Readings.FirstOrDefault()?.Measurements["waterPh"].GetSingle() };
        }

        public async Task<DisplayPlantECDto?> CheckECAsync()
        {
            var jsonString = await _connectionController.SendRequestToArduinoAsync(ApiParameters.DataRequest);
            var plantData = JsonSerializer.Deserialize<MonitoringResultDto>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (plantData == null) throw new Exception("Plant Data object is null or empty.");

            var configuration = await _configurationService.GetConfigurationAsync();
            var status = DetermineStatus("waterConductivity", plantData.Readings.FirstOrDefault()?.Measurements["waterConductivity"].GetSingle(), configuration);

            return new DisplayPlantECDto(plantData.Readings.FirstOrDefault()?.Measurements["waterConductivity"].GetSingle(), status);
        }

        public async Task<DisplayPlantWaterFlowDto> CheckWaterFlowAsync()
        {
            var jsonString = await _connectionController.SendRequestToArduinoAsync(ApiParameters.DataRequest);
            var plantData = JsonSerializer.Deserialize<MonitoringResultDto>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (plantData == null) throw new Exception("Plant Data object is null or empty.");

            var configuration = await _configurationService.GetConfigurationAsync();
            var reading = plantData.Readings.First().Measurements["waterFlow"];
            var status = DetermineStatus("waterFlow", reading.GetSingle(), configuration);

            var dto = new DisplayPlantWaterFlowDto
            {
                Status = status,
                WaterFlow = (float)plantData.Readings.FirstOrDefault()?.Measurements["waterFlow"].GetSingle()!
            };

            var waterFlowValues = configuration.Thresholds.FirstOrDefault(type => type.Type.Equals("waterFlow"))!;
            var perfectThreshold = (waterFlowValues.WarningMin + waterFlowValues.WarningMax) / 2;

            if (!WaterFlowCorrectionEnabled) return dto;

            IPidService pid = new PidService(0.5, 0.1, 0.1, perfectThreshold);
            await _outputService.AlterPumpAsync("waterFlowCorrection", pid.Compute(reading.GetSingle(), 5));

            return dto;
        }

        public async Task<DisplayPlantWaterLevelDto> CheckWaterLevelAsync()
        {
            var jsonString = await _connectionController.SendRequestToArduinoAsync(ApiParameters.DataRequest);
            var plantData = JsonSerializer.Deserialize<MonitoringResultDto>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (plantData == null) throw new Exception("Plant Data object is null or empty.");

            var configuration = await _configurationService.GetConfigurationAsync();
            var status = DetermineStatus("waterLevel", plantData.Readings.FirstOrDefault()?.Measurements["waterLevel"].GetSingle(), configuration);

            return new DisplayPlantWaterLevelDto
            {
                Status = status,
                WaterLevelInMillimeters = (float)plantData.Readings.FirstOrDefault()?.Measurements["waterLevel"].GetSingle()!
            };
        }

        public async Task<bool> ToggleWaterFlowCorrectionAsync()
        {
            WaterFlowCorrectionEnabled = !WaterFlowCorrectionEnabled;

            return WaterFlowCorrectionEnabled;
        }

        private static string DetermineStatus(string type, float? reading, ThresholdConfigurationDto config)
        {
            var threshold = config.Thresholds.FirstOrDefault(dto => dto.Type.Equals(type));
            if (threshold == null) throw new Exception("Threshold does not exist!");

            var isWarningRange = (reading <= threshold.WarningMin && reading > threshold.Min) || (reading >= threshold.WarningMax && reading < threshold.Max);
            var isDangerRange = reading >= threshold.Max || reading <= threshold.Min;

            return isDangerRange ? "Danger" : isWarningRange ? "Warning" : "Normal";
        }
        
    }
}
