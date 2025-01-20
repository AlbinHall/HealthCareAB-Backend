using System;
using HealthCareABApi.DTO;
using HealthCareABApi.Models;

namespace HealthCareABApi.Repositories
{
    public interface IFeedbackRepository
    {
        Task<IEnumerable<Feedback>> GetAllAsync();
        Task<Feedback> GetByIdAsync(int id);
        Task<Feedback> GetByAppointmentIdAsync(int id);
        Task CreateAsync(Feedback feedback);
        Task UpdateAsync(int id, Feedback feedback);
        Task DeleteAsync(int id);
        Task<IEnumerable<Feedback>> GetByPatientIdAsync(int id);
        Task<IEnumerable<Feedback>> GetByCareGiverIdAsync(int id);
        Task<IEnumerable<Feedback>> GetByRatingAsync(int rating);
    }
}

