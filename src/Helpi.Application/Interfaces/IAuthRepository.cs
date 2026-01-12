using Helpi.Domain.Entities;
using Helpi.Domain.Enums;

namespace Helpi.Application.Interfaces;

public interface IAuthRepository
{

    Task RegisterStudent(Student student, ContactInfo contactInfo);
    Task RegisterCustomer(Customer customer, ContactInfo customerContactInfo, Senior senior, ContactInfo? seniorContactInfo);

    Task RegisterAdmin(Admin admin, ContactInfo contactInfo);
    Task<User?> FindByEmailAsync(string email);

}