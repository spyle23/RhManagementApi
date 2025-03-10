using Microsoft.EntityFrameworkCore;
using RhManagementApi.Data;
using RhManagementApi.DTOs;
using RhManagementApi.Model;

namespace RhManagementApi.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> CinExistsAsync(long cin)
        {
            return await _context.Users.AnyAsync(u => u.Cin == cin);
        }

        public async Task<T> CreateUserAsync<T>(T user) where T : User
        {
            await _context.Set<T>().AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<T> CreateEmployeeBasedAsync<T>(T employee) where T : Employee
        {
            await this.CreateUserAsync(employee);
            await _context.Set<T>().AddAsync(employee);
            return employee;
        }

        public async Task<BasePaginationList<UserWithRoleDto>> GetUsersByFilters(int pageNumber, int pageSize, string? role, string? searchTerm)
        {
            var usersQuery = _context.Users.AsQueryable();

            // Filter by role if specified
            if (!string.IsNullOrEmpty(role))
            {
                switch (role)
                {
                    case "Admin":
                        usersQuery = usersQuery.OfType<Admin>();
                        break;
                    case "RH":
                        usersQuery = usersQuery.OfType<RH>();
                        break;
                    case "Manager":
                        usersQuery = usersQuery.OfType<Manager>();
                        break;
                    case "Employee":
                        usersQuery = usersQuery.OfType<Employee>().Where(u => !(u is Manager) && !(u is RH));
                        break;
                    default:
                        throw new ArgumentException("Invalid role specified");
                }
            }

            // Search by name if a search term is provided
            if (!string.IsNullOrEmpty(searchTerm))
            {
                usersQuery = usersQuery.Where(u =>
                    u.FirstName.Contains(searchTerm) ||
                    u.LastName.Contains(searchTerm));
            }

            usersQuery = usersQuery.OrderByDescending(u => u.CreatedAt);

            var totalUsers = await usersQuery.CountAsync();
            var totalPage = (int)Math.Ceiling((double)totalUsers / pageSize);

            // Apply pagination
            var users = await usersQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(user => new UserWithRoleDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Cin = user.Cin,
                    Email = user.Email,
                    Role = GetUserRole(user),
                    Picture = user.Picture
                })
                .ToListAsync();
            return new BasePaginationList<UserWithRoleDto> { TotalPage = totalPage, Datas = users };
        }
        public static string GetUserRole(User user)
        {
            if (user is Admin)
            {
                return "Admin";
            }
            else if (user is RH)
            {
                return "RH";
            }
            else if (user is Manager)
            {
                return "Manager";
            }
            else
            {
                return "Employee";
            }
        }

        public async Task<Admin> UpdateAdminAsync(Admin admin)
        {
            _context.Admins.Update(admin);
            await _context.SaveChangesAsync();
            return admin;
        }

        public async Task<Manager> UpdateManagerAsync(Manager manager)
        {
            _context.Managers.Update(manager);
            await _context.SaveChangesAsync();
            return manager;
        }

        public async Task<RH> UpdateRHAsync(RH rh)
        {
            _context.RHs.Update(rh);
            await _context.SaveChangesAsync();
            return rh;
        }

        public async Task<Employee> UpdateEmployeeAsync(Employee employee)
        {
            _context.Employees.Update(employee);
            await _context.SaveChangesAsync();
            return employee;
        }

        public async Task<IEnumerable<UserDto>> GetAdminList()
        {
            var users = _context.Users.Where((user) => user is Admin);
            return await users.Select((user) => new UserDto() { Cin = user.Cin, Email = user.Email, FirstName = user.FirstName, Id = user.Id, LastName = user.LastName, Picture = user.Picture }).ToListAsync();
        }

        public async Task<IEnumerable<UserDto>> GetEmployeeList()
        {
            var users = _context.Users.OfType<Employee>();
            return await users.Select(user => new UserDto() 
            { 
                Cin = user.Cin, 
                Email = user.Email, 
                FirstName = user.FirstName, 
                Id = user.Id, 
                LastName = user.LastName, 
                Picture = user.Picture 
            }).ToListAsync();
        }

        public async Task<IEnumerable<UserDto>> GetManagerList()
        {
            var managers = _context.Users.OfType<Manager>();
            return await managers.Select(manager => new UserDto()
            {
                Cin = manager.Cin,
                Email = manager.Email,
                FirstName = manager.FirstName,
                Id = manager.Id,
                LastName = manager.LastName,
                Picture = manager.Picture
            }).ToListAsync();
        }

        public async Task<IEnumerable<UserDto>> GetOnlyEmployeesAsync(string? searchTerm = null)
        {
            var query = _context.Users
                .OfType<Employee>()
                .Where(e => !(e is Manager) && !(e is RH));

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(e => 
                    e.FirstName.Contains(searchTerm) || 
                    e.LastName.Contains(searchTerm));
            }

            return await query
                .Select(e => new UserDto
                {
                    Id = e.Id,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    Cin = e.Cin,
                    Email = e.Email,
                    Picture = e.Picture
                })
                .ToListAsync();
        }

        public async Task<int> GetEmployeeCountForMonth(DateTime date)
        {
            return await _context.Users
                .OfType<Employee>()
                .CountAsync(e => e.DateOfHiring <= date);
        }
    }
}