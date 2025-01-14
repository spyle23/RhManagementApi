using Microsoft.EntityFrameworkCore;
using RhManagementApi.Data;
using RhManagementApi.Model;
using System.Threading.Tasks;

namespace RhManagementApi.Repositories
{
    public class LeaveRepository : GenericRepository<Leave>, ILeaveRepository
    {
        public LeaveRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Leave?> GetLeaveWithEmployeeIdAsync(int id)
        {
            return await _context.Leaves.Include(leave => leave.Employee)
            .ThenInclude(e => e.Team)
            .FirstOrDefaultAsync(leave => leave.Id == id);
        }
    }
}