
using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;

namespace Helpi.Application.Services;


public class OrderService
{
        private readonly IOrderRepository _repository;
        private readonly IMapper _mapper;

        public OrderService(IOrderRepository repository, IMapper mapper)
        {
                _repository = repository;
                _mapper = mapper;
        }

        public async Task<List<OrderDto>> GetOrdersBySeniorAsync(int seniorId) =>
            _mapper.Map<List<OrderDto>>(await _repository.GetBySeniorAsync(seniorId));

        public async Task<OrderDto> CreateOrderAsync(OrderCreateDto dto)
        {
                var order = _mapper.Map<Order>(dto);
                await _repository.AddAsync(order);
                return _mapper.Map<OrderDto>(order);
        }
}