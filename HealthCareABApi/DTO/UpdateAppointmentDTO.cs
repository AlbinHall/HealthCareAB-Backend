using HealthCareABApi.Models;

namespace HealthCareABApi.DTO
{
    public class UpdateAppointmentDTO
    {
        public int AppointmentId { get; set; }
        public int CaregiverId { get; set; }
        public int NewAvailabilityId { get; set; }
        public int OldAvailabilityId { get; set; }
        public DateTime AppointmentTime { get; set; }
        public AppointmentStatus Status { get; set; }
    }

}
