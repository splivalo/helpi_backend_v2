namespace Helpi.Application.Interfaces;
public interface IUnitOfWork
{
    Task SaveChangesAsync();
}
