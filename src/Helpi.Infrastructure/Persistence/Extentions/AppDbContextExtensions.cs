using Microsoft.EntityFrameworkCore;

namespace Helpi.Infrastructure.Persistence.Extentions
{
    public static class AppDbContextExtensions
    {
        public static void DetachEntity<TEntity>(this DbContext context, TEntity entity)
            where TEntity : class
        {
            var entry = context.Entry(entity);
            if (entry != null)
            {
                entry.State = EntityState.Detached;
            }
        }

        public static void DetachAllEntities(this DbContext context)
        {
            var entries = context.ChangeTracker.Entries().ToList();
            foreach (var entry in entries)
            {
                entry.State = EntityState.Detached;
            }
        }
    }
}
