namespace HealthCareABApi.DTO
{
    public class CreateAvailabilityDTO
    {
        public int CaregiverId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}