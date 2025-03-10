using Microsoft.EntityFrameworkCore;
using RhManagementApi.Data;
using RhManagementApi.DTOs;
using RhManagementApi.Enums;
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
                .Where(leave => leave.AdminId == adminId)
                .ToListAsync();
        }

        public async Task<BasePaginationList<ListLeavesDto>> GetLeavesByAdminFilters(int adminId, int pageNumber, int pageSize, string? searchTerm, string? status, string? type)
        {
            var leavesQuery = _context.Leaves
                .Include(leave => leave.Employee)
                .Where(leave => leave.AdminId == adminId && leave.RHStatus == RHStatus.Approved.ToDisplayValue());

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
                    RHStatus = leave.RHStatus,
                    Type = leave.Type,
                    FirstName = leave.Employee.FirstName,
                    LastName = leave.Employee.LastName,
                    Reason = leave.Reason
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
                    RHStatus = leave.RHStatus,
                    Type = leave.Type,
                    FirstName = leave.Employee.FirstName,
                    LastName = leave.Employee.LastName,
                    AdminId = leave.AdminId,
                    Reason = leave.Reason
                })
                .ToListAsync();

            return new BasePaginationList<ListLeavesDto>
            {
                TotalPage = totalPages,
                Datas = leaves
            };
        }

        public async Task<Leave> UpdateLeave(Leave leave)
        {
            _context.Leaves.Update(leave);
            await _context.SaveChangesAsync();
            return leave;
        }

        public async Task<BasePaginationList<ListLeavesDto>> GetTeamLeaves(int managerId, int pageNumber, int pageSize, string? status, string? type)
        {
            var leavesQuery = _context.Leaves
                .Include(l => l.Employee)
                .ThenInclude(e => e.Team)
                .Where(l => l.Employee.Team.ManagerId == managerId);

            // Filter by status if specified
            if (!string.IsNullOrEmpty(status))
            {
                leavesQuery = leavesQuery.Where(l => l.Status == status);
            }

            // Filter by type if specified
            if (!string.IsNullOrEmpty(type))
            {
                leavesQuery = leavesQuery.Where(l => l.Type == type);
            }

            // Order by creation date
            leavesQuery = leavesQuery.OrderByDescending(l => l.CreatedAt);

            var totalLeaves = await leavesQuery.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalLeaves / pageSize);

            var leaves = await leavesQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new ListLeavesDto
                {
                    Id = l.Id,
                    StartDate = l.StartDate,
                    EndDate = l.EndDate,
                    Status = l.Status,
                    RHStatus = l.RHStatus,
                    Type = l.Type,
                    FirstName = l.Employee.FirstName,
                    LastName = l.Employee.LastName,
                    Reason = l.Reason,
                    AdminId = l.AdminId
                })
                .ToListAsync();

            return new BasePaginationList<ListLeavesDto>
            {
                TotalPage = totalPages,
                Datas = leaves
            };
        }

        public async Task<BasePaginationList<ListLeavesDto>> GetEmployeeLeaves(int pageNumber, int pageSize, string? status, string? type)
        {
            var leavesQuery = _context.Leaves
                .Include(l => l.Employee)
                .Where(l => !(l.Employee is Manager) && !(l.Employee is RH)) // Exclude manager leaves
                .AsQueryable();

            // Filter by status if specified
            if (!string.IsNullOrEmpty(status))
            {
                leavesQuery = leavesQuery.Where(l => l.RHStatus == status);
            }

            // Filter by type if specified
            if (!string.IsNullOrEmpty(type))
            {
                leavesQuery = leavesQuery.Where(l => l.Type == type);
            }

            // Order by creation date
            leavesQuery = leavesQuery.OrderByDescending(l => l.CreatedAt);

            var totalLeaves = await leavesQuery.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalLeaves / pageSize);

            var leaves = await leavesQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new ListLeavesDto
                {
                    Id = l.Id,
                    StartDate = l.StartDate,
                    EndDate = l.EndDate,
                    Status = l.Status,
                    RHStatus = l.RHStatus,
                    Type = l.Type,
                    FirstName = l.Employee.FirstName,
                    LastName = l.Employee.LastName,
                    Reason = l.Reason,
                    AdminId = l.AdminId
                })
                .ToListAsync();

            return new BasePaginationList<ListLeavesDto>
            {
                TotalPage = totalPages,
                Datas = leaves
            };
        }

        public async Task<int> GetPendingLeavesCount()
        {
            return await _context.Leaves
                .CountAsync(l => l.Status == RHStatus.Pending.ToDisplayValue());
        }

        public async Task<int> GetPendingLeavesCountForMonth(DateTime date)
        {
            return await _context.Leaves
                .CountAsync(l => l.Status == RHStatus.Pending.ToDisplayValue() &&
                                l.CreatedAt!.Value.Year == date.Year &&
                                l.CreatedAt.Value.Month == date.Month);
        }

        public async Task<IEnumerable<Leave>> GetLeavesByEmployeeId(int employeeId)
        {
            return await _context.Leaves
                .Where(l => l.EmployeeId == employeeId)
                .ToListAsync();
        }
    }
}