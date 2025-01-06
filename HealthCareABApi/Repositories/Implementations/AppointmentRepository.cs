using System;
using HealthCareABApi.Models;
using HealthCareABApi.Repositories.Data;
using Microsoft.EntityFrameworkCore;

namespace HealthCareABApi.Repositories.Implementations
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly HealthCareDbContext _Dbcontext;

        public AppointmentRepository(HealthCareDbContext context)
        {
            _Dbcontext = context;
        }

        public async Task<IEnumerable<Appointment>> GetAllAsync()
        {
            return await _Dbcontext.Appointment.Include(a => a.Patient).Include(a => a.Caregiver).ToListAsync();
        }

        public async Task<Appointment> GetByIdAsync(int id)
        {
            return await _Dbcontext.Appointment.Where(a => a.Id == id).FirstAsync();
        }

        public async Task<Appointment> GetByUserIdAsync(int userid)
        {
            return await _Dbcontext.Appointment.Where(x => x.PatientId == userid && x.Status == AppointmentStatus.Completed).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Appointment appointment)
        {
            await _Dbcontext.Appointment.AddAsync(appointment);
            await _Dbcontext.SaveChangesAsync();
        }

        public async Task UpdateAsync(int id, Appointment appointment)
        {
            var exist = await _Dbcontext.Appointment.Where(a => a.Id == id).FirstOrDefaultAsync();
            _Dbcontext.Appointment.Entry(exist).CurrentValues.SetValues(appointment);
            await _Dbcontext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            await _Dbcontext.Appointment.Where(a => a.Id == id).ExecuteDeleteAsync();
            await _Dbcontext.SaveChangesAsync();
        }
    }
}

