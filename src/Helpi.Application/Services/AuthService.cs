
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Helpi.Application.DTOs.Auth;
using Helpi.Application.Interfaces;

using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;


namespace Helpi.Application.Services
{
    // public interface IAuthService
    // {
    //     Task<string> GenerateJwtToken(User user);
    //     Task<(bool Success, string Token, string Message)> Login(string email, string password);
    // }

    public class AuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IAuthRepository _authRepository;



        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;

        public AuthService(UserManager<User> userManager, IConfiguration configuration, IAuthRepository authRepository)
        {
            _userManager = userManager;
            _configuration = configuration;
            _authRepository = authRepository;


            _secretKey = _configuration["JwtSettings:Secret"] ?? throw new InvalidOperationException("JWT Secret Key is not configured");
            _issuer = _configuration["JwtSettings:Issuer"] ?? throw new InvalidOperationException("JWT Issuer is not configured");
            _audience = _configuration["JwtSettings:Audience"] ?? throw new InvalidOperationException("JWT Audience is not configured");
        }




        public async Task<(bool Success, string Token, int UserId, string Message)> Login(LoginDto dto)
        {
            // Find user by email
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                return (false, string.Empty, -1, "Invalid email or password");
            }

            // Check password
            var result = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!result)
            {
                return (false, string.Empty, -1, "Invalid email or password");
            }

            // Generate token
            var token = await GenerateJwtToken(user);
            return (true, token, user.Id, "Login successful");
        }



        public async Task<User> _CreateUser(string email, UserType userType, string password)
        {

            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                throw new Exception("User with this email already exists");
            }

            // Create new user
            var user = new User
            {
                UserName = email,
                Email = email,
                UserType = userType,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Create user with password
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Failed to create user: {errors}");
            }

            // Add user to role based on UserType
            var result2 = await _userManager.AddToRoleAsync(user, userType.ToString());

            if (!result2.Succeeded)
            {
                var errors = string.Join(", ", result2.Errors.Select(e => e.Description));
                throw new Exception($"Failed to role based on UserType: {errors}");
            }

            return user;



        }
        public async Task<(bool Success, string Message)> RegisterCustomer(CustomerRegisterDto customerRegistrationDto)
        {


            try
            {
                var user = await _CreateUser(customerRegistrationDto.Email, customerRegistrationDto.UserType, customerRegistrationDto.Password);

                var customerContactInfoDto = customerRegistrationDto.ContactInfo;

                // Create contact info for the user
                var customerContactInfo = new ContactInfo
                {

                    FirstName = customerContactInfoDto.FirstName,
                    LastName = customerContactInfoDto.LastName,
                    Phone = customerContactInfoDto.Phone,
                    Gender = customerContactInfoDto.Gender,
                    GooglePlaceId = customerContactInfoDto.GooglePlaceId,
                    FullAddress = customerContactInfoDto.FullAddress,
                    CityId = customerContactInfoDto.CityId,
                    Latitude = customerContactInfoDto.Latitude,
                    Longitude = customerContactInfoDto.Longitude,
                    State = customerContactInfoDto.State,
                    PostalCode = customerContactInfoDto.PostalCode,
                    Country = customerContactInfoDto.Country
                };

                var seniorContactInfoDto = customerRegistrationDto.SeniorContactInfo;

                var seniorContactInfo = new ContactInfo
                {

                    FirstName = seniorContactInfoDto.FirstName,
                    LastName = seniorContactInfoDto.LastName,
                    Phone = seniorContactInfoDto.Phone,
                    Gender = seniorContactInfoDto.Gender,
                    GooglePlaceId = seniorContactInfoDto.GooglePlaceId,
                    FullAddress = seniorContactInfoDto.FullAddress,
                    CityId = seniorContactInfoDto.CityId,
                    Latitude = seniorContactInfoDto.Latitude,
                    Longitude = seniorContactInfoDto.Longitude,
                    State = seniorContactInfoDto.State,
                    PostalCode = seniorContactInfoDto.PostalCode,
                    Country = seniorContactInfoDto.Country
                };

                var customer = new Customer
                {
                    Id = user.Id,
                    PreferredNotificationMethod = customerRegistrationDto.PreferredNotificationMethod,

                };

                var senior = new Senior
                {
                    CustomerId = user.Id,
                    Relationship = Relationship.Self,
                };

                await _authRepository.RegisterCustomer(customer, customerContactInfo, senior, seniorContactInfo);

                return (true, "Customer /Senior registered successfully");
            }
            catch (Exception ex)
            {

                return (false, $"Registration failed: {ex.Message}");
            }
        }
        public async Task<(bool Success, string Message)> RegisterStudent(StudentRegisterDto registerDto)
        {


            try
            {

                var user = await _CreateUser(registerDto.Email, registerDto.UserType, registerDto.Password);

                // Create contact info for the user
                var contactInfo = new ContactInfo
                {

                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    Phone = registerDto.Phone,
                    Gender = registerDto.Gender,
                    GooglePlaceId = registerDto.GooglePlaceId,
                    FullAddress = registerDto.FullAddress,
                    CityId = registerDto.CityId,
                    Latitude = registerDto.Latitude,
                    Longitude = registerDto.Longitude,
                    City = registerDto.City,
                    State = registerDto.State,
                    PostalCode = registerDto.PostalCode,
                    Country = registerDto.Country
                };

                if (!registerDto.FacultyId.HasValue)
                {
                    throw new ArgumentException("Faculty ID is required for students");
                }

                var student = new Student
                {
                    Id = user.Id,
                    StudentNumber = registerDto.StudentNumber!,
                    FacultyId = registerDto.FacultyId.Value,
                };
                Console.WriteLine("---->?3");
                await _authRepository.RegisterStudent(student, contactInfo);

                return (true, "Student registered successfully");

            }
            catch (Exception ex)
            {

                return (false, $"Registration failed: {ex.Message}");
            }
        }


        public async Task<string> GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim("UserType", user.UserType.ToString()),
            };

            // Add user roles as claims
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}