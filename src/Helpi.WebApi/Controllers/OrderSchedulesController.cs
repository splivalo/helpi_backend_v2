
using Helpi.Application.DTOs;
using Helpi.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Helpi.WebApi.Controllers;


[ApiController]
[Route("api/order-schedules")]
public class OrderSchedulesController : ControllerBase
{
        private readonly OrderScheduleService _service;

        public OrderSchedulesController(OrderScheduleService service) => _service = service;

        [HttpGet("order/{orderId}")] public async Task<ActionResult<List<OrderScheduleDto>>> GetByOrder(int orderId) => Ok(await _service.GetSchedulesByOrderAsync(orderId));
        [HttpPost] public async Task<ActionResult<OrderScheduleDto>> Create(OrderScheduleCreateDto dto) => CreatedAtAction(nameof(GetByOrder), new { }, await _service.CreateScheduleAsync(dto));
}