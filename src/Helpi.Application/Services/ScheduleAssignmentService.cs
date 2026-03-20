
using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Application.Interfaces.Services;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Helpi.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Helpi.Application.Services;

public class ScheduleAssignmentService
{
        private readonly IScheduleAssignmentRepository _repository;
        private readonly IOrderScheduleRepository _orderScheduleRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IMapper _mapper;
        private readonly IReassignmentService _reassignmentService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ScheduleAssignmentService> _logger;
        private readonly OrderStatusMaintenanceService _statusMaintenanceService;

        public ScheduleAssignmentService(
                IScheduleAssignmentRepository repository,
                IOrderScheduleRepository orderScheduleRepository,
                IStudentRepository studentRepository,
                IMapper mapper,
                IReassignmentService reassignmentService,
                IUnitOfWork unitOfWork,
                ILogger<ScheduleAssignmentService> logger,
                OrderStatusMaintenanceService statusMaintenanceService)
        {
                _repository = repository;
                _orderScheduleRepository = orderScheduleRepository;
                _studentRepository = studentRepository;
                _mapper = mapper;
                _reassignmentService = reassignmentService;
                _unitOfWork = unitOfWork;
                _logger = logger;
                _statusMaintenanceService = statusMaintenanceService;
        }

        /// <summary>
        /// Admin directly assigns a student to an order schedule.
        /// Validates travel buffer conflicts and terminates any existing active assignment.
        /// </summary>
        public async Task<ScheduleAssignmentDto> AdminDirectAssignAsync(ScheduleAssignmentCreateDto dto)
        {
                _logger.LogInformation("Admin direct assign: Student {StudentId} → Schedule {ScheduleId}",
                        dto.StudentId, dto.OrderScheduleId);

                // Verify student is active (Active or ContractAboutToExpire)
                var student = await _studentRepository.GetByIdAsync(dto.StudentId)
                        ?? throw new DomainException($"Student {dto.StudentId} not found.");

                if (student.Status != StudentStatus.Active && student.Status != StudentStatus.ContractAboutToExpire)
                {
                        throw new DomainException(
                                $"Student {dto.StudentId} is not eligible for assignment. " +
                                $"Current status: {student.Status}. Only Active or ContractAboutToExpire students can be assigned.");
                }

                // Load OrderSchedule to get DayOfWeek, StartTime, EndTime
                var orderSchedule = await _orderScheduleRepository.GetByIdAsync(dto.OrderScheduleId)
                        ?? throw new DomainException($"OrderSchedule {dto.OrderScheduleId} not found.");

                // Check for time conflicts with 15-min travel buffer
                const int travelBufferMinutes = 15;
                var bufferedStart = orderSchedule.StartTime.AddMinutes(-travelBufferMinutes);
                var bufferedEnd = orderSchedule.EndTime.AddMinutes(travelBufferMinutes);

                var activeAssignments = await _repository.GetActiveAssignmentsByStudentId(dto.StudentId);
                foreach (var sa in activeAssignments)
                {
                        var saSchedule = await _orderScheduleRepository.GetByIdAsync(sa.OrderScheduleId);
                        if (saSchedule == null || saSchedule.Id == dto.OrderScheduleId) continue;

                        // Same day-of-week and overlapping time (with buffer)
                        if (saSchedule.DayOfWeek == orderSchedule.DayOfWeek
                                && saSchedule.StartTime < bufferedEnd
                                && saSchedule.EndTime > bufferedStart)
                        {
                                _logger.LogWarning(
                                        "Conflict detected: Student {StudentId} has assignment on schedule {ConflictScheduleId} " +
                                        "({Day} {Start}-{End}) overlapping with requested {ReqStart}-{ReqEnd} (incl. {Buffer}min buffer)",
                                        dto.StudentId, saSchedule.Id, saSchedule.DayOfWeek,
                                        saSchedule.StartTime, saSchedule.EndTime,
                                        orderSchedule.StartTime, orderSchedule.EndTime, travelBufferMinutes);

                                throw new DomainException(
                                        $"Student {dto.StudentId} has a schedule conflict on day {saSchedule.DayOfWeek} " +
                                        $"({saSchedule.StartTime}-{saSchedule.EndTime}). " +
                                        $"15-minute travel buffer required between sessions.");
                        }
                }

                // Terminate existing active assignment if any
                var existing = await _repository.GetAssignmentForOrderScheduleAsync(dto.OrderScheduleId);
                if (existing != null && existing.Status == AssignmentStatus.Accepted)
                {
                        existing.Status = AssignmentStatus.Terminated;
                        existing.TerminationReason = TerminationReason.AdminIntervention;
                        existing.TerminatedAt = DateTime.UtcNow;
                        await _repository.UpdateAsync(existing);

                        _logger.LogInformation("Terminated previous assignment {AssignmentId} for schedule {ScheduleId}",
                                existing.Id, dto.OrderScheduleId);
                }

                // Create new assignment directly as Accepted
                var assignment = new ScheduleAssignment
                {
                        OrderScheduleId = dto.OrderScheduleId,
                        StudentId = dto.StudentId,
                        Status = AssignmentStatus.Accepted,
                        AssignedAt = DateTime.UtcNow,
                        AcceptedAt = DateTime.UtcNow,
                        PrevAssignmentId = existing?.Id
                };

                await _repository.AddAsync(assignment);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Created assignment {AssignmentId}: Student {StudentId} → Schedule {ScheduleId}",
                        assignment.Id, dto.StudentId, dto.OrderScheduleId);

                // Update order status (Pending → FullAssigned if all schedules now have assignments)
                await _statusMaintenanceService.MaintainOrderStatuses(orderSchedule.OrderId);

                return _mapper.Map<ScheduleAssignmentDto>(assignment);
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