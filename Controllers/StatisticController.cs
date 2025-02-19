using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RhManagementApi.Repositories;
using System;
using System.Threading.Tasks;

namespace RhManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StatisticController : ControllerBase
    {
        private readonly ILeaveRepository _leaveRepository;
        private readonly IUserRepository _userRepository;

        public StatisticController(ILeaveRepository leaveRepository, IUserRepository userRepository)
        {
            _leaveRepository = leaveRepository;
            _userRepository = userRepository;
        }

        [HttpGet("leave-stats")]
        [Authorize(Roles = "Admin,RH")]
        public async Task<ActionResult<object>> GetLeaveStatistics()
        {
            try
            {
                var currentDate = DateTime.UtcNow;
                var lastMonthDate = currentDate.AddMonths(-1);

                // Get pending leaves counts
                var currentPendingLeavesCount = await _leaveRepository.GetPendingLeavesCountForMonth(currentDate);
                var lastMonthPendingLeavesCount = await _leaveRepository.GetPendingLeavesCountForMonth(lastMonthDate);

                // Calculate pending leaves rate
                var pendingLeavesRate = lastMonthPendingLeavesCount == 0 
                    ? 0 
                    : ((currentPendingLeavesCount - lastMonthPendingLeavesCount) / (double)lastMonthPendingLeavesCount) * 100;

                // Get employee counts
                var currentMonthCount = await _userRepository.GetEmployeeCountForMonth(currentDate);
                var lastMonthCount = await _userRepository.GetEmployeeCountForMonth(lastMonthDate);

                // Calculate employee growth rate
                var employeeGrowthRate = lastMonthCount == 0 
                    ? 0 
                    : ((currentMonthCount - lastMonthCount) / (double)lastMonthCount) * 100;

                return Ok(new
                {
                    CurrentPendingLeavesCount = currentPendingLeavesCount,
                    LastMonthPendingLeavesCount = lastMonthPendingLeavesCount,
                    PendingLeavesRate = Math.Round(pendingLeavesRate, 2),
                    CurrentMonthEmployeeCount = currentMonthCount,
                    LastMonthEmployeeCount = lastMonthCount,
                    EmployeeGrowthRate = Math.Round(employeeGrowthRate, 2)
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
} 