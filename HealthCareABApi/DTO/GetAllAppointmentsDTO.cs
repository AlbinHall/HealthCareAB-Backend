using HealthCareABApi.Models;

namespace HealthCareABApi.DTO
{
    public class GetAllAppointmentsDTO
    {
        public int Id { get; set; }
        public string PatientName { get; set; }
        public string CaregiverName { get; set; }
        public DateTime AppointmentTime { get; set; }
        public string Description { get; set; }
        public AppointmentStatus Status { get; set; }
    }
}
