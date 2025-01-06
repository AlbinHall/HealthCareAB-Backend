using System;
namespace HealthCareABApi.Models
{
    public class Role
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public virtual List<UserRole> Roles { get; set; } = [];
    }
}
