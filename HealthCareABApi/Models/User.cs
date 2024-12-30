using System;
using System.ComponentModel.DataAnnotations;

namespace HealthCareABApi.Models
{
    public class User
    {
        public int Id { get; set; } 

        public required string Username { get; set; }
        public required string PasswordHash { get; set; }
        // List of roles, a User can have one or more roles if needed.
        // Not specifying a role during User creation sets it to User by default
        public List<string> Roles { get; set; }
    }

}
