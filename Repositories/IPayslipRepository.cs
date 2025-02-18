using RhManagementApi.DTOs;
using RhManagementApi.Model;

namespace RhManagementApi.Repositories
{
    public interface IPayslipRepository : IGenericRepository<Payslip>
    {
        Task<BasePaginationList<PayslipDto>> GetPayslipsByFilters(
            int pageNumber,
            int pageSize,
            int employeeId);

        Task<Payslip?> GetPayslipByIdAsync(int id);
    }
} 