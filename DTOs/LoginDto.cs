using System.ComponentModel.DataAnnotations;

namespace RhManagementApi.DTOs
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class AuthReturnDto
    {
        [Required]
        public string Token { get; set; } = string.Empty;
        [Required]
        public string Role { get; set; } = string.Empty;

        [Required]
        public int UserId { get; set; }
    }
} 