
using Helpi.Application.DTOs;
using Helpi.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Helpi.WebApi.Controllers;


[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
        private readonly OrderService _service;

        public OrdersController(OrderService service) => _service = service;

        [HttpGet("senior/{seniorId}")] public async Task<ActionResult<List<OrderDto>>> GetBySenior(int seniorId) => Ok(await _service.GetOrdersBySeniorAsync(seniorId));
        [HttpPost] public async Task<ActionResult<OrderDto>> Create(OrderCreateDto dto) => CreatedAtAction(nameof(GetBySenior), new { seniorId = dto.SeniorId }, await _service.CreateOrderAsync(dto));
}