using System.ComponentModel.DataAnnotations;

namespace RhManagementApi.Model
{
    public class Admin : User
    {
        [Required]
        public string Department { get; set; } = string.Empty;
        
        [Required]
        public string AccessLevel { get; set; } = string.Empty;
        
        public virtual ICollection<Leave> ManagedLeaves { get; set; } = new List<Leave>();
    }
}
