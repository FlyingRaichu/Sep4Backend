namespace DatabaseInterfacing.Domain.DTOs;

public class DisplayPlantECDto
    {
        public int Id { get; }
        public float? WaterEC { get; }
        public string Status { get; }

        public DisplayPlantECDto (float? waterEC, string status)
        {
            WaterEC = waterEC;
            Status = status;
        }
    } 
