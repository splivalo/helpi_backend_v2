
using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;

namespace Helpi.Application.Services;

public class CustomerService
{
        private readonly ICustomerRepository _repository;
        private readonly IMapper _mapper;

        public CustomerService(ICustomerRepository repository, IMapper mapper)
        {
                _repository = repository;
                _mapper = mapper;
        }

        public async Task<List<CustomerDto>> GetAllCustomersAsync() { return null; }

        public async Task<CustomerDto> CreateCustomerAsync(CustomerCreateDto dto)
        {
                var customer = _mapper.Map<Customer>(dto);
                await _repository.AddAsync(customer);
                return _mapper.Map<CustomerDto>(customer);
        }
}