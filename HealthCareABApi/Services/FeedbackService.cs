using HealthCareABApi.DTO;
using HealthCareABApi.Repositories;
using HealthCareABApi.Repositories.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;

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

        public async Task <IEnumerable<FeedbackDTO>> GetFeedbackByRatingAsync(int rating)
        {
            var feedbacks = await _feedbackRepository.GetByRatingAsync(rating);
            List<FeedbackDTO> FeedbackList = new();

            if (feedbacks == null || !feedbacks.Any())
            {
                return FeedbackList; //Return empty list.
            }

            foreach (var feedback in feedbacks)
            {
                FeedbackList.Add(
                    new FeedbackDTO 
                    { 
                        AppointmentId = feedback.AppointmentId, 
                        Comment = feedback.Comment, 
                        Rating = feedback.Rating 
                    });
            }

            return FeedbackList;
        }
    }
}