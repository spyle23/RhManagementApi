using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RhManagementApi.DTOs;
using RhManagementApi.Model;
using RhManagementApi.Repositories;

namespace RhManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserWithRoleDto>> CreateUser([FromForm] CreateUserDto createUserDto, [FromForm] IFormFile? file)
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

            var pictureUser = string.Empty;

            if (file != null)
            {
                var filePath = Path.Combine("Uploads", file.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                pictureUser = filePath;
            }

            try
            {
                User user = createUserDto.Role switch
                {
                    "Admin" => await CreateAdminUser(createUserDto, passwordHash, pictureUser),
                    "RH" => await CreateRHUser(createUserDto, passwordHash, pictureUser),
                    "Manager" => await CreateManagerUser(createUserDto, passwordHash, pictureUser),
                    "Employee" => await CreateEmployeeUser(createUserDto, passwordHash, pictureUser),
                    _ => throw new ArgumentException("Invalid role")
                };

                return Ok(new UserWithRoleDto { Cin = user.Cin, Email = user.Email, FirstName = user.FirstName, Id = user.Id, LastName = user.LastName, Picture = user.Picture, Role = createUserDto.Role });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private async Task<Admin> CreateAdminUser(CreateUserDto dto, string passwordHash, string? pictureUser)
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
                Picture = pictureUser
            };

            return await _userRepository.CreateUserAsync(admin);
        }

        private async Task<RH> CreateRHUser(CreateUserDto dto, string passwordHash, string? pictureUser)
        {
            if (string.IsNullOrEmpty(dto.Specialization) || string.IsNullOrEmpty(dto.Certification))
            {
                throw new ArgumentException("Specialization and Certification are required for RHs");
            }

            if (!dto.DateOfHiring.HasValue)
            {
                throw new ArgumentException("Date of hiring is required for employees");
            }

            var hiringDate = ((DateTime)dto.DateOfHiring).ToUniversalTime();
            var currentDate = DateTime.UtcNow;

            // Calculate the number of months since hiring
            int monthsEmployed = ((currentDate.Year - hiringDate.Year) * 12) + 
                                (currentDate.Month - hiringDate.Month);

            // Adjust if the current day is before the hiring day in the month
            if (currentDate.Day < hiringDate.Day)
            {
                monthsEmployed--;
            }

            // Calculate balances
            int holidayBalance = Math.Max(0, monthsEmployed) * 2;
            int balancePermission = Math.Max(0, monthsEmployed) * 1;

            var rh = new RH
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Password = passwordHash,
                Cin = dto.Cin,
                Specialization = dto.Specialization,
                Certification = dto.Certification,
                HolidayBalance = holidayBalance,
                BalancePermission = balancePermission,
                DateOfHiring = hiringDate,
                Picture = pictureUser
            };

            return await _userRepository.CreateEmployeeBasedAsync(rh);
        }

        private async Task<Manager> CreateManagerUser(CreateUserDto dto, string passwordHash, string? pictureUser)
        {
            if (string.IsNullOrEmpty(dto.ManagementLevel) || dto.YearsOfExperience == null || dto.YearsOfExperience <= 0)
            {
                throw new ArgumentException("Management Level and Years of Experience are required for Managers");
            }

            if (!dto.DateOfHiring.HasValue)
            {
                throw new ArgumentException("Date of hiring is required for employees");
            }

            var hiringDate = ((DateTime)dto.DateOfHiring).ToUniversalTime();
            var currentDate = DateTime.UtcNow;

            // Calculate the number of months since hiring
            int monthsEmployed = ((currentDate.Year - hiringDate.Year) * 12) + 
                                (currentDate.Month - hiringDate.Month);

            // Adjust if the current day is before the hiring day in the month
            if (currentDate.Day < hiringDate.Day)
            {
                monthsEmployed--;
            }

            // Calculate balances
            int holidayBalance = Math.Max(0, monthsEmployed) * 2;
            int balancePermission = Math.Max(0, monthsEmployed) * 1;

            var manager = new Manager
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Password = passwordHash,
                Cin = dto.Cin,
                ManagementLevel = dto.ManagementLevel,
                YearsOfExperience = dto.YearsOfExperience.Value,
                HolidayBalance = holidayBalance,
                BalancePermission = balancePermission,
                DateOfHiring = hiringDate,
                Picture = pictureUser
            };

            return await _userRepository.CreateUserAsync(manager);
        }

        private async Task<Employee> CreateEmployeeUser(CreateUserDto dto, string passwordHash, string? pictureUser)
        {
            if (!dto.DateOfHiring.HasValue)
            {
                throw new ArgumentException("Date of hiring is required for employees");
            }

            var hiringDate = ((DateTime)dto.DateOfHiring).ToUniversalTime();
            var currentDate = DateTime.UtcNow;

            // Calculate the number of months since hiring
            int monthsEmployed = ((currentDate.Year - hiringDate.Year) * 12) + 
                                (currentDate.Month - hiringDate.Month);

            // Adjust if the current day is before the hiring day in the month
            if (currentDate.Day < hiringDate.Day)
            {
                monthsEmployed--;
            }

            // Calculate balances
            int holidayBalance = Math.Max(0, monthsEmployed) * 2;
            int balancePermission = Math.Max(0, monthsEmployed) * 1;

            var employee = new Employee
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Password = passwordHash,
                Cin = dto.Cin,
                DateOfHiring = hiringDate,
                TeamId = dto.TeamId,
                Picture = pictureUser,
                HolidayBalance = holidayBalance,
                BalancePermission = balancePermission
            };

            return await _userRepository.CreateUserAsync(employee);
        }

        private bool IsValidRole(string role)
        {
            return role is "Admin" or "RH" or "Manager" or "Employee";
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUserById(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var userDto = new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Cin = user.Cin,
                Email = user.Email,
                Picture = user.Picture
            };

            return Ok(userDto);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BasePaginationList<UserWithRoleDto>>> GetUsersFilters(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, string? role = null)
        {
            var usersQuery = await _userRepository.GetUsersByFilters(pageNumber, pageSize, role, searchTerm);
            return Ok(usersQuery);
        }

        [HttpPut("Update/{id}")]
        [Authorize]
        public async Task<ActionResult<UserWithRoleDto>> UpdateUser(int id, [FromForm] UpdateUserDto updateUserDto, [FromForm] IFormFile? file)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found");
            }

            // Handle file upload if provided
            if (file != null)
            {
                var filePath = Path.Combine("Uploads", file.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                user.Picture = filePath;
            }

            user.FirstName = updateUserDto.FirstName ?? user.FirstName;
            user.LastName = updateUserDto.LastName ?? user.LastName;
            user.Email = updateUserDto.Email ?? user.Email;
            user.Cin = updateUserDto.Cin ?? user.Cin;

            await _userRepository.UpdateAsync(user);

            if (user is Admin admin)
            {
                admin.Department = updateUserDto.Department ?? admin.Department;
                admin.AccessLevel = updateUserDto.AccessLevel ?? admin.AccessLevel;
                await _userRepository.UpdateAdminAsync(admin);
            }
            else if (user is Employee employee)
            {
                employee.DateOfHiring = updateUserDto.DateOfHiring != null ? ((DateTime)updateUserDto.DateOfHiring).ToUniversalTime() : employee.DateOfHiring;
                employee.TeamId = updateUserDto.TeamId ?? employee.TeamId;
                await _userRepository.UpdateEmployeeAsync(employee);

                if (employee is Manager manager)
                {
                    manager.ManagementLevel = updateUserDto.ManagementLevel ?? manager.ManagementLevel;
                    manager.YearsOfExperience = updateUserDto.YearsOfExperience ?? manager.YearsOfExperience;
                    await _userRepository.UpdateManagerAsync(manager);
                }
                else if (employee is RH rh)
                {
                    rh.Specialization = updateUserDto.Specialization ?? rh.Specialization;
                    rh.Certification = updateUserDto.Certification ?? rh.Certification;

                    await _userRepository.UpdateRHAsync(rh);
                }
            }


            return Ok(new UserWithRoleDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Cin = user.Cin,
                Picture = user.Picture,
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<string>> DeleteUser(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found");
            }

            try
            {
                await _userRepository.DeleteAsync(id);
                return Ok("User deleted successfully");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error deleting user: {ex.Message}");
            }
        }

        [HttpGet("Update/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CreateUserDto>> GetUserRolesById(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
            {
                return NotFound("User not found");
            }
            var userRoles = new CreateUserDto { Id = user.Id, FirstName = user.FirstName, Picture = user.Picture, Cin = user.Cin, Email = user.Email, LastName = user.LastName, Role = UserRepository.GetUserRole(user) };

            if (user is Admin admin)
            {
                userRoles.AccessLevel = admin.AccessLevel;
                userRoles.Department = admin.Department;
            }
            else if (user is Employee employee)
            {
                userRoles.DateOfHiring = employee.DateOfHiring;
                if (employee is Manager manager)
                {
                    userRoles.ManagementLevel = manager.ManagementLevel;
                    userRoles.YearsOfExperience = manager.YearsOfExperience;
                }
                else if (employee is RH rh)
                {
                    userRoles.Certification = rh.Certification;
                    userRoles.Specialization = rh.Specialization;
                }
            }

            return Ok(userRoles);

        }

        [HttpGet("Admin/List")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAdminList()
        {
            var admins = await _userRepository.GetAdminList();
            return Ok(admins);
        }

        [HttpGet("Employee/List")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetEmployeeList()
        {
            var employees = await _userRepository.GetEmployeeList();
            return Ok(employees);
        }

        [HttpGet("Manager/List")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetManagerList()
        {
            var managers = await _userRepository.GetManagerList();
            return Ok(managers);
        }

        [HttpGet("OnlyEmployees")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetOnlyEmployees(string? searchTerm = null)
        {
            var employees = await _userRepository.GetOnlyEmployeesAsync(searchTerm);
            return Ok(employees);
        }
    }
}