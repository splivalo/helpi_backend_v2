using System;
namespace Helpi.Domain.Entities
{
    public enum UserType
    {
        Admin,
        Student,
        Customer
    }

    public class User
    {
        public int Id { get; set; }
        public UserType UserType { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}