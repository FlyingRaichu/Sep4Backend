using DatabaseInterfacing;
using DatabaseInterfacing.Context;
using DatabaseInterfacing.Domain.EntityFramework;

namespace DbtestForreal
{
    class Program
    {
       public static void Main(string[] args)
        {
            // Build DbContext options
            var options = DatabaseUtils.BuildConnectionOptions();

            // Create a new instance of the DbContext
            using (var dbContext = new PlantDbContext(options))
            {
                // Ensure the database is created
                dbContext.Database.EnsureCreated();

                // Add sample data
                var plantData = new PlantData
                {
                    PlantName = "Sample Plant",
                    WaterTemperature = 25.5f,
                    PhLevel = 7.2f,
                    WaterFlow = 2.2f
                };
                dbContext.PlantData.Add(plantData);

                var user = new User
                {
                    UserName = "sample_user",
                    Password = "sample_password",
                    Email = "sample@example.com"
                };
                dbContext.Users.Add(user);

                dbContext.SaveChanges();

                Console.WriteLine("Sample data added successfully.");

                // Retrieve and display data
                var retrievedPlantData = dbContext.PlantData.FirstOrDefault();
                if (retrievedPlantData != null)
                {
                    Console.WriteLine($"Retrieved Plant Data: Id={retrievedPlantData.Id}, Name={retrievedPlantData.PlantName}, Water Temperature={retrievedPlantData.WaterTemperature}, pH Level={retrievedPlantData.PhLevel}, Water Flow={retrievedPlantData.WaterFlow}");
                }
                else
                {
                    Console.WriteLine("No plant data found.");
                }

                var retrievedUser = dbContext.Users.FirstOrDefault();
                if (retrievedUser != null)
                {
                    Console.WriteLine($"Retrieved User: Id={retrievedUser.Id}, Username={retrievedUser.UserName}, Email={retrievedUser.Email}");
                }
                else
                {
                    Console.WriteLine("No user found.");
                }
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
