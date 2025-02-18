using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RhManagementApi.DTOs;
using RhManagementApi.Model;
using RhManagementApi.Repositories;
using RhManagementApi.Services;
using System.Security.Claims;

namespace RhManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PayslipController : ControllerBase
    {
        private readonly IPayslipRepository _payslipRepository;
        private readonly PdfService _pdfService;
        private readonly IUserRepository _userRepository;

        public PayslipController(IPayslipRepository payslipRepository, PdfService pdfService, IUserRepository userRepository)
        {
            _payslipRepository = payslipRepository;
            _pdfService = pdfService;
            _userRepository = userRepository;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<BasePaginationList<PayslipDto>>> GetPayslipFilters(
            int pageNumber = 1,
            int pageSize = 10)
        {
            try
            {
                // Get employeeId from token
                var employeeId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                
                var payslips = await _payslipRepository.GetPayslipsByFilters(
                    pageNumber, 
                    pageSize, 
                    employeeId);
                    
                return Ok(payslips);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("Pdf/{id}")]
        [Authorize]
        public async Task<IActionResult> DownloadPayslipPdf(int id)
        {
            try
            {
                var payslip = await _payslipRepository.GetPayslipByIdAsync(id);
                if (payslip == null)
                {
                    return NotFound();
                }

                var pdfBytes = _pdfService.GeneratePayslipPdf(payslip);
                return File(pdfBytes, "application/pdf", $"Payslip_{id}.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<PayslipDto>> CreatePayslip(CreatePayslipDto createPayslipDto)
        {
            try
            {
                // Get employee from repository
                var employee = await _userRepository.GetByIdAsync(createPayslipDto.EmployeeId) as Employee;
                if (employee == null)
                {
                    return NotFound("Employee not found");
                }

                // Create new payslip
                var payslip = new Payslip
                {
                    EmployeeId = createPayslipDto.EmployeeId,
                    GrossSalary = createPayslipDto.GrossSalary,
                    NetSalary = createPayslipDto.GrossSalary * 0.80m, // Deduct 20% taxes
                    Bonuses = createPayslipDto.Bonuses,
                    Overtime = createPayslipDto.Overtime,
                    Month = createPayslipDto.Month
                };

                // Save payslip
                var createdPayslip = await _payslipRepository.AddAsync(payslip);

                // Return DTO
                var payslipDto = new PayslipDto
                {
                    Id = createdPayslip.Id,
                    EmployeeId = createdPayslip.EmployeeId,
                    EmployeeName = $"{employee.FirstName} {employee.LastName}",
                    Month = createdPayslip.Month,
                    GrossSalary = createdPayslip.GrossSalary,
                    NetSalary = createdPayslip.NetSalary,
                    Bonuses = createdPayslip.Bonuses,
                    Overtime = createdPayslip.Overtime
                };

                return Ok(new {message = "created"});
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
} 