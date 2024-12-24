using System.ComponentModel.DataAnnotations;

namespace RhManagementApi.Model
{
    public class Payslip : BaseEntity
    {
        [Key]
        public int Id { get; set; }
        
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }
        
        [Required]
        public DateTime Month { get; set; }
        
        [Required]
        public decimal GrossSalary { get; set; }
        
        public decimal Bonuses { get; set; }
        
        [Required]
        public decimal NetSalary { get; set; }
        
        public decimal Overtime { get; set; }
    }
} 