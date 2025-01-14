using System.ComponentModel.DataAnnotations;
using RhManagementApi.Enums;

namespace RhManagementApi.DTOs
{
    public class CreateLeaveDto
    {
        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public string Type { get; set; } = string.Empty;

        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public int AdminId { get; set; }
    }
} 