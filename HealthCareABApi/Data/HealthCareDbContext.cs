using HealthCareABApi.Models;
using Microsoft.EntityFrameworkCore;

namespace HealthCareABApi.Repositories.Data
{
    public class HealthCareDbContext : DbContext
    {
        public HealthCareDbContext(DbContextOptions<HealthCareDbContext> options) : base(options)
        {
        }

        public DbSet<Appointment> Appointment { get; set; }
        public DbSet<Availability> Availability { get; set; }
        public DbSet<Feedback> Feedback { get; set; }
        public DbSet<User> User { get; set; }

    }
}
