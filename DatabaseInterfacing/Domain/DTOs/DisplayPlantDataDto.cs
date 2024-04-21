﻿namespace DatabaseInterfacing.Domain.DTOs;

public class DisplayPlantDataDto
{
    public string Name { get; }
    public float WaterTemperature { get; }
    public float PHLevel { get; }

    public DisplayPlantDataDto(string name, float waterTemperature, float phLevel)
    {
        Name = name;
        WaterTemperature = waterTemperature;
        PHLevel = phLevel;
    }
}