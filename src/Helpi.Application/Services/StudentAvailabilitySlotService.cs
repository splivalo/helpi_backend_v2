
using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.Exceptions;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Domain.Events;
using Microsoft.Extensions.Logging;

namespace Helpi.Application.Services;

public class StudentAvailabilitySlotService
{
        private readonly IStudentAvailabilitySlotRepository _repository;
        private readonly IMapper _mapper;

        private readonly IEventMediator _mediator;

        private readonly ILogger<StudentAvailabilitySlotService> _logger;

        public StudentAvailabilitySlotService(
                IStudentAvailabilitySlotRepository repository,
IEventMediator mediator,
        IMapper mapper,
        ILogger<StudentAvailabilitySlotService> logger)
        {
                _repository = repository;
                _mapper = mapper;
                _mediator = mediator;
                _logger = logger;
        }

        public async Task<StudentAvailabilitySlotDto> GetByIdAsync(int studentId, byte dayOfWeek)
        {
                var slot = await _repository.GetByIdAsync(studentId, dayOfWeek);
                return _mapper.Map<StudentAvailabilitySlotDto>(slot);
        }
        public async Task<List<StudentAvailabilitySlotDto>> GetSlotsByStudentAsync(int studentId)
        {
                var slots = await _repository.GetByStudentAsync(studentId);
                return _mapper.Map<List<StudentAvailabilitySlotDto>>(slots);
        }

        public async Task<IEnumerable<StudentAvailabilitySlotDto>> GetSlotsByDayAndTimeRangeAsync(int studentId, DayOfWeek day, TimeOnly start, TimeOnly end)
        {
                var slots = await _repository.GetByDayAndTimeRangeAsync(studentId, day, start, end);
                return _mapper.Map<IEnumerable<StudentAvailabilitySlotDto>>(slots);
        }

        public async Task<StudentAvailabilitySlotDto> CreateSlotAsync(StudentAvailabilitySlotCreateDto dto)
        {
                var slot = _mapper.Map<StudentAvailabilitySlot>(dto);
                await _repository.AddAsync(slot);

                ReinitiateAllFailedMatches();


                return _mapper.Map<StudentAvailabilitySlotDto>(slot);
        }

        public async Task<List<StudentAvailabilitySlotDto>> CreateSlotsAsync(List<StudentAvailabilitySlotCreateDto> dtos)
        {

                var slots = dtos.Select(_mapper.Map<StudentAvailabilitySlot>).ToList();
                await _repository.AddRangeAsync(slots);

                ReinitiateAllFailedMatches();

                return slots.Select(_mapper.Map<StudentAvailabilitySlotDto>).ToList();

        }


        public async Task<StudentAvailabilitySlotDto> UpdateSlotAsync(int studentId, byte dayOfWeek, StudentAvailabilitySlotUpdateDto dto)
        {
                var slot = await _repository.GetByIdAsync(studentId, dayOfWeek);
                if (slot == null)
                {
                        throw new NotFoundException($"Slot for StudentId {studentId} on DayOfWeek {dayOfWeek} not found.");
                }
                slot.StartTime = dto.StartTime;
                slot.EndTime = dto.EndTime;

                await _repository.UpdateAsync(slot);

                ReinitiateAllFailedMatches();

                return _mapper.Map<StudentAvailabilitySlotDto>(slot);
        }

        public async Task DeleteSlotAsync(int studentId, byte dayOfWeek)
        {
                var slot = await _repository.GetByIdAsync(studentId, dayOfWeek);
                if (slot == null)
                {
                        throw new NotFoundException($"Slot for StudentId {studentId} on DayOfWeek {dayOfWeek} not found.");
                }

                await _repository.DeleteAsync(slot);
        }

        public async Task<List<StudentAvailabilitySlotDto>> UpdateSlotsAsync(List<StudentAvailabilitySlotCreateDto> dtos)
        {
                var slots = dtos.Select(_mapper.Map<StudentAvailabilitySlot>).ToList();
                await _repository.UpdateRangeAsync(slots);

                ReinitiateAllFailedMatches();

                return slots.Select(_mapper.Map<StudentAvailabilitySlotDto>).ToList();
        }
        public async Task<List<StudentAvailabilitySlotDto>> DeleteSlotsAsync(List<StudentAvailabilitySlotCreateDto> dtos)
        {
                var slots = dtos.Select(_mapper.Map<StudentAvailabilitySlot>).ToList();
                await _repository.DeleteRangeAsync(slots);
                return slots.Select(_mapper.Map<StudentAvailabilitySlotDto>).ToList();
        }

        private void ReinitiateAllFailedMatches()
        {
                _ = Task.Run(async () =>
                {
                        try
                        {
                                await _mediator.Publish(new ReinitiateAllFailedMatchesEvent());
                        }
                        catch (Exception ex)
                        {
                                _logger.LogError(ex, "❌ Failed to reinitiate failed matches");
                        }
                });
        }

}