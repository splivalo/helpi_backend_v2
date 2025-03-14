
using AutoMapper;
using Helpi.Application.DTOs.Order;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Domain.Exceptions;

namespace Helpi.Application.Services;


public class OrdersService
{


        private readonly IOrderRepository _orderRepository;
        private readonly IOrderScheduleRepository _scheduleRepository;
        private readonly IOrderServiceRepository _orderServiceRepository;
        private readonly IMapper _mapper;

        private readonly IUnitOfWork _unitOfWork;

        public OrdersService(IOrderRepository orderRepository,
        IOrderScheduleRepository scheduleRepository,
        IOrderServiceRepository orderServiceRepository,
        IUnitOfWork unitOfWork, IMapper mapper)
        {
                _orderRepository = orderRepository;
                _scheduleRepository = scheduleRepository;
                _orderServiceRepository = orderServiceRepository;
                _unitOfWork = unitOfWork;
                _mapper = mapper;
        }

        public async Task<List<OrderDto>> GetOrdersBySeniorAsync(int seniorId) =>
            _mapper.Map<List<OrderDto>>(await _orderRepository.GetBySeniorAsync(seniorId));

        public async Task<OrderDto> CreateOrderAsync(OrderCreateDto orderCreateDto)
        {
                try
                {
                        // === 1. Create Order ===
                        var order = _mapper.Map<Order>(orderCreateDto);

                        await _orderRepository.AddNoSaveAsync(order);

                        // === 2. Add Services ===
                        var orderServices = orderCreateDto.Services.Select(orderServiceCreateDto
                        => _mapper.Map<OrderService>(orderServiceCreateDto));

                        await _orderServiceRepository.AddRangeNoSaveAsync(orderServices);

                        // === 3. Add Schedules ===
                        var orderSchedules = orderCreateDto.Schedules.Select(orderScheduleCreateDto
                        => _mapper.Map<OrderSchedule>(orderScheduleCreateDto));

                        await _scheduleRepository.AddRangeNoSaveAsync(orderSchedules);

                        // === 4. Commit Transaction ===
                        await _unitOfWork.SaveChangesAsync();

                        // === 5. Matich service ===
                        /// TODO:

                        return _mapper.Map<OrderDto>(order);
                }
                catch (Exception ex)
                {
                        throw new DomainException("Order creation failed", ex);
                }
        }

        public async Task<OrderDto?> GetOrderByIdAsync(int id)
        {
                var order = await _orderRepository.GetByIdAsync(id);
                return _mapper.Map<OrderDto>(order);
        }

}