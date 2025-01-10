namespace HealthCareABApi.DTO
{
    public class AvailableSlotsDTO
    {
        public int Id { get; set; }
        public int CaregiverId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsBooked { get; set; }
        public int AppointmentId { get; set; }
    }
}