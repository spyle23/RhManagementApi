using System;
using System.ComponentModel.DataAnnotations;

namespace RhManagementApi.DTOs
{
    public class CreateEmployeeRecordDto
    {
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

        public decimal GrossSalary { get; set; }
        public int EmployeeId { get; set; }
    }

    public class UpdateEmployeeRecordDto
    {
        public string? Telephone { get; set; }
        public string? Adresse { get; set; }
        public DateTime? Birthday { get; set; }
        public string? Poste { get; set; }
        public string? Profil { get; set; }
        public decimal? GrossSalary { get; set; }
        public string? Cv { get; set; }
    }

    public class EmployeeRecordDto
    {
        public int Id { get; set; }
        public string Telephone { get; set; } = string.Empty;
        public string Adresse { get; set; } = string.Empty;
        public DateTime Birthday { get; set; }
        public string Poste { get; set; } = string.Empty;
        public string Profil { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal GrossSalary { get; set; }
        public string Cv { get; set; } = string.Empty;
        public int EmployeeId { get; set; }
        public EmployeeDto Employee { get; set; }
    }

    public class EmployeeDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Picture { get; set; } = string.Empty;
        public DateTime DateOfHiring { get; set; }
    }
}