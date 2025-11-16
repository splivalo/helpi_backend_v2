namespace Helpi.Infrastructure.Repositories;

using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class FacultyRepository : IFacultyRepository
{
        private readonly AppDbContext _context;

        public FacultyRepository(AppDbContext context) => _context = context;

        public async Task<Faculty> GetByIdAsync(int id) => await _context.Faculties.FindAsync(id);
        public async Task<Faculty?> GetByNameAsync(string name)
        {
                var faculties = await _context.Faculties.AsNoTracking().ToListAsync();

                return faculties.FirstOrDefault(f =>
                    f.Translations.Values.Any(t =>
                        string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase)));
        }

        public async Task<IEnumerable<Faculty>> GetAllAsync() => await _context.Faculties.ToListAsync();
        public async Task<Faculty> AddAsync(Faculty faculty)
        {
                await _context.Faculties.AddAsync(faculty);
                await _context.SaveChangesAsync();
                return faculty;
        }
        public async Task UpdateAsync(Faculty faculty)
        {
                _context.Faculties.Update(faculty);
                await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(Faculty faculty)
        {
                _context.Faculties.Remove(faculty);
                await _context.SaveChangesAsync();
        }
}