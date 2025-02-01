using Microsoft.EntityFrameworkCore;
using Quartz;
using RhManagementApi.Data;

namespace RhManagementApi.Job
{
    public class EmployeeBalanceUpdateJob : IJob
    {
        private readonly ILogger<EmployeeBalanceUpdateJob> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public EmployeeBalanceUpdateJob(ILogger<EmployeeBalanceUpdateJob> logger,
        IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var currentDate = DateTime.UtcNow.Date;

                // Get all employees
                var employees = await dbContext.Employees.ToListAsync();
                var updatedCount = 0;

                foreach (var employee in employees)
                {
                    // Calculate if this employee should receive an update this month
                    if (ShouldUpdateBalance(employee.DateOfHiring, currentDate))
                    {
                        var oldHolidayBalance = employee.HolidayBalance;
                        var oldPermissionBalance = employee.BalancePermission;

                        // Add 2 days to holiday balance
                        employee.HolidayBalance += 2;

                        // Add 1 day to permission balance
                        employee.BalancePermission += 1;

                        updatedCount++;

                        _logger.LogInformation(
                            "Updated employee ID: {EmployeeId} (Hired: {HireDate}). " +
                            "Holiday Balance: {OldHoliday}->{NewHoliday}, " +
                            "Permission Balance: {OldPermission}->{NewPermission}",
                            employee.Id,
                            employee.DateOfHiring.ToString("dd-MM-yyyy"),
                            oldHolidayBalance,
                            employee.HolidayBalance,
                            oldPermissionBalance,
                            employee.BalancePermission
                        );
                    }
                }

                await dbContext.SaveChangesAsync();
                _logger.LogInformation(
                    "Completed balance updates for {Month}. Updated {Count} employees.",
                    currentDate.ToString("MMMM yyyy"),
                    updatedCount
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating employee balances");
                throw;
            }
        }

        private bool ShouldUpdateBalance(DateTime hiringDate, DateTime currentDate)
        {
            // If this is the first month after hiring, don't update
            if (hiringDate.Year == currentDate.Year && hiringDate.Month == currentDate.Month)
                return false;

            // Calculate months between hiring date and current date
            int monthsEmployed = ((currentDate.Year - hiringDate.Year) * 12) +
                               (currentDate.Month - hiringDate.Month);

            // Check if we've reached the monthly anniversary
            if (currentDate.Day >= hiringDate.Day)
            {
                // It's past or on their monthly anniversary day
                return true;
            }
            else if (IsLastDayOfMonth(currentDate) && currentDate.Day < hiringDate.Day)
            {
                // Handle edge case: if hiring date was 31st and current month has fewer days
                return true;
            }

            return false;
        }

        private bool IsLastDayOfMonth(DateTime date)
        {
            return date.Day == DateTime.DaysInMonth(date.Year, date.Month);
        }
    }
}