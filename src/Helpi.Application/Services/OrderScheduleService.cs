
using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;

namespace Helpi.Application.Services;

public class OrderScheduleService
{
        private readonly IOrderScheduleRepository _repository;
        private readonly IMapper _mapper;

        public OrderScheduleService(IOrderScheduleRepository repository, IMapper mapper)
        {
                _repository = repository;
                _mapper = mapper;
        }

        public async Task<List<OrderScheduleDto>> GetSchedulesByOrderAsync(int orderId) =>
            _mapper.Map<List<OrderScheduleDto>>(await _repository.GetByOrderAsync(orderId));

        public async Task<OrderScheduleDto> CreateScheduleAsync(OrderScheduleCreateDto dto)
        {
                var schedule = _mapper.Map<OrderSchedule>(dto);
                await _repository.AddAsync(schedule);
                return _mapper.Map<OrderScheduleDto>(schedule);
        }
}