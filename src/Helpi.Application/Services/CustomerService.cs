
using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.Exceptions;
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

        public async Task<List<CustomerDto>> GetAllCustomersAsync()
        {
                var customers = await _repository.GetAllCustomersAsync();

                return _mapper.Map<List<CustomerDto>>(customers);

        }

        public async Task<CustomerDto> CreateCustomerAsync(CustomerCreateDto dto)
        {
                var customer = _mapper.Map<Customer>(dto);
                await _repository.AddAsync(customer);
                return _mapper.Map<CustomerDto>(customer);
        }

        public async Task<CustomerDto> GetByIdAsync(int id)
        {
                var customer = await _repository.GetByIdAsync(id);

                if (customer == null)
                {
                        throw new NotFoundException(nameof(Customer), id);
                }

                return _mapper.Map<CustomerDto>(customer);
        }

}