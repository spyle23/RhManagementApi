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
        private readonly ILogger<EmployeeRecordController> _logger;

        public EmployeeRecordController(IEmployeeRecordRepository employeeRecordRepository, PdfService pdfService, ILogger<EmployeeRecordController> logger)
        {
            _employeeRecordRepository = employeeRecordRepository;
            _pdfService = pdfService;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,RH")]
        public async Task<ActionResult<EmployeeRecordDto>> CreateEmployeeRecord([FromForm] CreateEmployeeRecordDto createEmployeeRecordDto, [FromForm] IFormFile? cvFile)
        {
            try
            {
                _logger.LogInformation("Starting CreateEmployeeRecord");
                
                if (createEmployeeRecordDto == null)
                {
                    _logger.LogWarning("Employee record data is null");
                    return BadRequest("Employee record data is required");
                }

                if (createEmployeeRecordDto.EmployeeId <= 0)
                {
                    _logger.LogWarning("Invalid EmployeeId: {EmployeeId}", createEmployeeRecordDto.EmployeeId);
                    return BadRequest("Invalid EmployeeId");
                }

                var cvPath = string.Empty;

                if (cvFile != null)
                {
                    _logger.LogInformation("Processing CV file upload");
                    
                    var uploadsFolder = Path.Combine("Uploads", "doc");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        _logger.LogInformation("Creating uploads directory: {UploadsFolder}", uploadsFolder);
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + cvFile.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    
                    _logger.LogInformation("Saving CV file to: {FilePath}", filePath);
                    
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await cvFile.CopyToAsync(stream);
                    }
                    cvPath = filePath;
                    
                    _logger.LogInformation("CV file saved successfully");
                }

                var employeeRecord = new EmployeeRecord
                {
                    Telephone = createEmployeeRecordDto.Telephone ?? string.Empty,
                    Adresse = createEmployeeRecordDto.Adresse ?? string.Empty,
                    Birthday = createEmployeeRecordDto.Birthday.ToUniversalTime(),
                    Poste = createEmployeeRecordDto.Poste ?? string.Empty,
                    Profil = createEmployeeRecordDto.Profil ?? string.Empty,
                    Status = EmployeeStatus.Active.ToDisplayValue(),
                    GrossSalary = createEmployeeRecordDto.GrossSalary,
                    Cv = cvPath,
                    EmployeeId = createEmployeeRecordDto.EmployeeId
                };

                _logger.LogInformation("Creating employee record in database");
                
                var createdRecord = await _employeeRecordRepository.CreateEmployeeRecordWithEmployeeAsync(employeeRecord);
                if (createdRecord == null)
                {
                    _logger.LogError("Failed to create employee record");
                    return BadRequest("Failed to create employee record");
                }

                _logger.LogInformation("Employee record created successfully with ID: {Id}", createdRecord.Id);
                
                return Ok(MapToDto(createdRecord));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateEmployeeRecord");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("Pdf/{id}")]
        public async Task<IActionResult> GetEmployeeRecordPdf(int id)
        {
            var record = await _employeeRecordRepository.GetEmployeeRecordByIdAsync(id);
            _logger.LogInformation("record {id}", record?.Id);
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
        public async Task<ActionResult<EmployeeRecordDto>> UpdateEmployeeRecord(int id, [FromForm] UpdateEmployeeRecordDto updateEmployeeRecordDto, [FromForm] IFormFile? cvFile)
        {
            try
            {
                _logger.LogInformation("Starting UpdateEmployeeRecord for ID: {Id}", id);
                
                var existingRecord = await _employeeRecordRepository.GetEmployeeRecordByIdAsync(id);
                if (existingRecord == null)
                {
                    _logger.LogWarning("Employee record not found with ID: {Id}", id);
                    return NotFound();
                }

                // Handle CV file upload if provided
                if (cvFile != null)
                {
                    _logger.LogInformation("Processing CV file upload for record ID: {Id}", id);
                    
                    var uploadsFolder = Path.Combine("Uploads", "doc");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        _logger.LogInformation("Creating uploads directory: {UploadsFolder}", uploadsFolder);
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + cvFile.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    
                    _logger.LogInformation("Saving CV file to: {FilePath}", filePath);
                    
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await cvFile.CopyToAsync(stream);
                    }
                    
                    // Delete old CV file if it exists
                    if (!string.IsNullOrEmpty(existingRecord.Cv) && System.IO.File.Exists(existingRecord.Cv))
                    {
                        _logger.LogInformation("Deleting old CV file: {OldCvPath}", existingRecord.Cv);
                        System.IO.File.Delete(existingRecord.Cv);
                    }
                    
                    existingRecord.Cv = filePath;
                    _logger.LogInformation("CV file updated successfully");
                }

                // Update other fields
                existingRecord.Telephone = updateEmployeeRecordDto.Telephone ?? existingRecord.Telephone;
                existingRecord.Adresse = updateEmployeeRecordDto.Adresse ?? existingRecord.Adresse;
                existingRecord.Birthday = updateEmployeeRecordDto.Birthday?.ToUniversalTime() ?? existingRecord.Birthday.ToUniversalTime();
                existingRecord.Poste = updateEmployeeRecordDto.Poste ?? existingRecord.Poste;
                existingRecord.Profil = updateEmployeeRecordDto.Profil ?? existingRecord.Profil;
                existingRecord.GrossSalary = updateEmployeeRecordDto.GrossSalary ?? existingRecord.GrossSalary;

                await _employeeRecordRepository.UpdateAsync(existingRecord);
                _logger.LogInformation("Employee record updated successfully for ID: {Id}", id);
                
                return Ok(MapToDto(existingRecord));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateEmployeeRecord for ID: {Id}", id);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Authorize]
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
                Cv = record.Cv,
                EmployeeId = record.EmployeeId,
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