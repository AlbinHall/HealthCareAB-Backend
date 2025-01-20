﻿using HealthCareABApi.DTO;
using HealthCareABApi.Models;

namespace HealthCareABApi.Repositories.Interfaces
{
    public interface IFeedbackService
    {
        Task<CaregiverFeedbackSummaryDTO> GetFeedbackSummaryByCaregiverIdAsync(int caregiverId);
        Task<IEnumerable<FeedbackDTO>> GetFeedbackByRatingAsync(int rating);
    }
}