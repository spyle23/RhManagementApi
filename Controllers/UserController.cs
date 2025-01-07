using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RhManagementApi.DTOs;
using RhManagementApi.Model;
using RhManagementApi.Repositories;

namespace RhManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpPost]
        public async Task<ActionResult<User>> CreateUser(CreateUserDto createUserDto)
        {
            // Validate role
            if (!IsValidRole(createUserDto.Role))
            {
                return BadRequest("Invalid role specified");
            }

            // Check if email or CIN already exists
            if (await _userRepository.EmailExistsAsync(createUserDto.Email))
            {
                return BadRequest("Email already exists");
            }

            if (await _userRepository.CinExistsAsync(createUserDto.Cin))
            {
                return BadRequest("CIN already exists");
            }

            // Hash password
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password);

            try
            {
                User user = createUserDto.Role switch
                {
                    "Admin" => await CreateAdminUser(createUserDto, passwordHash),
                    "RH" => await CreateRHUser(createUserDto, passwordHash),
                    "Manager" => await CreateManagerUser(createUserDto, passwordHash),
                    "Employee" => await CreateEmployeeUser(createUserDto, passwordHash),
                    _ => throw new ArgumentException("Invalid role")
                };

                return Ok(new { message = "User created successfully", userId = user.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private async Task<Admin> CreateAdminUser(CreateUserDto dto, string passwordHash)
        {
            if (string.IsNullOrEmpty(dto.Department) || string.IsNullOrEmpty(dto.AccessLevel))
            {
                throw new ArgumentException("Department and Access Level are required for Admins");
            }

            var admin = new Admin
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Password = passwordHash,
                Cin = dto.Cin,
                Department = dto.Department,
                AccessLevel = dto.AccessLevel,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            return await _userRepository.CreateUserAsync(admin);
        }

        private async Task<RH> CreateRHUser(CreateUserDto dto, string passwordHash)
        {
            if (string.IsNullOrEmpty(dto.Specialization) || string.IsNullOrEmpty(dto.Certification))
            {
                throw new ArgumentException("Specialization and Certification are required for RHs");
            }

            var rh = new RH
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Password = passwordHash,
                Cin = dto.Cin,
                Specialization = dto.Specialization,
                Certification = dto.Certification,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            return await _userRepository.CreateUserAsync(rh);
        }

        private async Task<Manager> CreateManagerUser(CreateUserDto dto, string passwordHash)
        {
            if (string.IsNullOrEmpty(dto.ManagementLevel) || dto.YearsOfExperience == null || dto.YearsOfExperience <= 0)
            {
                throw new ArgumentException("Management Level and Years of Experience are required for Managers");
            }

            var manager = new Manager
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Password = passwordHash,
                Cin = dto.Cin,
                ManagementLevel = dto.ManagementLevel,
                YearsOfExperience = dto.YearsOfExperience.Value,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            return await _userRepository.CreateUserAsync(manager);
        }

        private async Task<Employee> CreateEmployeeUser(CreateUserDto dto, string passwordHash)
        {
            if (!dto.DateOfHiring.HasValue)
            {
                throw new ArgumentException("Date of hiring is required for employees");
            }

            var employee = new Employee
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Password = passwordHash,
                Cin = dto.Cin,
                DateOfHiring = dto.DateOfHiring.Value,
                TeamId = dto.TeamId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                HolidayBalance = 0,
                BalancePermission = 0
            };

            return await _userRepository.CreateUserAsync(employee);
        }

        private bool IsValidRole(string role)
        {
            return role is "Admin" or "RH" or "Manager" or "Employee";
        }
    }
} 