
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


        [HttpGet("senior/{seniorId}/pending")]
        public async Task<ActionResult<List<ReviewDto>>> GetPendingSeniorReviews(int seniorId)
        {
                var reviews = await _service.GetPendingSeniorReviews(seniorId);
                return Ok(reviews);
        }


        [HttpPut("{reviewId}/decline")]
        public async Task<ActionResult> DeclineToReview(int reviewId)
        {
                await _service.DeclineToReview(reviewId);
                return Ok();
        }


        [HttpPut]
        public async Task<ActionResult<ReviewDto>> Make(ReviewUpdateDto dto)
        {
                var newReview = await _service.MakeReviewAsync(dto);
                return Ok(newReview);
        }
}

