using System.ComponentModel.DataAnnotations;

namespace RhManagementApi.DTOs
{
    public class LeaveSoldDto
    {
        [Required]
        public int HolidayBalance { get; set; }

        [Required]
        public int BalancePermission { get; set; }
    }
}