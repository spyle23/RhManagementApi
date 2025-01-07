using System.ComponentModel.DataAnnotations;

namespace RhManagementApi.Model
{
    public class Manager : Employee
    {
        [Required]
        public string ManagementLevel { get; set; } = string.Empty;
        
        [Required]
        public int YearsOfExperience { get; set; }
        
        public virtual ICollection<Team> ManagedTeams { get; set; } = new List<Team>();
    }
}
