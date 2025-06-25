
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

        public async Task<List<ReviewDto>> GetReviewsByStudentAsync(int studentId) =>
            _mapper.Map<List<ReviewDto>>(await _repository.GetByStudentAsync(studentId));

        public async Task<ReviewDto> CreateReviewAsync(ReviewCreateDto dto)
        {
                // 1. Add the review
                var review = _mapper.Map<Review>(dto);
                await _repository.AddAsync(review);

                // 2. Update student rating fields incrementally
                var student = await _studentRepository.GetByIdAsync(dto.StudentId);


                if (student == null)
                {
                        throw new Exception($"Student with ID {dto.StudentId} not found.");
                }

                // Increment totals
                student.TotalReviews += 1;
                student.TotalRatingSum += dto.Rating;
                student.AverageRating = Math.Round(student.TotalRatingSum / student.TotalReviews, 2);

                await _studentRepository.UpdateAsync(student);

                return _mapper.Map<ReviewDto>(review);
        }

}