using HealthCareABApi.Models;

namespace HealthCareABApi.DTO
{
    public class HistoryDTO
    {
        public int Id { get; set; }
        public string PatientName { get; set; }
        public string CaregiverName { get; set; }
        public DateTime DateTime { get; set; }
        public bool HasFeedback { get; set; }
    }
}
