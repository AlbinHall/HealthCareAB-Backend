namespace HealthCareABApi.DTO
{
    public class CaregiverFeedbackSummaryDTO
    {
        public int CaregiverId { get; set; }
        public double AverageRating { get; set; }
        public Dictionary<int, List<string>> CommentsByRating { get; set; }
    }
}