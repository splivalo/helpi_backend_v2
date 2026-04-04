using Helpi.Domain.Entities;

namespace Helpi.Infrastructure.Repositories;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Helpi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class ReviewRepository : IReviewRepository
{
        private readonly AppDbContext _context;

        public ReviewRepository(AppDbContext context) => _context = context;

        public async Task<IEnumerable<Review>> GetAllAsync()
            => await _context.Reviews
                .Where(r => r.IsPending == false)
                .Include(r => r.Student)
                .Include(r => r.Senior)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

        public async Task<Review?> GetByIdAsync(int id)
    => await _context.Reviews
        .Include(r => r.Student)
        .Include(r => r.Senior)
        .FirstOrDefaultAsync(r => r.Id == id);

        public async Task<IEnumerable<Review>> GetByStudentAsync(int studentId)
            => await _context.Reviews
                .Where(r => r.StudentId == studentId)
                .Where(r => r.IsPending == false)
                .ToListAsync();

        public async Task<IEnumerable<Review>> GetBySeniorAsync(int seniorId)
            => await _context.Reviews
                .Where(r => r.SeniorId == seniorId)
                .ToListAsync();

        public async Task<decimal> GetAverageRatingAsync(int studentId)
            => await _context.Reviews
                .Where(r => r.StudentId == studentId)
                .Where(r => r.IsPending == false)
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

        public Task<int> CountAsync(Expression<Func<Review, bool>> predicate)
        {
                return _context.Reviews.Where(r => r.IsPending == false).CountAsync(predicate);
        }

        public async Task<double?> AverageAsync(Expression<Func<Review, bool>> predicate, Expression<Func<Review, double>> selector)
        {

                var query = _context.Reviews
                .Where(r => r.IsPending == false)
                .Where(predicate);
                if (!await query.AnyAsync())
                {
                        return null;
                }

                return await query.AverageAsync(selector);
        }

        public async Task<List<Review>> GetPendingSeniorReviews(int seniorId)
        {
                return await _context.Reviews
                      .Where(r => r.SeniorId == seniorId && r.IsPending)
                      .Where(r => r.Type == ReviewType.SeniorToStudent)
                      .Where(r => r.RetryCount < r.MaxRetry)
                      .ToListAsync();
        }

        public async Task<List<Review>> GetPendingStudentReviews(int studentId)
        {
                return await _context.Reviews
                      .Where(r => r.StudentId == studentId && r.IsPending)
                      .Where(r => r.Type == ReviewType.StudentToSenior)
                      .Where(r => r.RetryCount < r.MaxRetry)
                      .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetAboutSeniorAsync(int seniorId)
            => await _context.Reviews
                .Where(r => r.SeniorId == seniorId)
                .Where(r => r.Type == ReviewType.StudentToSenior)
                .Where(r => r.IsPending == false)
                .ToListAsync();


}