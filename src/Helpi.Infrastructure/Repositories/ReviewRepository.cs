using Helpi.Domain.Entities;

namespace Helpi.Infrastructure.Repositories;

using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class ReviewRepository : IReviewRepository
{
        private readonly AppDbContext _context;

        public ReviewRepository(AppDbContext context) => _context = context;

        public async Task<Review> GetByIdAsync(int id)
            => await _context.Reviews
                .Include(r => r.Student)
                .Include(r => r.Senior)
                .FirstOrDefaultAsync(r => r.Id == id);

        public async Task<IEnumerable<Review>> GetByStudentAsync(int studentId)
            => await _context.Reviews
                .Where(r => r.StudentId == studentId)
                .ToListAsync();

        public async Task<IEnumerable<Review>> GetBySeniorAsync(int seniorId)
            => await _context.Reviews
                .Where(r => r.SeniorId == seniorId)
                .ToListAsync();

        public async Task<decimal> GetAverageRatingAsync(int studentId)
            => await _context.Reviews
                .Where(r => r.StudentId == studentId)
                .AverageAsync(r => (decimal?)r.Rating) ?? 0m;

        public async Task<Review> AddAsync(Review review)
        {
                await _context.Reviews.AddAsync(review);
                await _context.SaveChangesAsync();
                return review;
        }

        public async Task UpdateAsync(Review review)
        {
                _context.Reviews.Update(review);
                await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Review review)
        {
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();
        }
}