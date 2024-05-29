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
    public class AirDataLogic : IAirDataLogic
    {
        private readonly IConnectionController _connectionController;
        private readonly IThresholdConfigurationService _configurationService;
        private readonly IAlertNotificationService _alertNotificationService;

        public AirDataLogic(IConnectionController connectionController,
            IThresholdConfigurationService configurationService,
            IAlertNotificationService alertNotificationService)
        {
            _connectionController = connectionController;
            _configurationService = configurationService;
            _alertNotificationService = alertNotificationService;
        }

        public async Task<DisplayAirTemperatureDto> CheckAirTemperatureAsync()
        {
            var jsonString = await _connectionController.SendRequestToArduinoAsync(ApiParameters.DataRequest);
            var plantData = JsonSerializer.Deserialize<MonitoringResultDto>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (plantData == null) throw new Exception("Plant Data object is null or empty.");

            var configuration = await _configurationService.GetConfigurationAsync();
            var status = DetermineStatus("airTemperature", plantData.Readings.FirstOrDefault()?.Measurements["airTemperature"].GetSingle(), configuration);

            return new DisplayAirTemperatureDto
            {
                Status = status,
                AirTemperatureInC = (float)plantData.Readings.FirstOrDefault()?.Measurements["airTemperature"].GetSingle()!
            };
        }

        public async Task<DisplayAirHumidityDto> CheckAirHumidityAsync()
        {
            var jsonString = await _connectionController.SendRequestToArduinoAsync(ApiParameters.DataRequest);
            var plantData = JsonSerializer.Deserialize<MonitoringResultDto>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (plantData == null) throw new Exception("Plant Data object is null or empty.");

            var configuration = await _configurationService.GetConfigurationAsync();
            var status = DetermineStatus("airHumidity", plantData.Readings.FirstOrDefault()?.Measurements["airHumidity"].GetSingle(), configuration);

            return new DisplayAirHumidityDto
            {
                Status = status,
                AirHumidityPercentage = (float)plantData.Readings.FirstOrDefault()?.Measurements["airHumidity"].GetSingle()!
            };
        }

        public async Task<DisplayAirCO2Dto> CheckAirCO2Async()
        {
            var jsonString = await _connectionController.SendRequestToArduinoAsync(ApiParameters.DataRequest);
            var plantData = JsonSerializer.Deserialize<MonitoringResultDto>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (plantData == null) throw new Exception("Plant Data object is null or empty.");

            var configuration = await _configurationService.GetConfigurationAsync();
            var status = DetermineStatus("airCo2", plantData.Readings.FirstOrDefault()?.Measurements["airCo2"].GetSingle(), configuration);

            return new DisplayAirCO2Dto
            {
                Status = status,
                AirCO2 = plantData.Readings.FirstOrDefault()?.Measurements["airCo2"].GetSingle()!
            };
        }

        public async Task<DisplayVPDLevelDto> CheckVPDAsync()
        {
            var jsonString = await _connectionController.SendRequestToArduinoAsync(ApiParameters.DataRequest);
            var plantData = JsonSerializer.Deserialize<MonitoringResultDto>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (plantData == null) throw new Exception("Plant Data object is null or empty.");

            var airTemperature = plantData.Readings.FirstOrDefault()?.Measurements["airTemperature"];
            var airHumidity = plantData.Readings.FirstOrDefault()?.Measurements["airHumidity"];

            if (airTemperature == null || airHumidity == null) throw new Exception("Insufficient data to calculate VPD.");

            var vpd = CalculateVPD(airTemperature.Value.GetSingle(), airHumidity.Value.GetSingle());
            var configuration = await _configurationService.GetConfigurationAsync();
            var status = DetermineStatus("vpdLevel", vpd, configuration);

            return new DisplayVPDLevelDto
            {
                Status = status,
                VPDLevel = vpd
            };
        }

        public async Task<DisplayDewPointDto> CheckDewPointAsync()
        {
            var jsonString = await _connectionController.SendRequestToArduinoAsync(ApiParameters.DataRequest);
            var plantData = JsonSerializer.Deserialize<MonitoringResultDto>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (plantData == null) throw new Exception("Plant Data object is null or empty.");

            var airTemperature = plantData.Readings.FirstOrDefault()?.Measurements["airTemperature"];
            var airHumidity = plantData.Readings.FirstOrDefault()?.Measurements["airHumidity"];

            if (airTemperature == null || airHumidity == null) throw new Exception("Insufficient data to calculate Dew Point.");

            var dewPoint = CalculateDewPoint(airTemperature.Value.GetSingle(), airHumidity.Value.GetSingle());
            var configuration = await _configurationService.GetConfigurationAsync();
            var status = DetermineStatus("dewPoint", dewPoint, configuration);

            return new DisplayDewPointDto
            {
                Status = status,
                DewPoint = dewPoint
            };
        }

        public async Task<DisplayLightLevelDto> CheckLightLevelAsync()
        {
            var jsonString = await _connectionController.SendRequestToArduinoAsync(ApiParameters.DataRequest);
            var plantData = JsonSerializer.Deserialize<MonitoringResultDto>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (plantData == null) throw new Exception("Plant Data object is null or empty.");

            var configuration = await _configurationService.GetConfigurationAsync();
            var status = DetermineStatus("lightLevel", plantData.Readings.FirstOrDefault()?.Measurements["lightLevel"].GetSingle(), configuration);

            return new DisplayLightLevelDto(plantData.Readings.FirstOrDefault()?.Measurements["lightLevel"].GetSingle(), status);
        }

        private static float CalculateVPD(float temperature, float humidity)
        {
            // Calculation for VPD
            float es = 0.6108f * (float)Math.Exp((17.27f * temperature) / (temperature + 237.3f));
            float ea = humidity / 100 * es;
            return es - ea;
        }

        private static float CalculateDewPoint(float temperature, float humidity)
        {
            // Calculation for Dew Point
            double a = 17.27;
            double b = 237.7;
            double alpha = ((a * temperature) / (b + temperature)) + Math.Log(humidity / 100.0);
            return (float)((b * alpha) / (a - alpha));
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
