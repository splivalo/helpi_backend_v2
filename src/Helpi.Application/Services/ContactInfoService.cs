
using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Application.Interfaces.Services;
using Helpi.Domain.Entities;

namespace Helpi.Application.Services;

public class ContactInfoService
{
        private readonly IContactInfoRepository _repository;
        private readonly IMapper _mapper;

        private readonly ICityRepository _cityRepo;
        private readonly IGooglePlaceService _googlePlaceService;
        public ContactInfoService(IContactInfoRepository repository,
        IMapper mapper,
            ICityRepository cityRepo,
               IGooglePlaceService googlePlaceService)
        {
                _repository = repository;
                _mapper = mapper;
                _cityRepo = cityRepo;
                _googlePlaceService = googlePlaceService;
        }

        public async Task<ContactInfoDto> GetContactInfoByIdAsync(int id) =>
            _mapper.Map<ContactInfoDto>(await _repository.GetByIdAsync(id));

        public async Task<ContactInfoDto> CreateContactInfoAsync(ContactInfoCreateDto dto)
        {
                var contactInfo = _mapper.Map<ContactInfo>(dto);
                await _repository.AddAsync(contactInfo);
                return _mapper.Map<ContactInfoDto>(contactInfo);
        }

        public async Task UpdateContactInfoAsync(int id, ContactInfoUpdateDto dto)
        {
                var contactInfo = await _repository.GetByIdAsync(id);
                dto.Id = contactInfo.Id;
                dto.CityId = contactInfo.CityId;

                // 
                if (dto.GooglePlaceId != contactInfo.GooglePlaceId)
                {
                        var cityCreateDto = await _googlePlaceService.GetCityFromLocationPlaceIdAsync(dto.GooglePlaceId);

                        if (cityCreateDto != null)
                        {
                                var cityId = await _cityRepo.EnsureCityExistsAsync(
                                    cityCreateDto.GooglePlaceId,
                                    cityCreateDto.Name
                                );

                                dto.CityId = cityId;
                                contactInfo.CityId = cityId;
                        }
                        else
                        {
                                throw new Exception("City resolution failed from GooglePlaceId.");
                        }
                }


                _mapper.Map(dto, contactInfo);

                await _repository.UpdateAsync(contactInfo);
        }


        public async Task DeleteContactInfoAsync(int id) =>
            await _repository.DeleteAsync(await _repository.GetByIdAsync(id));
}