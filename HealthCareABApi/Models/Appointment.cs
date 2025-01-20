using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
namespace HealthCareABApi.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual User Patient { get; set; }
        public int CaregiverId { get; set; }
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual User Caregiver { get; set; }
        public string Description { get; set; }
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

