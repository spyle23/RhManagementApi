using RhManagementApi.DTOs;
using RhManagementApi.Model;

namespace RhManagementApi.Repositories
{
    public interface ILeaveRepository : IGenericRepository<Leave>
    {
        // add some method
        Task<Leave?> GetLeaveWithEmployeeIdAsync(int id);
        Task<IEnumerable<Leave>> GetLeavesByAdminId(int adminId);
        Task<BasePaginationList<ListLeavesDto>> GetLeavesByAdminFilters(int adminId, int pageNumber, int pageSize, string? searchTerm, string? status, string? type);
        Task<BasePaginationList<ListLeavesDto>> GetMyLeavesFilters(int employeeId, int pageNumber, int pageSize, string? status, string? type);
        Task<BasePaginationList<ListLeavesDto>> GetTeamLeaves(int managerId, int pageNumber, int pageSize, string? status, string? type);
        Task<BasePaginationList<ListLeavesDto>> GetEmployeeLeaves(int pageNumber, int pageSize, string? status, string? type);

        Task<Leave> UpdateLeave(Leave leave);
    }
}