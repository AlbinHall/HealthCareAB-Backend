using HealthCareABApi.DTO;
using HealthCareABApi.Repositories;

namespace HealthCareABApi.Services
{
    public class FeedbackService
    {
        private readonly IFeedbackRepository _feedbackRepository;

        public FeedbackService(IFeedbackRepository feedbackRepository)
        {
            _feedbackRepository = feedbackRepository;
        }

        public async Task<CaregiverFeedbackSummaryDTO> GetFeedbackSummaryByCaregiverIdAsync(int caregiverId)
        {
            var feedbacks = await _feedbackRepository.GetByCareGiverIdAsync(caregiverId);

            if (feedbacks == null || !feedbacks.Any())
            {
                return new CaregiverFeedbackSummaryDTO
                {
                    CaregiverId = caregiverId,
                    AverageRating = 0,
                    CommentsByRating = new Dictionary<int, List<string>>()
                };
            }

            // Calculate average rating
            double totalRating = feedbacks.Sum(f => f.Rating);
            double averageRating = totalRating / feedbacks.Count();

            // Group comments by rating
            var commentsByRating = feedbacks
                .GroupBy(f => f.Rating)
                .ToDictionary(
                    g => g.Key, // Rating
                    g => g.Select(f => f.Comment).ToList() // List of comments for this rating
                );

            return new CaregiverFeedbackSummaryDTO
            {
                CaregiverId = caregiverId,
                AverageRating = averageRating,
                CommentsByRating = commentsByRating
            };
        }
    }
}