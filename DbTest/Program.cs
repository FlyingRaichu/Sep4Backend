using System;
using DatabaseInterfacing;
using Microsoft.EntityFrameworkCore;
using DatabaseInterfacing.Context;

class Program
{
    static void Main(string[] args)
    {

        using (var dbContext = new PlantDbContext(DatabaseUtils.BuildConnectionOptions()))
        {
            dbContext.Database.EnsureCreated();
            
            var plantData = new PlantData
            {
                PlantName = "Test Plant",
                WaterTemperature = 25.5f,
                PHLevel = 7.0f
            };
            dbContext.PlantData.Add(plantData);
            dbContext.SaveChanges();

            // Retrieve and display all PlantData records
            var allPlantData = dbContext.PlantData.ToList();
            foreach (var data in allPlantData)
            {
                Console.WriteLine($"ID: {data.Id}, PlantName: {data.PlantName}, WaterTemperature: {data.WaterTemperature}, PHLevel: {data.PHLevel}");
            }
        }
    }
}