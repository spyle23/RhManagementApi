using System.ComponentModel.DataAnnotations;

namespace RhManagementApi.DTOs
{
    public class ActionLeaveDto
    {
        [Required]
        public string Status { get; set; } = string.Empty;
    }
}