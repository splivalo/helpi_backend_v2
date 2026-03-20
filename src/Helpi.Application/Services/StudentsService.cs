
using System.Reflection.Metadata.Ecma335;
using AutoMapper;
using Helpi.Application.Common.Interfaces;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Application.Interfaces.Services;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Helpi.Application.Services;


public class StudentsService
{
        private readonly IStudentRepository _repository;
        private readonly IMapper _mapper;

        ILogger<StudentsService> _logger;
        private readonly IContactInfoRepository _contactInfoRepo;
        private readonly IStudentServiceRepository _studentServiceRepo;
        private readonly IStudentAvailabilitySlotRepository _studentAvailabilityRepo;
        private readonly IReassignmentService _reassignmentService;
        private readonly FcmTokensService _fcmTokensService;
        private readonly IFirebaseService _firebaseService;
        private readonly IUserRepository _userRepository;
        private readonly INotificationService _notificationService;
        private readonly INotificationFactory _notificationFactory;
        private readonly IScheduleAssignmentRepository _scheduleAssignmentRepo;
        private readonly IJobInstanceRepository _jobInstanceRepo;

        public StudentsService(IStudentRepository repository,
         IMapper mapper,
           ILogger<StudentsService> logger,
             IStudentServiceRepository studentServiceRepo,
        IStudentAvailabilitySlotRepository studentAvailabilityRepo,
        IContactInfoRepository contactInfoRepo,
        IReassignmentService reassignmentService,
        FcmTokensService fcmTokensService,
        IFirebaseService firebaseService,
        IUserRepository userRepository,
        INotificationService notificationService,
        INotificationFactory notificationFactory,
        IScheduleAssignmentRepository scheduleAssignmentRepo,
        IJobInstanceRepository jobInstanceRepo
         )
        {
                _repository = repository;
                _mapper = mapper;
                _logger = logger;
                _studentServiceRepo = studentServiceRepo;
                _studentAvailabilityRepo = studentAvailabilityRepo;
                _contactInfoRepo = contactInfoRepo;
                _reassignmentService = reassignmentService;
                _fcmTokensService = fcmTokensService;
                _firebaseService = firebaseService;
                _userRepository = userRepository;
                _notificationService = notificationService;
                _notificationFactory = notificationFactory;
                _scheduleAssignmentRepo = scheduleAssignmentRepo;
                _jobInstanceRepo = jobInstanceRepo;
        }

        public async Task<List<StudentDto>> GetStudentsAsync(StudentFilterDto? filter = null)
        {
                return await _repository.GetStudentsWithDetailsAsync(filter);
        }


        public async Task<StudentDto> GetStudentByIdAsync(int id)
        {
                var student = await _repository.GetByIdAsync(id);
                var dto = _mapper.Map<StudentDto>(student);
                var user = await _userRepository.GetByIdAsync(id);
                dto.IsSuspended = user.IsSuspended;
                dto.SuspensionReason = user.SuspensionReason;
                return dto;
        }


        public async Task<StudentDto> CreateStudentAsync(StudentCreateDto dto)
        {
                var student = _mapper.Map<Student>(dto);
                await _repository.AddAsync(student);
                return _mapper.Map<StudentDto>(student);
        }

        public async Task UpdateVerificationStatusAsync(int id, StudentStatus status)
        {
                var student = await _repository.GetByIdAsync(id);
                student.Status = status;
                await _repository.UpdateAsync(student);
        }

        public async Task<StudentDto> UpdateStudentAsync(int id, StudentUpdateDto dto)
        {
                var student = await _repository.GetByIdAsync(id);
                if (dto.StudentNumber != null) student.StudentNumber = dto.StudentNumber;
                if (dto.FacultyId.HasValue) student.FacultyId = dto.FacultyId.Value;
                if (dto.Status.HasValue) student.Status = dto.Status.Value;
                if (dto.BackgroundCheckDate.HasValue) student.BackgroundCheckDate = dto.BackgroundCheckDate.Value;
                await _repository.UpdateAsync(student);
                return _mapper.Map<StudentDto>(student);
        }

        public async Task<List<StudentDto>> FindEligibleStudentsForSchedule(int orderScheduleId, List<int>? notifiedStudentIds)
        {
                var students = await _repository.FindEligibleStudentsForSchedule(
                        orderScheduleId,
                         notifiedStudentIds, preferedStudentId: null);
                return _mapper.Map<List<StudentDto>>(students);
        }

