using System.ComponentModel.DataAnnotations;

namespace RhManagementApi.Model
{
    public class Employee : User
    {
        [Required]
        public DateTime DateOfHiring { get; set; } = DateTime.MinValue;

        [Required]
        public int HolidayBalance { get; set; } = 0;

        [Required]
        public int BalancePermission { get; set; } = 0;

        public int? TeamId { get; set; }
        public virtual Team Team { get; set; }
    }
}
