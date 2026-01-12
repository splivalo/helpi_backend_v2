namespace Helpi.Application.Common.Interfaces
{
    public interface ILocalizationService
    {
        string GetString(string key, string? culture = null, params object[] args);
    }
}
