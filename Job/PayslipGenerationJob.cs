using Microsoft.EntityFrameworkCore;
using Quartz;
using RhManagementApi.Data;
using RhManagementApi.Model;

namespace RhManagementApi.Job
{
    public class PayslipGenerationJob : IJob
    {
        private readonly ILogger<PayslipGenerationJob> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public PayslipGenerationJob(
            ILogger<PayslipGenerationJob> logger,
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

                // Get all active employees with proper includes
                var employees = await dbContext.EmployeeRecords
                    .Include(a => a.Employee)
                    .Where(a => a.Employee != null)  // Ensure we only get records with valid employees
                    .ToListAsync();

                _logger.LogInformation("Starting payslip generation for {Count} employees", employees.Count);

                foreach (var employee in employees)
                {
                    var payslip = GeneratePayslip(employee);
                    await SavePayslip(payslip, dbContext);

                    _logger.LogInformation(
                        "Generated payslip for employee {EmployeeId} for period {Period}",
                        employee.Id,
                        DateTime.UtcNow.ToString("MMMM yyyy"));
                }

                await dbContext.SaveChangesAsync();
                _logger.LogInformation("Completed payslip generation for all employees");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating payslips");
                throw;
            }
        }

        private Payslip GeneratePayslip(EmployeeRecord employee)
        {
            // Get the current date in UTC
            var currentDate = DateTime.UtcNow;

            // Get the first day of the current month in UTC
            var firstDayOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            // Check if the employee was hired in the same month as the current date
            bool isHiredInCurrentMonth = employee.Employee.DateOfHiring.Year == currentDate.Year &&
                                         employee.Employee.DateOfHiring.Month == currentDate.Month;

            decimal grossSalary;

            if (isHiredInCurrentMonth)
            {
                // Employee was hired in the current month: calculate proportional salary
                var startDate = employee.Employee.DateOfHiring > firstDayOfMonth ? 
                    DateTime.SpecifyKind(employee.Employee.DateOfHiring, DateTimeKind.Utc) : 
                    firstDayOfMonth;

                // Calculate the number of days worked in the current month
                var daysInMonth = DateTime.DaysInMonth(currentDate.Year, currentDate.Month);
                var daysWorked = (currentDate - startDate).Days + 1; // +1 to include the start day

                // Ensure daysWorked does not exceed the total days in the month
                daysWorked = Math.Min(daysWorked, daysInMonth);

                // Calculate the proportional gross salary
                var dailySalary = employee.GrossSalary / daysInMonth;
                grossSalary = dailySalary * daysWorked;
            }
            else
            {
                // Employee was hired in a previous month: full salary for the current month
                grossSalary = employee.GrossSalary;
            }
            // Calculate net salary (deduct 20% taxes)
            decimal taxRate = 0.20m; // 20%
            decimal netSalary = grossSalary * (1 - taxRate);

            // Create payslip model with UTC DateTime
            var payslip = new Payslip
            {
                EmployeeId = employee.EmployeeId,
                GrossSalary = grossSalary,
                NetSalary = netSalary,
                Bonuses = 0,
                Month = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 0, 0, 0, DateTimeKind.Utc),
            };

            return payslip;
        }

        private async Task SavePayslip(Payslip payslip, ApplicationDbContext dbContext)
        {
            dbContext.Payslips.Add(payslip);
            await dbContext.SaveChangesAsync();
        }
    }
}