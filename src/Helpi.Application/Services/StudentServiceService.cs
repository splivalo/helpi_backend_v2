
using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Application.Services;
using Helpi.Domain.Entities;
using Helpi.Domain.Events;
using Microsoft.Extensions.Logging;



public class StudentServiceService
{
        private readonly IStudentServiceRepository _repository;
        private readonly IMapper _mapper;

        private readonly IEventMediator _mediator;
        private readonly IScheduleAssignmentRepository _assignmentRepository;

        private readonly ILogger<StudentServiceService> _logger;
        public StudentServiceService(
                        IStudentServiceRepository repository,
                       IScheduleAssignmentRepository assignmentRepository,
                        IMapper mapper,
                        IEventMediator mediator,
                        ILogger<StudentServiceService> logger
        )
        {
                _repository = repository;
                _assignmentRepository = assignmentRepository;
                _mapper = mapper;
                _mediator = mediator;
                _logger = logger;
        }

        public async Task<StudentServiceDto> AddServiceToStudentAsync(StudentServiceCreateDto dto)
        {
                var studentService = _mapper.Map<StudentService>(dto);
                await _repository.AddAsync(studentService);

                ReinitiateAllFailedMatches();

                return _mapper.Map<StudentServiceDto>(studentService);
        }

        public async Task<List<StudentServiceDto>> AddServicesToStudentAsync(List<StudentServiceCreateDto> dtos)
        {
                var studentServices = dtos.Select(_mapper.Map<StudentService>).ToList();

                await _repository.AddRangeAsync(studentServices);

                ReinitiateAllFailedMatches();

                return studentServices.Select(_mapper.Map<StudentServiceDto>).ToList();
        }


        public async Task<List<StudentServiceDto>> GetByStudentAsync(int studentId)
        {
                var studentServices = await _repository.GetByStudentAsync(studentId);
                return _mapper.Map<List<StudentServiceDto>>(studentServices);
        }

        public async Task RemoveServiceFromStudentAsync(int studentId, int serviceId)
        {
                var isInUse = await _assignmentRepository.HasActiveAssignmentsForServicesAsync(studentId, new List<int> { serviceId });

                if (isInUse)
                { throw new InvalidOperationException("Cannot remove service used by active assignments."); }

                await _repository.DeleteAsync(studentId, serviceId);
        }

        public async Task RemoveServicesFromStudentAsync(int studentId, List<int> serviceIds)
        {
                var isInUse = await _assignmentRepository.HasActiveAssignmentsForServicesAsync(studentId, serviceIds);

                if (isInUse)
                {
                        throw new InvalidOperationException("Cannot remove services used by active assignments.");
                }

                await _repository.DeleteRangeAsync(studentId, serviceIds);
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