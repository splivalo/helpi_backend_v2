
using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;

namespace Helpi.Application.Services;

public class ContactInfoService
{
        private readonly IContactInfoRepository _repository;
        private readonly IMapper _mapper;

        public ContactInfoService(IContactInfoRepository repository, IMapper mapper)
        {
                _repository = repository;
                _mapper = mapper;
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
                _mapper.Map(dto, contactInfo);
                await _repository.UpdateAsync(contactInfo);
        }

        public async Task DeleteContactInfoAsync(int id) =>
            await _repository.DeleteAsync(await _repository.GetByIdAsync(id));
}