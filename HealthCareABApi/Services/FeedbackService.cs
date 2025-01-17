using HealthCareABApi.DTO;
using HealthCareABApi.Repositories;
using HealthCareABApi.Repositories.Interfaces;

namespace HealthCareABApi.Services
{
    public class FeedbackService : IFeedbackService
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

            double totalRating = feedbacks.Sum(f => f.Rating);
            double averageRating = totalRating / feedbacks.Count();

            var commentsByRating = feedbacks
                .GroupBy(f => f.Rating)
                .ToDictionary(
                    g => g.Key, // Rating
                    g => g.Select(f => f.Comment).ToList()
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