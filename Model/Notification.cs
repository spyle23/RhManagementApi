using System.ComponentModel.DataAnnotations;

namespace RhManagementApi.Model
{
    public class Notification : BaseEntity
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public string Content { get; set; } = string.Empty;
        
        public int UserId { get; set; }
        public User User { get; set; }
    }
} 