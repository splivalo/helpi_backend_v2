
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

        [HttpGet("student/{studentId}")]
        public async Task<ActionResult<List<ReviewDto>>> GetByStudent(int studentId)
        {
                var reviews = await _service.GetReviewsByStudentAsync(studentId);
                return Ok(reviews);
        }
        [HttpPost]
        public async Task<ActionResult<ReviewDto>> Create(ReviewCreateDto dto)
        {
                var newReview = await _service.CreateReviewAsync(dto);
                return CreatedAtAction(nameof(GetByStudent), new { }, newReview);
        }
}