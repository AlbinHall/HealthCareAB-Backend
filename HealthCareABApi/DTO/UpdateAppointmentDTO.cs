using HealthCareABApi.Models;

namespace HealthCareABApi.DTO
{
    public class UpdateAppointmentDTO
    {
        public int CaregiverId { get; set; }
        public DateTime AppointmentTime { get; set; }
        public AppointmentStatus Status { get; set; }
    }

}
