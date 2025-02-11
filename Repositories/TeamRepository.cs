using Microsoft.EntityFrameworkCore;
using RhManagementApi.Data;
using RhManagementApi.DTOs;
using RhManagementApi.Model;

namespace RhManagementApi.Repositories
{
    public class TeamRepository : GenericRepository<Team>, ITeamRepository
    {
        public TeamRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Team> AddEmployeeToTeam(int teamId, int employeeId)
        {
            var team = await _context.Teams
                .Include(t => t.Employees)
                .FirstOrDefaultAsync(t => t.Id == teamId);
            
            if (team == null)
                throw new ArgumentException("Team not found");

            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null)
                throw new ArgumentException("Employee not found");

            team.Employees.Add(employee);
            await _context.SaveChangesAsync();
            return team;
        }

        public async Task<Team> AddEmployeesToTeam(int teamId, IEnumerable<int> employeeIds)
        {
            var team = await _context.Teams
                .Include(t => t.Employees)
                .FirstOrDefaultAsync(t => t.Id == teamId);
            
            if (team == null)
                throw new ArgumentException("Team not found");

            var employees = await _context.Employees
                .Where(e => employeeIds.Contains(e.Id))
                .ToListAsync();

            if (employees.Count != employeeIds.Count())
                throw new ArgumentException("One or more employees not found");

            foreach (var employee in employees)
            {
                if (team.Employees.Any(e => e.Id == employee.Id))
                    throw new ArgumentException($"Employee {employee.Id} is already in this team");

                if (employee.TeamId.HasValue && employee.TeamId != teamId)
                    throw new ArgumentException($"Employee {employee.Id} is already assigned to another team");

                team.Employees.Add(employee);
                employee.TeamId = teamId;
            }

            await _context.SaveChangesAsync();
            return team;
        }

        public async Task<BasePaginationList<TeamDto>> GetTeamFilters(int pageNumber, int pageSize, string? searchTerm)
        {
            var query = _context.Teams.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(t => 
                    t.Name.Contains(searchTerm) || 
                    t.Specialty.Contains(searchTerm));
            }

            var totalTeams = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalTeams / pageSize);

            var teams = await query
                .OrderBy(t => t.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TeamDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Specialty = t.Specialty,
                    ManagerId = t.ManagerId,
                    ManagerFirstName = t.Manager.FirstName,
                    ManagerLastName = t.Manager.LastName,
                    ManagerEmail = t.Manager.Email,
                    ManagerPicture = t.Manager.Picture ?? string.Empty,
                    MemberCount = t.Employees.Count,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt
                })
                .ToListAsync();

            return new BasePaginationList<TeamDto>
            {
                TotalPage = totalPages,
                Datas = teams
            };
        }

        public async Task<IEnumerable<TeamMemberDto>> GetTeamMembers(int teamId)
        {
            return await _context.Employees
                .Where(e => e.TeamId == teamId)
                .Join(_context.EmployeeRecords,
                    e => e.Id,
                    er => er.EmployeeId,
                    (e, er) => new TeamMemberDto
                    {
                        Id = e.Id,
                        FirstName = e.FirstName,
                        LastName = e.LastName,
                        Phone = er.Telephone,
                        Status = er.Status,
                        Email = e.Email,
                        Picture = e.Picture ?? string.Empty,
                        TeamId = e.TeamId
                    })
                .ToListAsync();
        }

        public async Task<bool> IsTeamEmpty(int teamId)
        {
            return !await _context.Employees
                .AnyAsync(e => e.TeamId == teamId);
        }

        public async Task<Team?> GetByManagerIdAsync(int managerId)
        {
            return await _context.Teams
                .Include(t => t.Employees)
                .FirstOrDefaultAsync(t => t.ManagerId == managerId);
        }

        public async Task<TeamDto> CreateTeamWithManager(CreateTeamDto createTeamDto)
        {
            // Get manager details
            var manager = await _context.Managers
                .FirstOrDefaultAsync(m => m.Id == createTeamDto.ManagerId);
            
            if (manager == null)
            {
                throw new ArgumentException("Manager not found");
            }

            // Create team
            var team = new Team
            {
                Name = createTeamDto.Name,
                Specialty = createTeamDto.Specialty,
                ManagerId = createTeamDto.ManagerId
            };

            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            // Return TeamDto with manager details
            return new TeamDto
            {
                Id = team.Id,
                Name = team.Name,
                Specialty = team.Specialty,
                ManagerId = team.ManagerId,
                ManagerFirstName = manager.FirstName,
                ManagerLastName = manager.LastName,
                ManagerEmail = manager.Email,
                ManagerPicture = manager.Picture ?? string.Empty,
                CreatedAt = team.CreatedAt,
                UpdatedAt = team.UpdatedAt,
                MemberCount = 0 // New team has no members
            };
        }

        public async Task<TeamDto> UpdateTeamAsync(int id, UpdateTeamDto updateTeamDto)
        {
            var team = await _context.Teams
                .Include(t => t.Manager)
                .Include(t => t.Employees)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (team == null)
            {
                throw new ArgumentException("Team not found");
            }

            if (!string.IsNullOrEmpty(updateTeamDto.Name))
            {
                team.Name = updateTeamDto.Name;
            }

            if (!string.IsNullOrEmpty(updateTeamDto.Specialty))
            {
                team.Specialty = updateTeamDto.Specialty;
            }

            if (updateTeamDto.ManagerId.HasValue)
            {
                var manager = await _context.Managers.FindAsync(updateTeamDto.ManagerId.Value);
                if (manager == null)
                {
                    throw new ArgumentException("Manager not found");
                }
                team.ManagerId = updateTeamDto.ManagerId.Value;
            }

            await _context.SaveChangesAsync();

            return new TeamDto
            {
                Id = team.Id,
                Name = team.Name,
                Specialty = team.Specialty,
                ManagerId = team.ManagerId,
                ManagerFirstName = team.Manager.FirstName,
                ManagerLastName = team.Manager.LastName,
                ManagerEmail = team.Manager.Email,
                ManagerPicture = team.Manager.Picture ?? string.Empty,
                MemberCount = team.Employees.Count,
                CreatedAt = team.CreatedAt,
                UpdatedAt = team.UpdatedAt
            };
        }
    }
}
