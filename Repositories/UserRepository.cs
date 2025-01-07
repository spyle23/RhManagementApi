using Microsoft.EntityFrameworkCore;
using RhManagementApi.Data;
using RhManagementApi.Model;

namespace RhManagementApi.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<User> GetByEmailAsync(string email)
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
    }
} 