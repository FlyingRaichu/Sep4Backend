using System.Text.Json;
using Application.ServiceInterfaces;
using DatabaseInterfacing.Domain.DTOs;

namespace Application.Services;

public class ThresholdConfigurationService : IThresholdConfigurationService
{
    private const string ConfigFilePath = "Config/thresholdConfig.json";
    private ThresholdConfigurationDto _currentConfig;

    public ThresholdConfigurationService()
    {
        _currentConfig = LoadConfiguration();
    }

    public Task<ThresholdConfigurationDto> GetConfigurationAsync()
    {
        var configDto = new ThresholdConfigurationDto
        {
            MinTemperature = _currentConfig.MinTemperature,
            WarningTemperatureMin = _currentConfig.WarningTemperatureMin,
            WarningTemperatureMax = _currentConfig.WarningTemperatureMax,
            MaxTemperature = _currentConfig.MaxTemperature
        };
        return Task.FromResult(configDto);
    }

    public async Task UpdateConfigurationAsync(ThresholdConfigurationDto configDto)
    {
        _currentConfig.MinTemperature = configDto.MinTemperature;
        _currentConfig.WarningTemperatureMin = configDto.WarningTemperatureMin;
        _currentConfig.WarningTemperatureMax = configDto.WarningTemperatureMax;
        _currentConfig.MaxTemperature = configDto.MaxTemperature;
        
        await SaveConfigurationAsync(_currentConfig);
    }

    private ThresholdConfigurationDto LoadConfiguration()
    {
        if (!File.Exists(ConfigFilePath)) return new ThresholdConfigurationDto();
        
        var json = File.ReadAllText(ConfigFilePath);
        return JsonSerializer.Deserialize<ThresholdConfigurationDto>(json) ?? throw new Exception("File is empty. Save some data before attempting to load.");
    }

    private async Task SaveConfigurationAsync(ThresholdConfigurationDto config)
    {
        // Ensure the directory exists
        var directory = Path.GetDirectoryName(ConfigFilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });

        // Ensure atomic file writing to avoid data corruption
        var tempFilePath = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFilePath, json);
        File.Move(tempFilePath, ConfigFilePath, true);
    }

}