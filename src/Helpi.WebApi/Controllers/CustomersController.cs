
using Helpi.Application.DTOs;
using Helpi.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Helpi.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
        private readonly CustomerService _service;

        public CustomersController(CustomerService service) => _service = service;

        [HttpGet]
        public async Task<ActionResult<List<CustomerDto>>> GetAll()
        {
                return Ok(await _service.GetAllCustomersAsync());
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerDto>> GetById(int id)
        {
                var customer = await _service.GetByIdAsync(id);
                return Ok(customer);
        }
        [HttpPost]
        public async Task<ActionResult<CustomerDto>> Create(CustomerCreateDto dto)
        {
                return CreatedAtAction(nameof(GetAll), await _service.CreateCustomerAsync(dto));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
                var result = await _service.DeleteCustomerAsync(id);
                if (!result)
                        return BadRequest("Failed to delete customer");
                return NoContent();
        }
}