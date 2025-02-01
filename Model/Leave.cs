using System.ComponentModel.DataAnnotations;

namespace RhManagementApi.Model
{
    public class Leave : BaseEntity
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public DateTime StartDate { get; set; }
        
        [Required]
        public DateTime EndDate { get; set; }
        
        [Required]
        public string Status { get; set; } = string.Empty;

        [Required]
        public string Reason {get; set; } = string.Empty;
        
        [Required]
        public string Type { get; set; } = string.Empty;
        
        public string RHStatus { get; set; } = string.Empty;
        
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }
        
        public int AdminId { get; set; }
        public Admin Admin { get; set; }
    }
} 