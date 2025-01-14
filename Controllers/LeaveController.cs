using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RhManagementApi.DTOs;
using RhManagementApi.Model;
using RhManagementApi.Repositories;
using RhManagementApi.Enums;
using System.Security.Claims;

namespace RhManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaveController : ControllerBase
    {
        private readonly ILeaveRepository _leaveRepository;

        public LeaveController(ILeaveRepository leaveRepository)
        {
            _leaveRepository = leaveRepository;
        }

        [HttpPost]
        [Authorize(Roles = "Employee,Manager,RH")]
        public async Task<ActionResult<Leave>> CreateLeave(CreateLeaveDto createLeaveDto)
        {

            var leave = new Leave
            {
                StartDate = createLeaveDto.StartDate,
                EndDate = createLeaveDto.EndDate,
                Status = RHStatus.Pending.ToString(), // Convert enum to string if needed
                Type = createLeaveDto.Type,
                RHStatus = RHStatus.Pending.ToString(), // Convert enum to string if needed
                EmployeeId = createLeaveDto.EmployeeId,
                AdminId = createLeaveDto.AdminId
            };

            var createdLeave = await _leaveRepository.AddAsync(leave);
            return CreatedAtAction(nameof(GetLeaveById), new { id = createdLeave.Id }, createdLeave);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Leave>> GetLeaveById(int id)
        {
            var leave = await _leaveRepository.GetByIdAsync(id);
            if (leave == null)
            {
                return NotFound();
            }
            return Ok(leave);
        }

        [HttpPut("{id}/validate")]
        [Authorize(Roles = "Manager,RH")]
        public async Task<ActionResult<Leave>> ValidateRHLeave(int id, string status)
        {
            var leave = await _leaveRepository.GetLeaveWithEmployeeIdAsync(id);
            if (leave == null)
            {
                return NotFound();
            }

            // Check if the user is authorized to validate the leave
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            if (int.TryParse(userId, out int userIdNumber))
            {
                if (userRole == "Manager")
                {
                    // logic for manager
                    if (userIdNumber != leave.Employee.Team.ManagerId)
                    {
                        return Forbid("User cannot validate this leave");
                    }
                }
                else
                {
                    // logic for RH
                    if (userIdNumber != leave.Employee.RHId)
                    {
                        return Forbid("User cannot validate this leave");
                    }
                }
            }

            // Update the leave status
            leave.RHStatus = status;
            await _leaveRepository.UpdateAsync(leave);

            return Ok(leave);
        }

        [HttpPut("{id}/validate/admin")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Leave>> ValidateAdminLeave(int id, string status)
        {
            var leave = await _leaveRepository.GetLeaveWithEmployeeIdAsync(id);
            if (leave == null)
            {
                return NotFound();
            }

            // Check if the user is authorized to validate the leave
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userId, out int userIdNumber))
            {
                if (userIdNumber != leave.AdminId)
                {
                    return Forbid("User cannot validate this leave");
                }
            }
            leave.RHStatus = status;
            await _leaveRepository.UpdateAsync(leave);

            return Ok(leave);
        }
    }
}