using System;
using Microsoft.EntityFrameworkCore;
namespace HealthCareABApi.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public required User Patient { get; set; }
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public required User Caregiver { get; set; }
        public DateTime DateTime { get; set; }
        public AppointmentStatus Status { get; set; }
    }

    public enum AppointmentStatus
    {
        Scheduled,
        Completed,
        Cancelled
    }
}

