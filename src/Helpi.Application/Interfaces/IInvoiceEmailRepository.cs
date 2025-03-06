using Helpi.Domain.Entities;

namespace Helpi.Application.Interfaces;
public interface IInvoiceEmailRepository
{
    Task<InvoiceEmail> GetByIdAsync(int id);
    Task<IEnumerable<InvoiceEmail>> GetFailedEmailsAsync();
    Task<InvoiceEmail> AddAsync(InvoiceEmail email);
    Task UpdateAsync(InvoiceEmail email);
    Task DeleteAsync(InvoiceEmail email);
}