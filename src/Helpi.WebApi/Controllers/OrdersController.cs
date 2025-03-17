
using Helpi.Application.DTOs.Order;
using Helpi.Application.Services;
using Helpi.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Helpi.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
        private readonly OrdersService _ordersService;

        public OrdersController(OrdersService ordersService) => _ordersService = ordersService;

        [HttpPost]
        public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] OrderCreateDto orderCreateDto)
        {
                try
                {
                        var order = await _ordersService.CreateOrderAsync(orderCreateDto);
                        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
                }
                catch
                {
                        throw;
                }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
                var order = await _ordersService.GetOrderByIdAsync(id);
                if (order == null) return NotFound();
                return Ok(order);
        }

        [HttpGet("senior/{seniorId}")]
        public async Task<ActionResult<List<OrderDto>>> GetBySenior(int seniorId)
        {
                var orders = await _ordersService.GetOrdersBySeniorAsync(seniorId);
                return Ok(orders);
        }

}