using RhManagementApi.Model;

namespace RhManagementApi.Repositories
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User> GetByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> CinExistsAsync(long cin);
        Task<T> CreateUserAsync<T>(T user) where T : User;
    }
} 