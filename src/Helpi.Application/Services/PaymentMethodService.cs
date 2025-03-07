
using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;

namespace Helpi.Application.Services;

public class PaymentMethodService
{
        private readonly IPaymentMethodRepository _repository;
        private readonly IMapper _mapper;

        public PaymentMethodService(IPaymentMethodRepository repository, IMapper mapper)
        {
                _repository = repository;
                _mapper = mapper;
        }

        public async Task<List<PaymentMethodDto>> GetMethodsByCustomerAsync(int customerId) =>
            _mapper.Map<List<PaymentMethodDto>>(await _repository.GetByCustomerIdAsync(customerId));

        public async Task<PaymentMethodDto> AddPaymentMethodAsync(PaymentMethodCreateDto dto)
        {
                var method = _mapper.Map<PaymentMethod>(dto);
                await _repository.AddAsync(method);
                return _mapper.Map<PaymentMethodDto>(method);
        }
}