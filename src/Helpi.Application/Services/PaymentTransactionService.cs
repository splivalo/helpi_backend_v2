
using AutoMapper;
using Helpi.Application.Interfaces;

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
}
