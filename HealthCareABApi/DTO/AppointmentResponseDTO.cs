using HealthCareABApi.Models;

namespace HealthCareABApi.DTO
{
    public class AppointmentResponseDTO
    {
        public int PatientId { get; set; }
        public int CaregiverId { get; set; }
        public DateTime AppointmentTime { get; set; }
        public AppointmentStatus Status { get; set; }
    }

    public class DetailedResponseDTO : AppointmentResponseDTO
    {
        public int Id { get; set; }
        public string PatientName { get; set; }
        public string CaregiverName { get; set; }
    }
}
