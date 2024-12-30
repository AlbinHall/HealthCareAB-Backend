using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthCareABApi.Models
{
    public class Feedback
    {
        public int Id { get; set; }

        // Reference to Appointment
        [ForeignKey(nameof(Appointment.Id))]
        public int AppointmentId { get; set; }

        // Reference to Patient (User)
        [ForeignKey(nameof(Appointment.Patient))]
        public User PatientId { get; set; }
        public required string Comment { get; set; }

        [ForeignKey(nameof(AppointmentId))]
        public virtual Appointment Appointment { get; set; }
    }
}

