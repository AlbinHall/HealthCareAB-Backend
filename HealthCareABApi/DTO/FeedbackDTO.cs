using System.ComponentModel.DataAnnotations;

namespace HealthCareABApi.DTO
{
    public class FeedbackDTO
    {
        public required int AppointmentId { get; set; }
        public required string Comment { get; set; }

        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public required int Rating { get; set; }
    }
}
