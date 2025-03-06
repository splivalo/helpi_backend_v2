using Helpi.Domain.Entities;

namespace Helpi.Application.Interfaces;

public interface IReviewRepository
{
    Task<Review> GetByIdAsync(int id);
    Task<IEnumerable<Review>> GetByStudentAsync(int studentId);
    Task<IEnumerable<Review>> GetBySeniorAsync(int seniorId);
    Task<decimal> GetAverageRatingAsync(int studentId);
    Task<Review> AddAsync(Review review);
    Task UpdateAsync(Review review);
    Task DeleteAsync(Review review);
}
