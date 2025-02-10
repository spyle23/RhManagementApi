using RhManagementApi.DTOs;
using RhManagementApi.Model;

namespace RhManagementApi.Repositories
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> CinExistsAsync(long cin);
        Task<T> CreateUserAsync<T>(T user) where T : User;

        Task<T> CreateEmployeeBasedAsync<T>(T employee) where T : Employee;

        Task<BasePaginationList<UserWithRoleDto>> GetUsersByFilters(int pageNumber, int pageSize, string? role, string? searchTerm);

        Task<Admin> UpdateAdminAsync(Admin admin);
        Task<Manager> UpdateManagerAsync(Manager manager);
        Task<RH> UpdateRHAsync(RH rh);
        Task<Employee> UpdateEmployeeAsync(Employee employee);

        Task<IEnumerable<UserDto>> GetAdminList();
        Task<IEnumerable<UserDto>> GetEmployeeList();
    }
}