namespace Helpi.Infrastructure.Repositories;

using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class UserRepository : IUserRepository
{
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
                _context = context;
        }

        public async Task<User> GetByIdAsync(int id) => await _context.Users.FindAsync(id);
        public async Task<User> GetByEmailAsync(string email) => await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        public async Task<IEnumerable<User>> GetAllAsync() => await _context.Users.ToListAsync();
        public async Task<User> AddAsync(User user)
        {
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();
                return user;
        }
        public async Task UpdateAsync(User user)
        {
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(User user)
        {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
        }
}
