
using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;


namespace Helpi.Application.Services;

public class InvoiceService
{
        private readonly IInvoiceRepository _repository;
        private readonly IMapper _mapper;

        public InvoiceService(IInvoiceRepository repository, IMapper mapper)
        {
                _repository = repository;
                _mapper = mapper;
        }

        public async Task<List<InvoiceDto>> GetInvoicesByCustomerAsync(int customerId)
        {
                //     _mapper.Map<List<InvoiceDto>>(await _repository.GetByCustomerAsync(customerId));
                return null;
        }

        // public async Task<InvoiceDto> CreateInvoiceAsync(InvoiceCreateDto dto)
        // {
        //         var invoice = _mapper.Map<Invoice>(dto);
        //         await _repository.AddAsync(invoice);
        //         return _mapper.Map<InvoiceDto>(invoice);
        // }
}