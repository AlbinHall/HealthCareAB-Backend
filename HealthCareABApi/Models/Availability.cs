using HealthCareABApi.Models;

public class Availability
{
    public int Id { get; set; }
    public User Caregiver { get; set; }
    public int? AppointmentId { get; set; }
    public Appointment? Appointment { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsBooked { get; set; } = false;
}