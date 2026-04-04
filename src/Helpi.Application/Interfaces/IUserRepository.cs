using System.Linq.Expressions;
using Helpi.Domain.Entities;

namespace Helpi.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetAllAsync();
    Task<User> AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(User user);
    Task<int> CountAsync(Expression<Func<User, bool>> predicate);

    /// <summary>
    /// Anonymizes user data (email, username, phone, password) and invalidates all sessions
    /// by regenerating the security stamp. Works for any user type.
    /// </summary>
    /// <param name="userId">The user ID to anonymize</param>
    /// <returns>The original username/display name before anonymization</returns>
    Task<string> AnonymizeAndLogoutUserAsync(int userId);

    Task<List<int>> GetAdminIdsAsync();

}