
using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;

namespace Helpi.Application.Services;

public class ReviewService
{
        private readonly IReviewRepository _repository;
        private readonly IMapper _mapper;

        public ReviewService(IReviewRepository repository, IMapper mapper)
        {
                _repository = repository;
                _mapper = mapper;
        }

        public async Task<List<ReviewDto>> GetReviewsByStudentAsync(int studentId) =>
            _mapper.Map<List<ReviewDto>>(await _repository.GetByStudentAsync(studentId));

        public async Task<ReviewDto> CreateReviewAsync(ReviewCreateDto dto)
        {
                var review = _mapper.Map<Review>(dto);
                await _repository.AddAsync(review);
                return _mapper.Map<ReviewDto>(review);
        }
}