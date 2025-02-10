using RhManagementApi.DTOs;
using RhManagementApi.Model;

namespace RhManagementApi.Repositories
{
    public interface IEmployeeRecordRepository : IGenericRepository<EmployeeRecord>
    {
        Task<BasePaginationList<EmployeeRecord>> GetEmployeeRecordsByFilters(int pageNumber, int pageSize, string? searchTerm, string? status);
        Task<EmployeeRecord?> GetEmployeeRecordByIdAsync(int id);
        Task<EmployeeRecord> CreateEmployeeRecordWithEmployeeAsync(EmployeeRecord record);
    }
} 