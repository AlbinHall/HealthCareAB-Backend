using HealthCareABApi.Models;

namespace HealthCareABApi.DTO
{
    public class AppointmentResponseDTO
    {
        public int PatientId { get; set; }
        public int CaregiverId { get; set; }
        public DateTime AppointmentCreatedAt { get; set; }
        public AppointmentStatus Status { get; set; }
    }
}
