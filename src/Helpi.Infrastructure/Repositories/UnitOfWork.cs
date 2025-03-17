using Helpi.Application.Interfaces;
using Helpi.Domain.Exceptions;
using Helpi.Infrastructure.Persistence;

namespace Helpi.Infrastructure.Repositories;
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public async Task SaveChangesAsync()
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(

        );
        try
        {
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}