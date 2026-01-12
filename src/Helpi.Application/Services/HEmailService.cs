
using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;

namespace Helpi.Application.Services;

public class HEmailService
{
        private readonly IHEmailRepository _repository;
        private readonly IMapper _mapper;

        public HEmailService(IHEmailRepository repository, IMapper mapper)
        {
                _repository = repository;
                _mapper = mapper;
        }

        public async Task<List<HEmailDto>> GetEmailsByInvoiceAsync(int invoiceId)
        {
                //     _mapper.Map<List<InvoiceEmailDto>>(await _repository.GetByInvoiceAsync(invoiceId));
                return null;
        }

        //         public async Task<InvoiceEmailDto> CreateInvoiceEmailAsync(InvoiceEmailCreateDto dto)
        //         {
        //                 var email = _mapper.Map<InvoiceEmail>(dto);
        //                 await _repository.AddAsync(email);
        //                 return _mapper.Map<InvoiceEmailDto>(email);
        //         }
}