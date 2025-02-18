namespace RhManagementApi.DTOs
{
    public class PayslipDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public DateTime Month { get; set; }
        public decimal GrossSalary { get; set; }
        public decimal NetSalary { get; set; }
        public decimal Bonuses { get; set; }
        public decimal Overtime { get; set; }
    }
} 