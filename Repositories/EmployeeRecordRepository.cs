using Microsoft.EntityFrameworkCore;
using RhManagementApi.Data;
using RhManagementApi.DTOs;
using RhManagementApi.Model;

namespace RhManagementApi.Repositories
{
    public class EmployeeRecordRepository : GenericRepository<EmployeeRecord>, IEmployeeRecordRepository
    {
        public EmployeeRecordRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<BasePaginationList<EmployeeRecord>> GetEmployeeRecordsByFilters(int pageNumber, int pageSize, string? searchTerm, string? status)
        {
            var query = _context.EmployeeRecords
                .Include(er => er.Employee)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(er =>
                    er.Employee.FirstName.Contains(searchTerm) ||
                    er.Employee.LastName.Contains(searchTerm) ||
                    er.Poste.Contains(searchTerm) ||
                    er.Profil.Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(er => er.Status == status);
            }

            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            var records = await query
                .OrderBy(er => er.Employee.LastName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new BasePaginationList<EmployeeRecord>
            {
                TotalPage = totalPages,
                Datas = records
            };
        }

        public async Task<EmployeeRecord?> GetEmployeeRecordByIdAsync(int id)
        {
            return await _context.EmployeeRecords
                .Include(er => er.Employee)
                .FirstOrDefaultAsync(er => er.Id == id);
        }

        public async Task<EmployeeRecord> CreateEmployeeRecordWithEmployeeAsync(EmployeeRecord record)
        {
            // Include the Employee relation
            _context.EmployeeRecords.Add(record);
            await _context.SaveChangesAsync();

            // Explicitly load the Employee relation
            await _context.Entry(record)
                .Reference(r => r.Employee)
                .LoadAsync();

            return record;
        }
    }
}