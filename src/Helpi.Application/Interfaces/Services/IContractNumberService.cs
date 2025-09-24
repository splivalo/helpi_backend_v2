namespace Helpi.Application.Services
{
    public interface IContractNumberService
    {
        Task<int> GetNextContractNumberAsync();
    }
}
