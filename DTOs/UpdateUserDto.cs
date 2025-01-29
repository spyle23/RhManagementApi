using System.ComponentModel.DataAnnotations;

namespace RhManagementApi.DTOs
{
    public class UpdateUserDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public DateTime? DateOfHiring { get; set; }
        public int? TeamId { get; set; }

        public long? Cin { get; set; }

        [Required]
        public string Role { get; set; } = string.Empty;

        public string? Department { get; set; }
        public string? AccessLevel { get; set; }

        public string? Specialization { get; set; }
        public string? Certification { get; set; }

        public string? ManagementLevel { get; set; }
        public int? YearsOfExperience { get; set; }

    }
}