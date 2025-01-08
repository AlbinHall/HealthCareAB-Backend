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
            return await _Dbcontext.Appointment
                .Include(a => a.Patient)
                .Include(a => a.Caregiver)
                .ToListAsync();
        }

        public async Task<Appointment> GetByIdAsync(int id)
        {
            return await _Dbcontext.Appointment.Where(a => a.Id == id).FirstAsync();
        }

        public async Task<IEnumerable<Appointment>> GetByUserIdAsync(int patientId)
        {
            return await _Dbcontext.Appointment
                .Include(x => x.Caregiver)
                .Include(x => x.Patient)
                .Where(x => x.PatientId == patientId && x.Status == AppointmentStatus.Completed)
                .ToListAsync();
        }

        public async Task CreateAsync(Appointment appointment)
        {
            if (appointment == null)
            {
                throw new ArgumentNullException(nameof(appointment), "Appointment is null and will blow up the system in 3........2........1.........");
            }

            try
            {
                await _Dbcontext.Appointment.AddAsync(appointment);
                await _Dbcontext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Database error while creating appointment", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error creating new appointment", ex);
            }
        }

        public async Task<bool> UpdateAsync(int id, Appointment appointment)
        {
            var exist = await _Dbcontext.Appointment.Where(a => a.Id == id).FirstOrDefaultAsync();

            if (exist == null)
            {
                return false;
            }

            _Dbcontext.Appointment.Entry(exist).CurrentValues.SetValues(appointment);
            await _Dbcontext.SaveChangesAsync();
            return true;
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                await _Dbcontext.Appointment.Where(a => a.Id == id).ExecuteDeleteAsync();
                await _Dbcontext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Database error while deleting appointment", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error deleting appointment", ex);
            }
        }
    }
}

