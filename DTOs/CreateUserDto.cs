using System.ComponentModel.DataAnnotations;

namespace RhManagementApi.DTOs
{
    public class CreateUserDto
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public long Cin { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;

        // Additional fields for Employee-based roles
        public DateTime? DateOfHiring { get; set; }
        public int? TeamId { get; set; }

        public string? Department { get; set; }
        public string? AccessLevel { get; set; }

        public string? Specialization { get; set; }
        public string? Certification { get; set; }

        public string? ManagementLevel { get; set; }
        public int? YearsOfExperience { get; set; }

        public string? Picture { get; set; }
        public int? Id { get; set; }
    }
}