        public async Task<List<StudentDto>> FindEligibleStudentsForInstance2(DateOnly date,
                                TimeOnly startTime,
                                TimeOnly endTime,
                                int orderId,
                              List<int> excludeJobInstanceIds)
        {
                var students = await _repository.FindEligibleStudentsForInstance2(
                        date, startTime, endTime, orderId, preferedStudentId: null, excludeJobInstanceIds: excludeJobInstanceIds);

                return _mapper.Map<List<StudentDto>>(students);
        }


        public async Task<bool> SoftDeleteStudent(int studentId)
        {
                _logger.LogInformation("🗑️ Soft deleting student account {StudentId}", studentId);

                try
                {
                        var student = await _repository.GetByIdAsync(studentId);

                        // Soft delete - disable account but keep ID linked
                        student.Status = StudentStatus.PendingPermanentDeletion;
                        await _repository.UpdateAsync(student);

                        _logger.LogInformation("✅ Student {StudentId} account soft deleted successfully", student.UserId);
                        return true;
                }
                catch (Exception ex)
                {
                        _logger.LogError(ex, "❌ Failed to soft delete student {StudentId}", studentId);
                        return false;
                }
        }

        public async Task<bool> PermanentlyDeleteStudent(int studentId)
        {
                _logger.LogInformation("💀 Permanently deleting student {StudentId} from database", studentId);

                try
                {
                        var student = await _repository.GetByIdAsync(studentId);

                        // Step 1: Reassign all active jobs for this student
                        _logger.LogInformation("🔄 Reassigning jobs for student {StudentId}", student.UserId);
                        await _reassignmentService.ReassignExpiredContractJobs(student.UserId);
                        _logger.LogInformation("✅ Job reassignment completed for student {StudentId}", student.UserId);

                        // Step 2: Delete FCM tokens (non-blocking on failure)
                        try
                        {
                                _logger.LogInformation("🗑️ Deleting FCM tokens for student {StudentId}", student.UserId);
                                await _fcmTokensService.DeleteUserFcmTokensAsync(student.UserId);
                        }
                        catch (Exception fcmEx)
                        {
                                _logger.LogError(fcmEx, "⚠️ Failed to delete FCM tokens for student {StudentId}, continuing with deletion", student.UserId);
                        }

                        // Step 3: Anonymize Firebase data and revoke sessions (non-blocking on failure)
                        try
                        {
                                _logger.LogInformation("🔥 Anonymizing Firebase data for student {StudentId}", student.UserId);
                                await _firebaseService.AnonymizeAndLogoutUserAsync(student.UserId);
                        }
                        catch (Exception firebaseEx)
                        {
                                _logger.LogError(firebaseEx, "⚠️ Failed to anonymize Firebase data for student {StudentId}, continuing with deletion", student.UserId);
                        }

                        // Step 4: Anonymize ASP.NET Core Identity user data
                        _logger.LogInformation("🔐 Anonymizing Identity data for student {StudentId}", student.UserId);
                        await _userRepository.AnonymizeAndLogoutUserAsync(student.UserId);
                        var originalUserName = student.Contact?.FullName ?? $"Student {student.UserId}";
                        _logger.LogInformation("✅ Identity data anonymized for student {StudentId}", student.UserId);

                        // Step 6: Update student status and anonymize contact info
                        student.Status = StudentStatus.Deleted;
                        student.DeletedAt = DateTime.UtcNow;
                        student.StudentNumber = "Deleted";



                        await _contactInfoRepo.AnonymizeContactAsync(student.Contact);

                        await _repository.UpdateAsync(student);

                        // Step 7: Delete related entities (maintaining referential integrity)
                        await _studentAvailabilityRepo.RemoveAllByStudentIdAsync(student.UserId);
                        await _studentServiceRepo.RemoveAllByStudentIdAsync(student.UserId);

                        // Step 8: Notify admin about the deletion
                        try
                        {
                                _logger.LogInformation("📧 Sending deletion notification to admin for student {StudentId}", student.UserId);
                                var notification = _notificationFactory.UserDeletedNotification(
                                        receiverUserId: 1, // admin
                                        deletedUserId: student.UserId,
                                        deletedUserName: originalUserName,
                                        NotificationType.StudentDeleted
                                );
                                await _notificationService.StoreAndNotifyAsync(notification);
                                _logger.LogInformation("✅ Admin notification sent for deleted student {StudentId}", student.UserId);
                        }
                        catch (Exception notifyEx)
                        {
                                _logger.LogError(notifyEx, "⚠️ Failed to send deletion notification for student {StudentId}, but deletion completed", student.UserId);
                        }

                        _logger.LogInformation("✅ Student {StudentId} permanently deleted from database", student.UserId);

                        return true;
                }
                catch (Exception ex)
                {
                        _logger.LogError(ex, "❌ Failed to permanently delete student {StudentId}", studentId);
                        return false;
                }
        }

