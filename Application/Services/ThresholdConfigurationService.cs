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
        SaveConfigurationAsync(_currentConfig);
    }

    public Task<ThresholdConfigurationDto> GetConfigurationAsync()
    {
        var configDto = new ThresholdConfigurationDto
        {
            Thresholds = _currentConfig.Thresholds
        };
        return Task.FromResult(configDto);
    }

    public async Task UpdateConfigurationAsync(ThresholdDto dto)
    {
        var threshold = _currentConfig.Thresholds.FirstOrDefault(d => dto.Type == d.Type);
        if (threshold == null) throw new Exception("No threshold with such type!");
        threshold.Min = dto.Min;
        threshold.Max = dto.Max;
        threshold.WarningMin = dto.WarningMin;
        threshold.WarningMax = dto.WarningMax;
        await SaveConfigurationAsync(_currentConfig);
    }

    private ThresholdConfigurationDto LoadConfiguration()
    {
        if (!File.Exists(ConfigFilePath)) return LoadGeneralConfiguration();
        
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
        Console.WriteLine("File saved!");
    }

    private ThresholdConfigurationDto LoadGeneralConfiguration()
    {
        ThresholdConfigurationDto dto = new ThresholdConfigurationDto();
        dto.Thresholds.Add(new ThresholdDto()
        {
            Type = "water_conductivity",
            Min = 6,
            Max = 7,
            WarningMin = 6.2,
            WarningMax = 6.8
        });
        dto.Thresholds.Add(new ThresholdDto()
        {
            Type = "water_temperature",
            Min = 6,
            Max = 7,
            WarningMin = 6.2,
            WarningMax = 6.8
        });
        dto.Thresholds.Add(new ThresholdDto()
        {
            Type = "water_ph",
            Min = 6,
            Max = 7,
            WarningMin = 6.2,
            WarningMax = 6.8
        });
        dto.Thresholds.Add(new ThresholdDto()
        {
            Type = "water_flow",
            Min = 6,
            Max = 7,
            WarningMin = 6.2,
            WarningMax = 6.8
        });
        return dto;
    }

}