namespace Helpi.Application.Interfaces.Services
{
    public interface IPaymentService
    {
        Task ProcessPaymentAsync(int jobInstanceId);
    }
}