using System.ComponentModel.DataAnnotations;

namespace RhManagementApi.Model
{
    public class File : BaseEntity
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public string Mime { get; set; } = string.Empty;
        
        [Required]
        public string Type { get; set; } = string.Empty;
    }
} 