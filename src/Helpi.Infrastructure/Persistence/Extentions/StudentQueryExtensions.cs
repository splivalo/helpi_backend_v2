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
                StudentStatus.Active,
                StudentStatus.ContractAboutToExpire
            };

            return query.Where(s => activeStatuses.Contains(s.Status)).Where(s => s.DeletedAt == null);
        }
    }
}
