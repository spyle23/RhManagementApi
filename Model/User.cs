using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace RhManagementApi.Model
{
    [Index(nameof(Cin), IsUnique = true)]
    [Index(nameof(Email), IsUnique = true)]
    public class User : BaseEntity
    {
        [Key]
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

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
