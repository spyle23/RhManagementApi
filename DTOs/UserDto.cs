using System.ComponentModel.DataAnnotations;

namespace RhManagementApi.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public long Cin { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? Picture { get; set; } = string.Empty;
    }
} 