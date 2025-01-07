using System.ComponentModel.DataAnnotations;

namespace RhManagementApi.Model
{
    public class Team : BaseEntity
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public string Specialty { get; set; } = string.Empty;
        
        public int ManagerId { get; set; }
        public Manager Manager { get; set; } = new Manager();
        
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
} 