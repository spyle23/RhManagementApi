using System.ComponentModel.DataAnnotations;

namespace RhManagementApi.DTOs
{
    public class BasePaginationList<T>
    {
        [Required]
        public ICollection<T> Datas { get; set; } = new List<T>();

        [Required]
        public int TotalPage { get; set; } = 0;
    }
}