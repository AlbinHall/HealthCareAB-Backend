namespace HealthCareABApi.DTO
{
    public class ScheduledAppointmentsDTO
    {
        public int Id { get; set; }
        public DateTime AppointmentTime { get; set; }
        public string CaregiverName { get; set; }
        public string PatientName { get; set; }
    }
}
