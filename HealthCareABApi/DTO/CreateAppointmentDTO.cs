using HealthCareABApi.Models;

namespace HealthCareABApi.DTO
{
    public class CreateAppointmentDTO
    {
        public int PatientId { get; set; }
        public int CaregiverId { get; set; }
        public string Description { get; set; }
        public DateTime AppointmentTime { get; set; }
    }
}
