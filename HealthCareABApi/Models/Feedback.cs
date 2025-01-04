using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthCareABApi.Models
{
    public class Feedback
    {
        public int Id { get; set; }
        public int AppointmentId { get; set; }
        public required string Comment { get; set; }
        public virtual Appointment Appointment { get; set; }
    }
}

