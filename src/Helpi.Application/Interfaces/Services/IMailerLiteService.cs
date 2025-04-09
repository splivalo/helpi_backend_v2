

using Helpi.Application.DTOs;

namespace Helpi.Application.Interfaces.Services;

public interface IMailerLiteService
{
    Task<bool> AddSubscriberAsync(MailerLiteSubscriberDto subscriber);
}
