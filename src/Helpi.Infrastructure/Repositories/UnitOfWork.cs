using System.Data;
using Helpi.Application.Interfaces;
using Helpi.Domain.Exceptions;
using Helpi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Helpi.Infrastructure.Repositories;

/// TODO: concider disposing transaction after use
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public async Task SaveChangesAsync()
    {

        /// todo:research concider using  IsolationLevel.Serializable
        await using var transaction = await _context.Database.BeginTransactionAsync(

        );

        // using var connection = _context.Database.GetDbConnection();
        // await connection.OpenAsync();
        // var transaction = await _context.Database.UseTransactionAsync(
        //     await connection.BeginTransactionAsync(System.Data.IsolationLevel.Serializable));




        if (transaction == null)
        {
            throw new Exception("UnitOfWork Transaction failed to initialize");

        }


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