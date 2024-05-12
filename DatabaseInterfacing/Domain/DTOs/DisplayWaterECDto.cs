namespace DatabaseInterfacing.Domain.DTOs;

public class DisplayWaterECDto
    {
        public int Id { get; }
        public float WaterEC { get; }
        public string Status { get; }

        public DisplayWaterECDto (float waterEC, string status)
        {
            WaterEC = waterEC;
            Status = status;
        }
    } 
