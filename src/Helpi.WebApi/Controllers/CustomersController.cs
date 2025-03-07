
using Helpi.Application.DTOs;
using Helpi.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Helpi.WebApi.Controllers;


[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
        private readonly CustomerService _service;

        public CustomersController(CustomerService service) => _service = service;

        [HttpGet] public async Task<ActionResult<List<CustomerDto>>> GetAll() => Ok(await _service.GetAllCustomersAsync());
        [HttpPost] public async Task<ActionResult<CustomerDto>> Create(CustomerCreateDto dto) => CreatedAtAction(nameof(GetAll), await _service.CreateCustomerAsync(dto));
}