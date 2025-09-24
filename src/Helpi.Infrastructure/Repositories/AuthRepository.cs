namespace Helpi.Infrastructure.Repositories;

using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Helpi.Infrastructure.Persistence;

public class AuthRepository : IAuthRepository
{
    private readonly AppDbContext _context;

    public AuthRepository(AppDbContext context) => _context = context;

    public async Task RegisterStudent(Student student, ContactInfo contactInfo)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Set the relationship
            student.Contact = contactInfo;

            // Add Student (EF Core will handle ContactInfo automatically)
            _context.Set<Student>().Add(student);

            // Save all changes
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    public async Task RegisterCustomer(
        Customer customer,
        ContactInfo customerContactInfo,
        Senior senior,
        ContactInfo? seniorContactInfo)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Add ContactInfo entities to the context
            _context.Set<ContactInfo>().Add(customerContactInfo);

            var isOrderingForAnother = senior.Relationship != Relationship.Self;

            if (isOrderingForAnother)
            {
                _context.Set<ContactInfo>().Add(seniorContactInfo!);
            }


            // Save ContactInfo entities to generate their Ids
            await _context.SaveChangesAsync();

            // Set the relationships
            customer.Contact = customerContactInfo;
            senior.Contact = isOrderingForAnother ? seniorContactInfo! : customerContactInfo;
            customer.Seniors.Add(senior);

            // Add Customer and Senior to the context
            _context.Set<Customer>().Add(customer);

            // Save all changes
            await _context.SaveChangesAsync();

            // Commit the transaction
            await transaction.CommitAsync();


        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task RegisterAdmin(Admin admin, ContactInfo contactInfo)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Set the relationship
            admin.Contact = contactInfo;


            _context.Set<Admin>().Add(admin);

            // Save all changes
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

}