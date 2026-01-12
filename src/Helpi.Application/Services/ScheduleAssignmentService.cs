
using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Application.Interfaces.Services;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;

namespace Helpi.Application.Services;

public class ScheduleAssignmentService
{
        private readonly IScheduleAssignmentRepository _repository;
        private readonly IMapper _mapper;

        private readonly IReassignmentService _reassignmentService;

        public ScheduleAssignmentService(IScheduleAssignmentRepository repository, IMapper mapper, IReassignmentService reassignmentService)
        {
                _repository = repository;
                _mapper = mapper;
                _reassignmentService = reassignmentService;
        }

        public async Task<List<ScheduleAssignmentDto>> GetAssignmentsByStudentAsync(int studentId)
        {
                var sa = await _repository.GetByStudentAsync(studentId);
                return _mapper.Map<List<ScheduleAssignmentDto>>(sa);
        }

        public async Task<ScheduleAssignmentDto> CreateAssignmentAsync(ScheduleAssignmentCreateDto dto)
        {
                var assignment = _mapper.Map<ScheduleAssignment>(dto);
                await _repository.AddAsync(assignment);
                return _mapper.Map<ScheduleAssignmentDto>(assignment);
        }

        public async Task<StudentDto> GetActiveStudentForOrderScheduleAsync(int orderScheduleId)
        {
                var student = await _repository.GetActiveStudentForOrderScheduleAsync(orderScheduleId);
                return _mapper.Map<StudentDto>(student);
        }
        public async Task<ScheduleAssignmentDto> GetAssignmentForOrderScheduleAsync(int orderScheduleId)
        {
                var assignment = await _repository.GetAssignmentForOrderScheduleAsync(orderScheduleId);
                return _mapper.Map<ScheduleAssignmentDto>(assignment);
        }

        public async Task<(bool, string)> ReassignScheduleAssignment(int scheduleAssignmentId, int? preferedStudentId)
        {
                try
                {
                        var adminId = 1;
                        var requestedByUserId = adminId;

                        // Initiate reassignment with preferred student (if specified)
                        await _reassignmentService.InitiateReassignment(
                                ReassignmentType.CompleteTakeover,
                                ReassignmentTrigger.AdminIntervention,
                                "requested by admin",
                                requestedByUserId,
                                null,
                                scheduleAssignmentId,
                                preferedStudentId
                            );

                        return (true, "success");
                }
                catch (Exception)
                {

                        return (false, "fail");
                }
        }

}