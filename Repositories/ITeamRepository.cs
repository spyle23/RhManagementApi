using RhManagementApi.DTOs;
using RhManagementApi.Model;

namespace RhManagementApi.Repositories
{
    public interface ITeamRepository : IGenericRepository<Team>
    {
        Task<Team> AddEmployeeToTeam(int teamId, int employeeId);
        Task<Team> AddEmployeesToTeam(int teamId, IEnumerable<int> employeeIds);
        Task<BasePaginationList<TeamDto>> GetTeamFilters(int pageNumber, int pageSize, string? searchTerm);
        Task<IEnumerable<TeamMemberDto>> GetTeamMembers(int teamId);
        Task<bool> IsTeamEmpty(int teamId);
        Task<TeamDto?> GetByManagerIdAsync(int managerId);
        Task<TeamDto> CreateTeamWithManager(CreateTeamDto createTeamDto);
        Task<TeamDto> UpdateTeamAsync(int id, UpdateTeamDto updateTeamDto);
        Task<TeamDto?> GetByEmployeeIdAsync(int employeeId);
        Task<IEnumerable<TeamMemberDto>> GetTeamMembersByEmployeeIdAsync(int employeeId);
    }
}