using Microsoft.EntityFrameworkCore;
using RhManagementApi.Data;
using RhManagementApi.DTOs;
using RhManagementApi.Model;

namespace RhManagementApi.Repositories
{
    public class PayslipRepository : GenericRepository<Payslip>, IPayslipRepository
    {
        public PayslipRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<BasePaginationList<PayslipDto>> GetPayslipsByFilters(
            int pageNumber,
            int pageSize,
            int employeeId)
        {
            var query = _context.Payslips
                .Include(p => p.Employee)
                .Where(p => p.EmployeeId == employeeId);

            var totalPayslips = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalPayslips / pageSize);

            var payslips = await query
                .OrderByDescending(p => p.Month)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PayslipDto
                {
                    Id = p.Id,
                    EmployeeId = p.EmployeeId,
                    EmployeeName = $"{p.Employee.FirstName} {p.Employee.LastName}",
                    Month = p.Month,
                    GrossSalary = p.GrossSalary,
                    NetSalary = p.NetSalary,
                    Bonuses = p.Bonuses,
                    Overtime = p.Overtime
                })
                .ToListAsync();

            return new BasePaginationList<PayslipDto>
            {
                TotalPage = totalPages,
                Datas = payslips
            };
        }

        public async Task<Payslip?> GetPayslipByIdAsync(int id)
        {
            return await _context.Payslips
                .Include(p => p.Employee)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
    }
} 