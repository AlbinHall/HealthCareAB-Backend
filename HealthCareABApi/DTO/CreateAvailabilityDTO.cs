using System;
namespace HealthCareABApi.DTO
{
    public class CreateAvailabilityDTO
    {
        public Guid CaregiverId { get; set; }
        public List<DateTime> AvailableSlots { get; set; }
    }
}

