using System;

namespace HealthCareABApi.Models
{
    public class Availability
    {
        public int Id { get; set; }
        public required User Caregiver { get; set; }
        public List<DateTime> AvailableSlots { get; set; }
    }
}

