using HealthCareABApi.Models;

namespace HealthCareABApi.DTO
{
    public class CreateAppointmentDTO
    {
        public int PatientId { get; set; }
        public int CaregiverId { get; set; }
        public DateTime DateTime { get; set; } = DateTime.UtcNow;
    }
}
