using System.ComponentModel.DataAnnotations;

namespace RhManagementApi.DTOs
{
    public class CreatePayslipDto
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal GrossSalary { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Bonuses { get; set; } = 0;

        [Range(0, double.MaxValue)]
        public decimal Overtime { get; set; } = 0;

        [Required]
        public DateTime Month { get; set; }
    }
} 