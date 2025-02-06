using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RhManagementApi.DTOs;
using RhManagementApi.Enums;
using RhManagementApi.Model;
using RhManagementApi.Repositories;
using RhManagementApi.Services;

namespace RhManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeRecordController : ControllerBase
    {
        private readonly IEmployeeRecordRepository _employeeRecordRepository;
        private readonly PdfService _pdfService;

        public EmployeeRecordController(IEmployeeRecordRepository employeeRecordRepository, PdfService pdfService)
        {
            _employeeRecordRepository = employeeRecordRepository;
            _pdfService = pdfService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,RH")]
        public async Task<ActionResult<EmployeeRecordDto>> CreateEmployeeRecord([FromForm] CreateEmployeeRecordDto createEmployeeRecordDto, [FromForm] IFormFile? cvFile)
        {
            try
            {
                var cvPath = string.Empty;

                // Handle CV file upload if provided
                if (cvFile != null)
                {
                    var uploadsFolder = Path.Combine("Uploads", "doc");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + cvFile.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await cvFile.CopyToAsync(stream);
                    }
                    cvPath = filePath;
                }

                var employeeRecord = new EmployeeRecord
                {
                    Telephone = createEmployeeRecordDto.Telephone,
                    Adresse = createEmployeeRecordDto.Adresse,
                    Birthday = createEmployeeRecordDto.Birthday,
                    Poste = createEmployeeRecordDto.Poste,
                    Profil = createEmployeeRecordDto.Profil,
                    Status = EmployeeStatus.Active.ToDisplayValue(),
                    GrossSalary = createEmployeeRecordDto.GrossSalary,
                    Cv = cvPath,
                    EmployeeId = createEmployeeRecordDto.EmployeeId
                };

                var createdRecord = await _employeeRecordRepository.AddAsync(employeeRecord);
                return Ok(MapToDto(createdRecord));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("/Pdf/{id}")]
        public async Task<IActionResult> GetEmployeeRecordPdf(int id)
        {
            var record = await _employeeRecordRepository.GetEmployeeRecordByIdAsync(id);
            if (record == null)
            {
                return NotFound();
            }

            var employeeRecord = MapToDto(record);
            employeeRecord.GrossSalary = record.GrossSalary;

            var pdfBytes = _pdfService.GenerateEmployeeRecordPdf(employeeRecord);
            return File(pdfBytes, "application/pdf", $"EmployeeRecord_{id}.pdf");
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,RH")]
        public async Task<ActionResult<EmployeeRecordDto>> UpdateEmployeeRecord(int id, UpdateEmployeeRecordDto updateEmployeeRecordDto)
        {
            try
            {
                var existingRecord = await _employeeRecordRepository.GetByIdAsync(id);
                if (existingRecord == null)
                {
                    return NotFound();
                }

                existingRecord.Telephone = updateEmployeeRecordDto.Telephone ?? existingRecord.Telephone;
                existingRecord.Adresse = updateEmployeeRecordDto.Adresse ?? existingRecord.Adresse;
                existingRecord.Birthday = updateEmployeeRecordDto.Birthday ?? existingRecord.Birthday;
                existingRecord.Poste = updateEmployeeRecordDto.Poste ?? existingRecord.Poste;
                existingRecord.Profil = updateEmployeeRecordDto.Profil ?? existingRecord.Profil;

                await _employeeRecordRepository.UpdateAsync(existingRecord);
                return Ok(MapToDto(existingRecord));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,RH")]
        public async Task<ActionResult<BasePaginationList<EmployeeRecordDto>>> GetEmployeeRecords(
            int pageNumber = 1,
            int pageSize = 10,
            string? searchTerm = null,
            string? status = null)
        {
            try
            {
                var records = await _employeeRecordRepository.GetEmployeeRecordsByFilters(pageNumber, pageSize, searchTerm, status);
                var mappedRecords = new BasePaginationList<EmployeeRecordDto>
                {
                    TotalPage = records.TotalPage,
                    Datas = records.Datas.Select(MapToDto).ToList()
                };
                return Ok(mappedRecords);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<EmployeeRecordDto>> GetEmployeeRecordById(int id)
        {
            try
            {
                var record = await _employeeRecordRepository.GetEmployeeRecordByIdAsync(id);
                if (record == null)
                {
                    return NotFound();
                }

                var dto = MapToDto(record);
                dto.GrossSalary = record.GrossSalary;
                dto.Cv = record.Cv;
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private static EmployeeRecordDto MapToDto(EmployeeRecord record)
        {
            return new EmployeeRecordDto
            {
                Id = record.Id,
                Telephone = record.Telephone,
                Adresse = record.Adresse,
                Birthday = record.Birthday,
                Poste = record.Poste,
                Profil = record.Profil,
                Status = record.Status,
                Employee = new EmployeeDto
                {
                    Id = record.Employee.Id,
                    FirstName = record.Employee.FirstName,
                    LastName = record.Employee.LastName,
                    Email = record.Employee.Email,
                    DateOfHiring = record.Employee.DateOfHiring,
                    Picture = record.Employee?.Picture
                }
            };
        }
    }
}