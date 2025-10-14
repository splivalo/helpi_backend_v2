
using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;

namespace Helpi.Application.Services;

public class ReviewService
{
        private readonly IReviewRepository _repository;
        private readonly IStudentRepository _studentRepository;
        private readonly IMapper _mapper;

        public ReviewService(
                IReviewRepository repository,
                IStudentRepository studentRepository,
        IMapper mapper)
        {
                _repository = repository;
                _studentRepository = studentRepository;
                _mapper = mapper;
        }

        public async Task<List<ReviewDto>> GetPendingSeniorReviews(int seniorId)
        {
                var pendingReviews = await _repository.GetPendingSeniorReviews(seniorId);
                return _mapper.Map<List<ReviewDto>>(pendingReviews);
        }

        public async Task<List<ReviewDto>> GetReviewsByStudentAsync(int studentId) =>
                _mapper.Map<List<ReviewDto>>(await _repository.GetByStudentAsync(studentId));

        public async Task<ReviewDto> MakeReviewAsync(ReviewUpdateDto dto)
        {
                // 1. Add the review
                var review = await _repository.GetByIdAsync(dto.ReviewId);

                review.Rating = dto.Rating;
                review.Comment = dto.Comment;
                review.IsPending = false;

                await _repository.UpdateAsync(review);

                // 2. Update student rating fields incrementally
                var student = await _studentRepository.GetByIdAsync(review.StudentId);


                if (student == null)
                {
                        throw new Exception($"Student with ID {review.StudentId} not found.");
                }

                // Increment totals
                student.TotalReviews += 1;
                student.TotalRatingSum += (decimal)dto.Rating;
                student.AverageRating = Math.Round(student.TotalRatingSum / student.TotalReviews, 2);

                await _studentRepository.UpdateAsync(student);

                return _mapper.Map<ReviewDto>(review);
        }

        public async Task DeclineToReview(int reviewId)
        {
                var review = await _repository.GetByIdAsync(reviewId);

                if (review == null || review.IsPending == false) return;

                // Stop if already reached max retry
                if (review.RetryCount >= review.MaxRetry) return;

                var now = DateTime.UtcNow;
                var nextRetry = now.AddHours(1); // try again

                // Clamp retry to a user-friendly window (e.g. 9 AM – 8 PM UTC)
                var startHour = 9;
                var endHour = 20;

                if (nextRetry.Hour < startHour)
                {
                        nextRetry = new DateTime(
                            nextRetry.Year, nextRetry.Month, nextRetry.Day,
                            startHour, 0, 0, DateTimeKind.Utc
                        );
                }
                else if (nextRetry.Hour > endHour)
                {
                        // push to next day at startHour
                        var tomorrow = nextRetry.AddDays(1);
                        nextRetry = new DateTime(
                            tomorrow.Year, tomorrow.Month, tomorrow.Day,
                            startHour, 0, 0, DateTimeKind.Utc
                        );
                }

                review.RetryCount += 1;
                review.NextRetryAt = nextRetry;

                await _repository.UpdateAsync(review);
        }


}