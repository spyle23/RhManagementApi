using System.ComponentModel.DataAnnotations;

namespace RhManagementApi.DTOs
{
    public class ListLeavesDto
    {
        public int Id { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public string Status { get; set; } = string.Empty;
        [Required]
        public string Type { get; set; } = string.Empty;

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;
    }
}