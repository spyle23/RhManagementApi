using Microsoft.EntityFrameworkCore;
using RhManagementApi.Data;
using RhManagementApi.DTOs;
using RhManagementApi.Model;

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

        public async Task<IEnumerable<Leave>> GetLeavesByAdminId(int adminId)
        {
            return await _context.Leaves
                .Include(leave => leave.Employee)
                .ThenInclude(employee => employee as User)
                .Where(leave => leave.AdminId == adminId)
                .ToListAsync();
        }

        public async Task<BasePaginationList<ListLeavesDto>> GetLeavesByAdminFilters(int adminId, int pageNumber, int pageSize, string? searchTerm, string? status, string? type)
        {
            var leavesQuery = _context.Leaves
                .Include(leave => leave.Employee)
                .ThenInclude(employee => employee as User)
                .Where(leave => leave.AdminId == adminId && leave.RHStatus == "Approved");

            // Filter by status if specified
            if (!string.IsNullOrEmpty(status))
            {
                leavesQuery = leavesQuery.Where(leave => leave.Status == status);
            }

            // Filter by type if specified
            if (!string.IsNullOrEmpty(type))
            {
                leavesQuery = leavesQuery.Where(leave => leave.Type == type);
            }

            // Search by employee name if a search term is provided
            if (!string.IsNullOrEmpty(searchTerm))
            {
                leavesQuery = leavesQuery.Where(leave =>
                    leave.Employee.FirstName.Contains(searchTerm) ||
                    leave.Employee.LastName.Contains(searchTerm));
            }

            // Order by creation date
            leavesQuery = leavesQuery.OrderByDescending(leave => leave.CreatedAt);

            var totalLeaves = await leavesQuery.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalLeaves / pageSize);

            // Apply pagination and map to DTO
            var leaves = await leavesQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(leave => new ListLeavesDto
                {
                    Id = leave.Id,
                    StartDate = leave.StartDate,
                    EndDate = leave.EndDate,
                    Status = leave.Status,
                    Type = leave.Type,
                    FirstName = leave.Employee.FirstName,
                    LastName = leave.Employee.LastName
                })
                .ToListAsync();

            return new BasePaginationList<ListLeavesDto>
            {
                TotalPage = totalPages,
                Datas = leaves
            };
        }

        public async Task<BasePaginationList<ListLeavesDto>> GetMyLeavesFilters(int employeeId, int pageNumber, int pageSize, string? status, string? type)
        {
            var leavesQuery = _context.Leaves
                .Include(leave => leave.Employee)
                .ThenInclude(employee => employee as User)
                .Where(leave => leave.EmployeeId == employeeId);

            // Filter by status if specified
            if (!string.IsNullOrEmpty(status))
            {
                leavesQuery = leavesQuery.Where(leave => leave.Status == status);
            }

            // Filter by type if specified
            if (!string.IsNullOrEmpty(type))
            {
                leavesQuery = leavesQuery.Where(leave => leave.Type == type);
            }

            // Order by creation date
            leavesQuery = leavesQuery.OrderByDescending(leave => leave.CreatedAt);

            var totalLeaves = await leavesQuery.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalLeaves / pageSize);

            // Apply pagination and map to DTO
            var leaves = await leavesQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(leave => new ListLeavesDto
                {
                    Id = leave.Id,
                    StartDate = leave.StartDate,
                    EndDate = leave.EndDate,
                    Status = leave.Status,
                    Type = leave.Type,
                    FirstName = leave.Employee.FirstName,
                    LastName = leave.Employee.LastName
                })
                .ToListAsync();

            return new BasePaginationList<ListLeavesDto>
            {
                TotalPage = totalPages,
                Datas = leaves
            };
        }
    }
}