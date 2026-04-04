

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Helpi.Application.Common.Interfaces;
using Helpi.Application.DTOs;
using Helpi.Application.DTOs.Auth;
using Helpi.Application.Interfaces;
using Helpi.Application.Interfaces.Services;

using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NetTopologySuite.Operation.Buffer;


namespace Helpi.Application.Services;

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
    private readonly IFirebaseService _firebaseService;
    private readonly IMailerLiteService _mailerLiteService;
    private readonly ICityRepository _cityRepo;
    private readonly IGooglePlaceService _googlePlaceService;
    private readonly INotificationService _notificationService;
    private readonly INotificationFactory _notificationFactory;
    private readonly IUserRepository _userRepository;

    private readonly ILocalizationService _loc;

    private readonly IPasswordResetRepository _passwordResetRepository;
    private readonly IMailgunService _mailgunService;
    private readonly IMapper _mapper;

    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;

    public AuthService(UserManager<User> userManager,
    IConfiguration configuration,
     IAuthRepository authRepository,
      IMapper mapper,
    IFirebaseService firebaseService,
   IMailerLiteService mailerLiteService,
   ICityRepository cityRepo,
   IGooglePlaceService googlePlaceService,
   INotificationService notificationService,
INotificationFactory notificationFactory,
IUserRepository userRepository,
IPasswordResetRepository passwordResetRepository,

IMailgunService mailgunService,
ILocalizationService loc
   )
    {
        _userManager = userManager;
        _configuration = configuration;
        _authRepository = authRepository;
        _firebaseService = firebaseService;
        _mailerLiteService = mailerLiteService;
        _mapper = mapper;
        _cityRepo = cityRepo;
        _googlePlaceService = googlePlaceService;
        _notificationService = notificationService;
        _notificationFactory = notificationFactory;
        _userRepository = userRepository;
        _passwordResetRepository = passwordResetRepository;
        _mailgunService = mailgunService;

        _loc = loc;


        _secretKey = Environment.GetEnvironmentVariable("JwtSettings:Secret")
                    ?? _configuration["JwtSettings:Secret"]
                    ?? throw new ArgumentNullException("JwtSettings:Secret");

        _issuer = Environment.GetEnvironmentVariable("JwtSettings:Issuer")
                        ?? _configuration["JwtSettings:Issuer"]
                        ?? throw new ArgumentNullException("JwtSettings:Issuer");

        _audience = Environment.GetEnvironmentVariable("JwtSettings:Audience")
                        ?? _configuration["JwtSettings:Audience"]
                        ?? throw new ArgumentNullException("JwtSettings:Audience");



    }




    public async Task<bool> CheckEmailExistsAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user != null;
    }

    public async Task<(bool Success, string Token, int UserId, UserType UserType, string firebaseToken, string Message)> Login(LoginDto dto)
    {
        // Find user by email
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            return (false, string.Empty, -1, UserType.Student, "", "Invalid email or password");
        }

        // Check password
        var result = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!result)
        {
            return (false, string.Empty, -1, UserType.Student, "", "Invalid email or password");
        }

        // Generate token
        var token = await GenerateJwtToken(user);


        var firebase_uid = user.Id.ToString();
        var firbase_claims = _firbaseUserClaims(user);
        var firebaseToken = await _firebaseService.GenerateCustomTokenAsync(firebase_uid, firbase_claims);

        return (true, token.AccessToken, user.Id, user.UserType, firebaseToken, "Login successful");
    }

    public Dictionary<string, dynamic> _firbaseUserClaims(User user)
    {

        var claims = new Dictionary<string, dynamic>
            {
                {"userType",user.UserType},
                {"backendUserId",user.Id}
            };

        return claims;

    }



    public async Task<User> _CreateUser(string name, string email, UserType userType, string password)
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


        await _addSubscriberToMailerLite(name, email, userType);


        return user;



    }


    public async Task<(bool Success, string Message)> RegisterCustomer(CustomerRegisterDto customerRegistrationDto)
    {


        User? user = null;
        try
        {
            user = await _CreateUser(customerRegistrationDto.ContactInfo.FullName, customerRegistrationDto.Email, customerRegistrationDto.UserType, customerRegistrationDto.Password);

            var customerContactInfoDto = customerRegistrationDto.ContactInfo;

            var city = await GetCity(customerContactInfoDto.GooglePlaceId);


            // Create contact info for the user
            var customerContactInfo = new ContactInfo
            {

                FullName = customerContactInfoDto.FullName,
                Phone = customerContactInfoDto.Phone,
                Email = customerRegistrationDto.Email,
                Gender = customerContactInfoDto.Gender,
                DateOfBirth = customerContactInfoDto.DateOfBirth,
                GooglePlaceId = customerContactInfoDto.GooglePlaceId,
                FullAddress = customerContactInfoDto.FullAddress,
                CityId = city.Id,
                CityName = city.Name,
                PostalCode = city.PostalCode,
                Latitude = customerContactInfoDto.Latitude,
                Longitude = customerContactInfoDto.Longitude,
                State = customerContactInfoDto.State,
                Country = customerContactInfoDto.Country
            };

            var seniorContactInfoDto = customerRegistrationDto.SeniorContactInfo;

            ContactInfo? seniorContactInfo = null;


            // if customer is not ordering for self
            if (customerRegistrationDto.Relationship != Relationship.Self)
            {

                var seniorcCity = await GetCity(seniorContactInfoDto!.GooglePlaceId);

                seniorContactInfo = new ContactInfo
                {

                    FullName = seniorContactInfoDto.FullName,
                    Phone = seniorContactInfoDto.Phone,
                    Email = customerRegistrationDto.Email,
                    Gender = seniorContactInfoDto.Gender,
                    DateOfBirth = seniorContactInfoDto.DateOfBirth,
                    GooglePlaceId = seniorContactInfoDto.GooglePlaceId,
                    FullAddress = seniorContactInfoDto.FullAddress,
                    CityId = seniorcCity.Id,
                    CityName = seniorcCity.Name,
                    PostalCode = city.PostalCode,
                    Latitude = seniorContactInfoDto.Latitude,
                    Longitude = seniorContactInfoDto.Longitude,
                    State = seniorContactInfoDto.State,
                    Country = seniorContactInfoDto.Country
                };
            }



            var customer = new Customer
            {
                UserId = user.Id,
                PreferredNotificationMethod = customerRegistrationDto.PreferredNotificationMethod,
            };

            var senior = new Senior
            {
                CustomerId = user.Id,
                Relationship = customerRegistrationDto.Relationship,
            };

            await _authRepository.RegisterCustomer(customer, customerContactInfo, senior, seniorContactInfo);

            var adminIds = await _userRepository.GetAdminIdsAsync();
            await _notificationService.StoreAndNotifyAdminsAsync(adminIds,
                adminId => _notificationFactory.CreateNewSeniorNotification(adminId, senior.Id));

            return (true, "Customer /Senior registered successfully");
        }
        catch (Exception ex)
        {
            // Clean up orphaned user if creation succeeded but customer registration failed
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }
            return (false, $"Registration failed: {ex.Message}");
        }
    }
    public async Task<(bool Success, string Message)> RegisterStudent(StudentRegisterDto registerDto)
    {
        try
        {



            var user = await _CreateUser(registerDto.ContactInfo.FullName, registerDto.Email, registerDto.UserType, registerDto.Password);

            // Create contact info for the user
            var contactInfo = _mapper.Map<ContactInfo>(registerDto.ContactInfo);
            contactInfo.Email = registerDto.Email;




            var city = await GetCity(contactInfo.GooglePlaceId);

            contactInfo.CityId = city.Id;
            contactInfo.CityName = city.Name;
            contactInfo.PostalCode = city.PostalCode;


            if (!registerDto.FacultyId.HasValue)
            {
                throw new ArgumentException("Faculty ID is required for students");
            }

            var student = new Student
            {
                UserId = user.Id,
                FacultyId = registerDto.FacultyId.Value,
            };

            await _authRepository.RegisterStudent(student, contactInfo);

            var adminIds = await _userRepository.GetAdminIdsAsync();
            await _notificationService.StoreAndNotifyAdminsAsync(adminIds,
                adminId => _notificationFactory.CreateNewStudentNotification(adminId, student.UserId));

            return (true, "Student registered successfully");

        }
        catch (Exception ex)
        {

            return (false, $"Registration failed: {ex.Message}");
        }
    }


    public async Task<TokenResponseDto> GenerateJwtToken(User user)
    {
        var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim("UserType", user.UserType.ToString()),
                new Claim("SecurityStamp", user.SecurityStamp ?? ""),
            };

        // Add user roles as claims
        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);


        var accessTokenExpiry = DateTime.Now.AddHours(8780); // should be 1 hour
        var refreshTokenExpiry = accessTokenExpiry.AddDays(7);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: accessTokenExpiry,
            signingCredentials: creds
        );

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        var refreshToken = GenerateRefreshToken();

        // todo: need to save refreshToken to a table
        // await _refreshTokenService.SaveRefreshTokenAsync(user.Id, refreshToken, refreshTokenExpiry);

        return new TokenResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiry = accessTokenExpiry,
            RefreshTokenExpiry = refreshTokenExpiry,
        };
    }

    private string GenerateRefreshToken()
    {

        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public async Task<(bool Success, string Message)> RegisterAdmin(AdminRegisterDto dto)
    {
        try
        {

            var user = await _CreateUser(dto.ContactInfo.FullName, dto.Email, dto.UserType, dto.Password);

            // Create contact info for the user
            var contactInfo = _mapper.Map<ContactInfo>(dto.ContactInfo);
            contactInfo.Email = user.Email;


            var admin = new Admin
            {
                UserId = user.Id,
            };


            var city = await GetCity(contactInfo.GooglePlaceId);

            contactInfo.CityId = city.Id;
            contactInfo.CityName = city.Name;
            contactInfo.PostalCode = city.PostalCode;

            await _authRepository.RegisterAdmin(admin, contactInfo);

            return (true, "Admin registered successfully");

        }
        catch (Exception ex)
        {

            return (false, $"Registration failed: {ex.Message}");
        }
    }

    public async Task _addSubscriberToMailerLite(string name, string email, UserType userType)
    {
        if (userType == UserType.Student || userType == UserType.Customer)
        {
            try
            {
                var group = userType == UserType.Student ? "Students" : "Customers";

                var mailerLiteSubscriber = new MailerLiteSubscriberDto
                {
                    Email = email,
                    Name = name,
                    Group = group,
                };

                await _mailerLiteService.AddSubscriberAsync(mailerLiteSubscriber);
            }
            catch
            {
                // MailerLite failure must not block successful user registration in development.
            }
        }

    }

    public async Task<TokenResponseDto> ChangePassword(string userId, ChangePasswordDto dto)
    {
        if (dto.NewPassword != dto.ConfirmNewPassword)
            throw new ArgumentException("New password and confirmation do not match.");

        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            throw new InvalidOperationException("User not found.");

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Password change failed: {errors}");
        }

        // Reissue new JWT
        var tokenResponseDto = await GenerateJwtToken(user);

        return tokenResponseDto;
    }

    private async Task<City> GetCity(string googlePlaceId)
    {

        var city = new City
        {
            Id = 2,
            Name = "Zagreb",
            GooglePlaceId = "-",
            PostalCode = "10000",
        };

        try
        {
            var cityCreateDto = await _googlePlaceService.GetCityFromLocationPlaceIdAsync(googlePlaceId);



            if (cityCreateDto != null)
            {
                city = await _cityRepo.EnsureCityExistsAsync(
                                cityCreateDto.GooglePlaceId,
                                cityCreateDto.Name,
                                cityCreateDto.PostalCode
                            );
            }

            return city;
        }
        catch (Exception)
        {

            return city;
        }
    }



    ///----------

    public async Task<(bool Success, string Message)> ForgotPasswordAsync(string email)
    {
        var user = await _authRepository.FindByEmailAsync(email);
        if (user == null)
            return (false, "If an account exists, a reset code will be sent."); // security best practice

        var code = new Random().Next(100000, 999999).ToString();

        var resetEntry = new PasswordResetCode
        {
            Email = email,
            Code = code,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };

        await _passwordResetRepository.AddAsync(resetEntry);

        string culture;

        switch (user.UserType)
        {
            case UserType.Admin:
                culture = user.Admin!.Contact.LanguageCode!;
                break;

            case UserType.Student:
                culture = user.Student!.Contact.LanguageCode!;
                break;

            case UserType.Customer:
                culture = user.Customer!.Contact.LanguageCode!;
                break;

            default:
                culture = "hr";
                break;
        }



        var subject = _loc.GetString("Emails.PasswordReset.Subject", culture);
        var body = _loc.GetString("Emails.PasswordReset.Body", culture, code);

        await _mailgunService.SendEmailAsync(email, subject, body);

        return (true, "If an account exists, a reset code has been sent.");
    }

    public async Task<(bool Success, string Message)> ResetPasswordWithCodeAsync(string email, string code, string newPassword)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return (false, "Invalid email or code");

        var validCode = await _passwordResetRepository.GetValidCodeAsync(email, code);
        if (validCode == null)
            return (false, "Invalid or expired code");

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return (false, $"Password reset failed: {errors}");
        }

        await _passwordResetRepository.MarkAsUsedAsync(validCode);

        return (true, "Password has been reset successfully");
    }


}










