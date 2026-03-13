
using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;

namespace Helpi.Application.Services;

public class ReviewService
{
        private readonly IReviewRepository _repository;
        private readonly IStudentRepository _studentRepository;
        private readonly ISeniorRepository _seniorRepository;
        private readonly IMapper _mapper;

        public ReviewService(
                IReviewRepository repository,
                IStudentRepository studentRepository,
                ISeniorRepository seniorRepository,
                IMapper mapper)
        {
                _repository = repository;
                _studentRepository = studentRepository;
                _seniorRepository = seniorRepository;
                _mapper = mapper;
        }

        public async Task<List<ReviewDto>> GetPendingSeniorReviews(int seniorId)
        {
                var pendingReviews = await _repository.GetPendingSeniorReviews(seniorId);
                return _mapper.Map<List<ReviewDto>>(pendingReviews);
        }

        public async Task<List<ReviewDto>> GetPendingStudentReviews(int studentId)
        {
                var pendingReviews = await _repository.GetPendingStudentReviews(studentId);
                return _mapper.Map<List<ReviewDto>>(pendingReviews);
        }

        public async Task<List<ReviewDto>> GetReviewsByStudentAsync(int studentId) =>
                _mapper.Map<List<ReviewDto>>(await _repository.GetByStudentAsync(studentId));

        public async Task<List<ReviewDto>> GetReviewsAboutSeniorAsync(int seniorId) =>
                _mapper.Map<List<ReviewDto>>(await _repository.GetAboutSeniorAsync(seniorId));

        public async Task<ReviewDto> MakeReviewAsync(ReviewUpdateDto dto)
        {
                var review = await _repository.GetByIdAsync(dto.ReviewId);

                review.Rating = dto.Rating;
                review.Comment = dto.Comment;
                review.IsPending = false;

                await _repository.UpdateAsync(review);

                if (review.Type == ReviewType.StudentToSenior)
                {
                        var senior = await _seniorRepository.GetByIdAsync(review.SeniorId)
                                ?? throw new Exception($"Senior with ID {review.SeniorId} not found.");

                        senior.TotalReviews += 1;
                        senior.TotalRatingSum += (decimal)dto.Rating;
                        senior.AverageRating = Math.Round(senior.TotalRatingSum / senior.TotalReviews, 2);

                        await _seniorRepository.UpdateAsync(senior);
                }
                else
                {
                        var student = await _studentRepository.GetByIdAsync(review.StudentId);

                        if (student == null)
                        {
                                throw new Exception($"Student with ID {review.StudentId} not found.");
                        }

                        student.TotalReviews += 1;
                        student.TotalRatingSum += (decimal)dto.Rating;
                        student.AverageRating = Math.Round(student.TotalRatingSum / student.TotalReviews, 2);

                        await _studentRepository.UpdateAsync(student);
                }

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