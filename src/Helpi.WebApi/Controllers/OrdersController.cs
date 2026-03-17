
using Helpi.Application.DTOs;
using Helpi.Application.DTOs.Order;
using Helpi.Application.Services;
using Helpi.Domain.Enums;
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

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<List<OrderDto>>> GetOrders([FromQuery] OrderStatus? status = null)
        {
                var orders = await _ordersService.GetOrdersAsync(status);
                return Ok(orders);
        }

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


        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<OrderDto>> UpdateOrder(int id, [FromBody] OrderUpdateDto updateDto)
        {
                try
                {
                        var updatedOrder = await _ordersService.UpdateOrderAsync(id, updateDto);
                        return Ok(updatedOrder);
                }
                catch (DomainException ex)
                {
                        // _logger.LogWarning(ex, "Domain error updating order {OrderId}", id);
                        return BadRequest(new { message = ex.Message });
                }
                catch (Exception ex)
                {
                        // _logger.LogError(ex, "Error updating order {OrderId}", id);
                        return StatusCode(500, new { message = "An error occurred while updating the order" });
                }
        }

        [HttpPost("{id}/cancel")]
        public async Task<ActionResult> CancelOrder(int id,
         [FromBody] OrderCancelDto cancelDto)
        {
                try
                {
                        var isAdmin = User.IsInRole("Admin");
                        var result = await _ordersService.CancelOrderAsync(id, cancelDto, isAdmin);
                        return result ? Ok() : BadRequest();
                }
                catch (DomainException ex)
                {
                        // _logger.LogWarning(ex, "Domain error cancelling order {OrderId}", id);
                        return BadRequest(new { message = ex.Message });
                }
                catch (Exception ex)
                {
                        // _logger.LogError(ex, "Error cancelling order {OrderId}", id);
                        return StatusCode(500, new { message = "An error occurred while cancelling the order" });
                }
        }

        /// <summary>
        /// Check if order can be archived and get blocking item counts.
        /// </summary>
        [HttpGet("{id}/archive-check")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ArchiveCheckDto>> GetArchiveCheck(int id)
        {
                var check = await _ordersService.GetArchiveCheckAsync(id);
                return Ok(check);
        }

        /// <summary>
        /// Archive an order. If force=true, cancels all schedules and sessions first.
        /// </summary>
        [HttpPost("{id}/archive")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ArchiveResultDto>> ArchiveOrder(int id, [FromBody] ArchiveRequestDto request)
        {
                var result = await _ordersService.ArchiveOrderAsync(id, request);
                if (!result.Success)
                {
                        return BadRequest(result);
                }
                return Ok(result);
        }

        /// <summary>
        /// Unarchive an order (restore from archive).
        /// </summary>
        [HttpPost("{id}/unarchive")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ArchiveResultDto>> UnarchiveOrder(int id)
        {
                var result = await _ordersService.UnarchiveOrderAsync(id);
                if (!result.Success)
                {
                        return BadRequest(result);
                }
                return Ok(result);
        }

}