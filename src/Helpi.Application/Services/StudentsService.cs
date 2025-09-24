
using System.Reflection.Metadata.Ecma335;
using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
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

        public StudentsService(IStudentRepository repository,
         IMapper mapper,
           ILogger<StudentsService> logger,
             IStudentServiceRepository studentServiceRepo,
        IStudentAvailabilitySlotRepository studentAvailabilityRepo,
        IContactInfoRepository contactInfoRepo
         )
        {
                _repository = repository;
                _mapper = mapper;
                _logger = logger;
                _studentServiceRepo = studentServiceRepo;
                _studentAvailabilityRepo = studentAvailabilityRepo;
                _contactInfoRepo = contactInfoRepo;
        }

        public async Task<List<StudentDto>> GetStudentsAsync(StudentFilterDto? filter = null)
        {
                return await _repository.GetStudentsWithDetailsAsync(filter);
        }


        public async Task<StudentDto> GetStudentByIdAsync(int id)
        {
                var student = await _repository.GetByIdAsync(id);
                return _mapper.Map<StudentDto>(student);
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

        public async Task<List<StudentDto>> FindEligibleStudentsForSchedule(int orderScheduleId, List<int>? notifiedStudentIds)
        {
                var students = await _repository.FindEligibleStudentsForSchedule(orderScheduleId, notifiedStudentIds);
                return _mapper.Map<List<StudentDto>>(students);
        }

        public async Task<List<StudentDto>> FindEligibleStudentsForInstance2(DateOnly date,
                                TimeOnly startTime,
                                TimeOnly endTime,
                                int orderId)
        {
                var students = await _repository.FindEligibleStudentsForInstance2(
                        date, startTime, endTime, orderId);

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

                        student.Status = StudentStatus.Deleted;

                        var contact = student.Contact;

                        contact.Phone = "";
                        contact.Email = $"deleted_{student.UserId}@deleted.local";
                        contact.FullAddress = "";
                        contact.PostalCode = "";

                        await _repository.UpdateAsync(student);
                        await _contactInfoRepo.UpdateAsync(contact);

                        // Delete related entities first (maintaining referential integrity)
                        await _studentAvailabilityRepo.RemoveAllByStudentIdAsync(student.UserId);
                        await _studentServiceRepo.RemoveAllByStudentIdAsync(student.UserId);



                        _logger.LogInformation("✅ Student {StudentId} permanently deleted from database", student.UserId);

                        return true;
                }
                catch (Exception ex)
                {
                        _logger.LogError(ex, "❌ Failed to permanently delete student {StudentId}", studentId);
                        return false;
                }
        }

}