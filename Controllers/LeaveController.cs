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
        private readonly IUserRepository _userRepository;

        public LeaveController(ILeaveRepository leaveRepository, IUserRepository userRepository)
        {
            _leaveRepository = leaveRepository;
            _userRepository = userRepository;
        }

        [HttpGet("admin/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BasePaginationList<ListLeavesDto>>> GetLeavesByAdminFilters(
            int id,
            int pageNumber = 1,
            int pageSize = 10,
            string? searchTerm = null,
            string? status = null,
            string? type = null)
        {
            try
            {
                var leaves = await _leaveRepository.GetLeavesByAdminFilters(id, pageNumber, pageSize, searchTerm, status, type);
                return Ok(leaves);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Employee,Manager,RH")]
        public async Task<ActionResult<ListLeavesDto>> CreateLeave(CreateLeaveDto createLeaveDto)
        {
            var user = await _userRepository.GetByIdAsync(createLeaveDto.EmployeeId);
            if (user == null)
            {
                return BadRequest("User does not exist");
            }
            var leave = new Leave
            {
                StartDate = createLeaveDto.StartDate.ToUniversalTime(),
                EndDate = createLeaveDto.EndDate.ToUniversalTime(),
                Status = RHStatus.Pending.ToDisplayValue(), // Convert enum to string if needed
                Type = createLeaveDto.Type,
                RHStatus = RHStatus.Pending.ToDisplayValue(), // Convert enum to string if needed
                EmployeeId = createLeaveDto.EmployeeId,
                Reason = createLeaveDto.Reason,
                AdminId = createLeaveDto.AdminId
            };

            var duration = leave.EndDate - leave.StartDate;
            var days = duration.Days;


            if (user is Employee employee)
            {
                if (leave.Type == RHType.holiday.ToDisplayValue())
                {
                    employee.HolidayBalance -= days;
                }
                else
                {
                    employee.BalancePermission -= days;
                }

                // vérifier si la demande est correcte
                if (employee.BalancePermission < 0 || employee.HolidayBalance < 0)
                {
                    return BadRequest("Sold inssufisant");
                }

                // Mise à jour des soldes si aucun erreur
                await _userRepository.UpdateEmployeeAsync(employee);
            }

            if (createLeaveDto.Id != null)
            {
                leave.Id = (int)createLeaveDto.Id;
            }

            var createdLeave = createLeaveDto.Id != null ? await _leaveRepository.UpdateLeave(leave) : await _leaveRepository.AddAsync(leave);
            return Ok(new ListLeavesDto() { Id = createdLeave.Id, EndDate = createdLeave.EndDate, FirstName = createdLeave.Employee.FirstName, LastName = createdLeave.Employee.LastName, Reason = createdLeave.Reason, RHStatus = createdLeave.RHStatus, Status = createdLeave.Status, Type = createdLeave.Type, StartDate = createdLeave.StartDate, AdminId = createdLeave.AdminId });
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
            }

            // Update the leave status
            leave.RHStatus = status;
            if (status == RHStatus.Rejected.ToDisplayValue())
            {
                leave.Status = status;
            }
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

        [HttpGet("my-leaves")]
        [Authorize(Roles = "Employee,Manager,RH")]
        public async Task<ActionResult<BasePaginationList<ListLeavesDto>>> MyLeavesFilter(
            int pageNumber = 1,
            int pageSize = 10,
            string? status = null,
            string? type = null)
        {
            try
            {
                var employeeId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var leaves = await _leaveRepository.GetMyLeavesFilters(employeeId, pageNumber, pageSize, status, type);
                return Ok(leaves);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("Employee/Sold")]
        [Authorize(Roles = "Employee,Manager,RH")]
        public async Task<ActionResult<LeaveSoldDto>> GetMySold()
        {
            var employeeId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _userRepository.GetByIdAsync(employeeId);
            if (user is Employee employee)
            {
                return Ok(new LeaveSoldDto() { BalancePermission = employee.BalancePermission, HolidayBalance = employee.HolidayBalance });
            }

            return BadRequest("User is not employee");
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Employee,Manager,RH")]
        public async Task<ActionResult> DeleteLeave(int id)
        {
            var leave = await _leaveRepository.GetByIdAsync(id);
            if (leave == null)
            {
                return NotFound("leave does not exist");
            }

            var user = await _userRepository.GetByIdAsync(leave.EmployeeId);
            if (user == null)
            {
                return NotFound("user does not exist");
            }

            await _leaveRepository.DeleteAsync(id);

            var duration = leave.EndDate - leave.StartDate;
            var days = duration.Days;

            if (user is Employee employee)
            {
                if (leave.Type == RHType.holiday.ToDisplayValue())
                {
                    employee.HolidayBalance += days;
                }
                else
                {
                    employee.BalancePermission += days;
                }

                // Mise à jour des soldes si aucun erreur
                await _userRepository.UpdateEmployeeAsync(employee);
            }
            return Ok(new { message = "Leave was deleted" });
        }
    }
}