using System.ComponentModel.DataAnnotations;

namespace RhManagementApi.Model
{
    public class RH : Employee
    {
        [Required]
        public string Specialization { get; set; } = string.Empty;
        
        [Required]
        public string Certification { get; set; } = string.Empty;
    }
}