        /// <summary>
        /// Checks if student can be archived and returns blocking item counts.
        /// </summary>
        public async Task<ArchiveCheckDto> GetArchiveCheckAsync(int studentId)
        {
                var activeAssignments = await _scheduleAssignmentRepo.GetActiveAssignmentsByStudentId(studentId);
                var upcomingSessions = await _jobInstanceRepo.GetStudentUpComingJobInstances(studentId);

                var activeCount = activeAssignments.Count;
                var upcomingCount = upcomingSessions.Count();

                var hasBlocking = activeCount > 0 || upcomingCount > 0;

                return new ArchiveCheckDto
                {
                        CanArchiveDirectly = !hasBlocking,
                        HasBlockingItems = hasBlocking,
                        ActiveAssignmentsCount = activeCount,
                        UpcomingSessionsCount = upcomingCount,
                        Message = hasBlocking
                                ? $"Student ima {activeCount} aktivnih dodjela i {upcomingCount} nadolazećih termina. Svi će biti otkazani."
                                : "Student nema aktivnih dodjela."
                };
        }

        /// <summary>
        /// Archives a student. If force=true, terminates all assignments and cancels sessions first.
        /// </summary>
        public async Task<ArchiveResultDto> ArchiveStudentAsync(int studentId, ArchiveRequestDto request)
        {
                _logger.LogInformation("📦 Archiving student {StudentId}, Force={Force}", studentId, request.Force);

                var student = await _repository.GetByIdAsync(studentId);
                if (student == null)
                {
                        return new ArchiveResultDto { Success = false, Message = "Student not found" };
                }

                // Check for blocking items
                var check = await GetArchiveCheckAsync(studentId);

                if (check.HasBlockingItems && !request.Force)
                {
                        return new ArchiveResultDto
                        {
                                Success = false,
                                Message = check.Message
                        };
                }

                var terminatedCount = 0;
                var cancelledCount = 0;

                // If force, terminate all active assignments (this triggers reassignment)
                if (check.HasBlockingItems && request.Force)
                {
                        _logger.LogInformation("🔄 Force archiving - terminating {Count} active assignments", check.ActiveAssignmentsCount);

                        // Use existing reassignment service to handle job reassignments
                        await _reassignmentService.ReassignExpiredContractJobs(student.UserId);

                        terminatedCount = check.ActiveAssignmentsCount;
                        cancelledCount = check.UpcomingSessionsCount;
                }

                // Archive the student
                student.Status = StudentStatus.AccountDeactivated;
                await _repository.UpdateAsync(student);

                _logger.LogInformation("✅ Student {StudentId} archived successfully", studentId);

                return new ArchiveResultDto
                {
                        Success = true,
                        Message = "Student uspješno arhiviran",
                        TerminatedAssignmentsCount = terminatedCount,
                        CancelledSessionsCount = cancelledCount
                };
        }

        /// <summary>
        /// Unarchives a student by setting status back to Active.
        /// </summary>
        public async Task<ArchiveResultDto> UnarchiveStudentAsync(int studentId)
        {
                _logger.LogInformation("📦 Unarchiving student {StudentId}", studentId);

                var student = await _repository.GetByIdAsync(studentId);
                if (student == null)
                {
                        return new ArchiveResultDto { Success = false, Message = "Student not found" };
                }

                if (student.Status != StudentStatus.AccountDeactivated)
                {
                        return new ArchiveResultDto
                        {
                                Success = false,
                                Message = $"Student is not archived (status: {student.Status})"
                        };
                }

                student.Status = StudentStatus.Active;
                await _repository.UpdateAsync(student);

                _logger.LogInformation("✅ Student {StudentId} unarchived successfully", studentId);

                return new ArchiveResultDto
                {
                        Success = true,
                        Message = "Student uspješno vraćen iz arhive"
                };
        }

}