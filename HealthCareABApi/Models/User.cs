using System;
using System.ComponentModel.DataAnnotations;

namespace HealthCareABApi.Models
{
    public class User
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string PasswordHash { get; set; }

        public virtual List<UserRole> Roles { get; set; } = [];
    }
}
