namespace Helpi.Infrastructure.Repositories;

using System;
using System.Linq.Expressions;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Helpi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class UserRepository : IUserRepository
{
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
                _context = context;
        }

        public async Task<User?> GetByIdAsync(int id) => await _context.Users.FindAsync(id);

        public async Task<User?> GetByIdWithContactAsync(int id) => await _context.Users
                .Include(u => u.Student)
                    .ThenInclude(s => s!.Contact)
                .Include(u => u.Customer)
                    .ThenInclude(c => c!.Contact)
                .FirstOrDefaultAsync(u => u.Id == id);

        public async Task<User?> GetByEmailAsync(string email) => await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
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

        public Task<int> CountAsync(Expression<Func<User, bool>> predicate)
        {
                return _context.Users.CountAsync(predicate);
        }

        public async Task<string> AnonymizeAndLogoutUserAsync(int userId)
        {
                var user = await _context.Users.FindAsync(userId)
                        ?? throw new InvalidOperationException($"User with ID {userId} not found");

                var originalUserName = user.UserName ?? "Unknown";

                // Anonymize user data
                user.Email = $"deleted_{userId}@deleted.local";
                user.UserName = $"deleted_{userId}";
                user.NormalizedEmail = $"DELETED_{userId}@DELETED.LOCAL";
                user.NormalizedUserName = $"DELETED_{userId}";
                user.PhoneNumber = null;
                user.PasswordHash = null;

                // Regenerate security stamp to invalidate all sessions (logout)
                user.SecurityStamp = Guid.NewGuid().ToString();

                await _context.SaveChangesAsync();

                return originalUserName;
        }

        public async Task<List<int>> GetAdminIdsAsync()
        {
                return await _context.Users
                        .Where(u => u.UserType == UserType.Admin)
                        .Select(u => u.Id)
                        .ToListAsync();
        }

}
