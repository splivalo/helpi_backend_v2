
using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;

namespace Helpi.Application.Services;

public class PaymentTransactionService
{
        private readonly IPaymentTransactionRepository _repository;
        private readonly IMapper _mapper;

        public PaymentTransactionService(IPaymentTransactionRepository repository, IMapper mapper)
        {
                _repository = repository;
                _mapper = mapper;
        }

        // public async Task<List<PaymentTransactionDto>> GetTransactionsByCustomerAsync(int customerId) =>
        //     _mapper.Map<List<PaymentTransactionDto>>(await _repository.GetByCustomerAsync(customerId));

        // public async Task<PaymentTransactionDto> CreateTransactionAsync(PaymentTransactionCreateDto dto)
        // {
        //         var transaction = _mapper.Map<PaymentTransaction>(dto);
        //         await _repository.AddAsync(transaction);
        //         return _mapper.Map<PaymentTransactionDto>(transaction);
        // }
}
