using RhManagementApi.Model;

namespace RhManagementApi.Repositories
{
    public interface ILeaveRepository : IGenericRepository<Leave>
    {
        // add some method
        Task<Leave?> GetLeaveWithEmployeeIdAsync(int id);
    }
} 