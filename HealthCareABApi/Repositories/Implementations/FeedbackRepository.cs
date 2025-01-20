﻿using System;
using HealthCareABApi.Models;
using HealthCareABApi.Repositories.Data;
using Microsoft.EntityFrameworkCore;

namespace HealthCareABApi.Repositories.Implementations
{
    public class FeedbackRepository : IFeedbackRepository
    {
        private readonly HealthCareDbContext _Dbcontext;

        public FeedbackRepository(HealthCareDbContext context)
        {
            _Dbcontext = context;
        }

        public async Task<IEnumerable<Feedback>> GetAllAsync()
        {
            return await _Dbcontext.Feedback.ToListAsync();
        }

        public async Task<Feedback> GetByIdAsync(int id)
        {
            return await _Dbcontext.Feedback.Where(f => f.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Feedback> GetByAppointmentIdAsync(int id)
        {
            return await _Dbcontext.Feedback.Where(f => f.AppointmentId == id).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Feedback>> GetByPatientIdAsync(int id)
        {
            return await _Dbcontext.Feedback.Include(f => f.Appointment).Where(f => f.Appointment.PatientId == id).ToListAsync();
        }

        public async Task CreateAsync(Feedback feedback)
        {
            await _Dbcontext.Feedback.AddAsync(feedback);
            await _Dbcontext.SaveChangesAsync();
        }

        public async Task UpdateAsync(int id, Feedback feedback)
        {
            var exist = await _Dbcontext.Feedback.Where(f => f.Id == id).FirstOrDefaultAsync();
            _Dbcontext.Feedback.Entry(exist).CurrentValues.SetValues(feedback);
            await _Dbcontext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            await _Dbcontext.Feedback.Where(f => f.Id == id).ExecuteDeleteAsync();
            await _Dbcontext.SaveChangesAsync();
        }

        public async Task<IEnumerable<Feedback>> GetByCareGiverIdAsync(int id)
        {
            return await _Dbcontext.Feedback.Include(f => f.Appointment).Where(f => f.Appointment.CaregiverId == id).ToListAsync();
        }
        public async Task<IEnumerable<Feedback>> GetByRatingAsync(int rating)
        {
            return await _Dbcontext.Feedback.Where(f => f.Rating == rating).ToListAsync();
        }
    }
}

