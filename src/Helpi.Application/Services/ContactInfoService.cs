
using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Application.Interfaces.Services;
using Helpi.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Helpi.Application.Services;

public class ContactInfoService
{
        private readonly IContactInfoRepository _repository;
        private readonly IMapper _mapper;
        private readonly ICityRepository _cityRepo;
        private readonly IGooglePlaceService _googlePlaceService;
        private readonly ICustomerRepository _customerRepo;
        private readonly IUserRepository _userRepo;
        private readonly ILogger<ContactInfoService> _logger;

        public ContactInfoService(IContactInfoRepository repository,
                IMapper mapper,
                ICityRepository cityRepo,
                IGooglePlaceService googlePlaceService,
                ICustomerRepository customerRepo,
                IUserRepository userRepo,
                ILogger<ContactInfoService> logger)
        {
                _repository = repository;
                _mapper = mapper;
                _cityRepo = cityRepo;
                _googlePlaceService = googlePlaceService;
                _customerRepo = customerRepo;
                _userRepo = userRepo;
                _logger = logger;
        }

        public async Task<ContactInfoDto> GetContactInfoByIdAsync(int id) =>
                        _mapper.Map<ContactInfoDto>(await _repository.GetByIdAsync(id)
                                ?? throw new KeyNotFoundException($"Contact info with ID {id} was not found."));

        public async Task<ContactInfoDto> CreateContactInfoAsync(ContactInfoCreateDto dto)
        {
                var contactInfo = _mapper.Map<ContactInfo>(dto);
                await _repository.AddAsync(contactInfo);
                return _mapper.Map<ContactInfoDto>(contactInfo);
        }

        public async Task UpdateContactInfoAsync(int id, ContactInfoUpdateDto dto)
        {
                var contactInfo = await _repository.GetByIdAsync(id)
                        ?? throw new KeyNotFoundException($"Contact info with ID {id} was not found.");
                dto.Id = contactInfo.Id;
                dto.CityId = contactInfo.CityId;
                dto.CityName = contactInfo.CityName;
                dto.PostalCode = contactInfo.PostalCode;

                // 
                if (dto.GooglePlaceId != contactInfo.GooglePlaceId)
                {
                        var cityCreateDto = await _googlePlaceService.GetCityFromLocationPlaceIdAsync(dto.GooglePlaceId);

                        if (cityCreateDto != null)
                        {
                                var city = await _cityRepo.EnsureCityExistsAsync(
                                    cityCreateDto.GooglePlaceId,
                                    cityCreateDto.Name,
                                    cityCreateDto.PostalCode
                                );

                                dto.CityId = city.Id;
                                dto.CityName = city.Name;
                                dto.PostalCode = city.PostalCode;
                        }
                        else
                        {
                                throw new Exception("City resolution failed from GooglePlaceId.");
                        }
                }


                var oldEmail = contactInfo.Email;
                _mapper.Map(dto, contactInfo);

                await _repository.UpdateAsync(contactInfo);

                // Sync login email when Customer contact email changes
                if (!string.IsNullOrEmpty(dto.Email) && dto.Email != oldEmail)
                {
                        await SyncCustomerLoginEmailAsync(id, dto.Email);
                }
        }

        /// <summary>
        /// If this contact belongs to a Customer, update AspNetUsers.Email + UserName
        /// so the login email stays in sync with the contact email.
        /// </summary>
        private async Task SyncCustomerLoginEmailAsync(int contactId, string newEmail)
        {
                var customer = await _customerRepo.GetByContactIdAsync(contactId);
                if (customer == null) return;

                var user = await _userRepo.GetByIdAsync(customer.UserId);
                if (user == null) return;

                var normalizedEmail = newEmail.ToUpperInvariant();
                user.Email = newEmail;
                user.UserName = newEmail;
                user.NormalizedEmail = normalizedEmail;
                user.NormalizedUserName = normalizedEmail;

                await _userRepo.UpdateAsync(user);
                _logger.LogInformation(
                        "Synced login email for User {UserId}: {OldEmail} → {NewEmail}",
                        user.Id, user.Email, newEmail);
        }

        public async Task DeleteContactInfoAsync(int id)
        {
                var contactInfo = await _repository.GetByIdAsync(id)
                        ?? throw new KeyNotFoundException($"Contact info with ID {id} was not found.");
                await _repository.DeleteAsync(contactInfo);
        }

        public async Task<bool> UpdateLanguageAsync(int contactId, string languageCode)
        {
                var contact = await _repository.GetByIdAsync(contactId);
                if (contact == null) return false;

                contact.LanguageCode = languageCode;
                await _repository.UpdateAsync(contact);

                return true;
        }
}