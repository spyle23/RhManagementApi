using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RhManagementApi.DTOs;
using RhManagementApi.Model;
using RhManagementApi.Repositories;
using System.Security.Claims;

namespace RhManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager")]
    public class TeamController : ControllerBase
    {
        private readonly ITeamRepository _teamRepository;

        public TeamController(ITeamRepository teamRepository)
        {
            _teamRepository = teamRepository;
        }

        [HttpPost]
        public async Task<ActionResult<TeamDto>> CreateTeam(CreateTeamDto createTeamDto)
        {
            try
            {
                var teamDto = await _teamRepository.CreateTeamWithManager(createTeamDto);
                return CreatedAtAction(nameof(GetTeam), new { id = teamDto.Id }, teamDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("{teamId}/add-employees")]
        public async Task<ActionResult<Team>> AddEmployeesToTeam(int teamId, [FromBody] IEnumerable<int> employeeIds)
        {
            try
            {
                await _teamRepository.AddEmployeesToTeam(teamId, employeeIds);
                return Ok(new { message = "added" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<ActionResult<BasePaginationList<TeamDto>>> GetTeamFilters(
            int pageNumber = 1,
            int pageSize = 10,
            string? searchTerm = null)
        {
            var teams = await _teamRepository.GetTeamFilters(pageNumber, pageSize, searchTerm);
            return Ok(teams);
        }

        [HttpGet("{teamId}/members")]
        public async Task<ActionResult<IEnumerable<TeamMemberDto>>> GetTeamMembers(int teamId)
        {
            try
            {
                var members = await _teamRepository.GetTeamMembers(teamId);
                return Ok(members);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpDelete("{teamId}")]
        public async Task<ActionResult> DeleteTeam(int teamId)
        {
            try
            {
                if (!await _teamRepository.IsTeamEmpty(teamId))
                {
                    return BadRequest("Cannot delete team with members");
                }

                await _teamRepository.DeleteAsync(teamId);
                return Ok(new { message = "Team deleted" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Team>> GetTeam(int id)
        {
            var team = await _teamRepository.GetByIdAsync(id);
            if (team == null)
            {
                return NotFound();
            }
            return Ok(team);
        }

        [HttpGet("my-team")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<Team>> GetMyTeam()
        {
            // Get manager ID from token
            var managerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Get team by manager ID
            var team = await _teamRepository.GetByManagerIdAsync(managerId);

            if (team == null)
            {
                return NotFound("No team found for this manager");
            }

            return Ok(team);
        }

        [HttpGet("my-team/members")]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<IEnumerable<TeamMemberDto>>> GetMyTeamMembers()
        {
            // Get manager ID from token
            var managerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Get team by manager ID
            var team = await _teamRepository.GetByManagerIdAsync(managerId);

            if (team == null)
            {
                return NotFound("No team found for this manager");
            }

            // Get team members
            var members = await _teamRepository.GetTeamMembers(team.Id);
            return Ok(members);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<TeamDto>> UpdateTeam(int id, UpdateTeamDto updateTeamDto)
        {
            try
            {
                var teamDto = await _teamRepository.UpdateTeamAsync(id, updateTeamDto);
                return Ok(teamDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
