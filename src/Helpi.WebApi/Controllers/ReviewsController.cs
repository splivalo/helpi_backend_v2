
using Helpi.Application.DTOs;
using Helpi.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Helpi.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/reviews")]
public class ReviewsController : ControllerBase
{
        private readonly ReviewService _service;

        public ReviewsController(ReviewService service) => _service = service;

        [HttpGet("student/{studentId}")] public async Task<ActionResult<List<ReviewDto>>> GetByStudent(int studentId) => Ok(await _service.GetReviewsByStudentAsync(studentId));
        [HttpPost] public async Task<ActionResult<ReviewDto>> Create(ReviewCreateDto dto) { return CreatedAtAction(nameof(GetByStudent), new { }, await _service.CreateReviewAsync(dto)); }
}