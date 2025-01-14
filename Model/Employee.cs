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
        public virtual Team Team { get; set; } = new Team();
        public virtual ICollection<Leave> Leaves { get; set; } = new List<Leave>();

        public int? RHId { get; set; }
        public virtual RH Rh { get; set; } = new RH();
    }
}
