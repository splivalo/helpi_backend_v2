using Helpi.Domain.Entities;
using Helpi.Domain.Enums;

namespace Helpi.Infrastructure.Persistence.Extentions
{
    public static class StudentQueryExtensions
    {
        public static IQueryable<Student> WhereIsActive(this IQueryable<Student> query)
        {
            var activeStatuses = new[]
            {
                StudentStatus.Verified
            };

            return query.Where(s => activeStatuses.Contains(s.Status));
        }
    }
}
