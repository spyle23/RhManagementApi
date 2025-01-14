using System.ComponentModel.DataAnnotations;

namespace RhManagementApi.Model
{
    public class EmployeeRecord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Telephone { get; set; } = string.Empty;

        [Required]
        public string Adresse { get; set; } = string.Empty;

        [Required]
        public DateTime Birthday { get; set; }

        [Required]
        public string Poste { get; set; } = string.Empty;

        [Required]
        public string Profil { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = string.Empty;

        public string Cv { get; set; } = string.Empty;

        // Navigation property for the one-to-one relationship
        public int EmployeeId { get; set; }
        public virtual Employee Employee { get; set; }
    }
}