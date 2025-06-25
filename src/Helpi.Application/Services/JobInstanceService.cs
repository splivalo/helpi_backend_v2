
using System.Text.Json;
using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Application.Interfaces.BackgroundJobs;
using Helpi.Application.Interfaces.Services;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;

namespace Helpi.Application.Services;

public class JobInstanceService : IJobInstanceService
{
        private readonly IJobInstanceRepository _repository;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly CompletionStatusService _completionStatusService;

        private readonly IHangfireService _hangfireService;
        public JobInstanceService(
                IJobInstanceRepository repository,
                IMapper mapper,
                INotificationService notificationService,
                 CompletionStatusService completionStatusService,
                  IHangfireService hangfireService
                )
        {
                _repository = repository;
                _mapper = mapper;
                _notificationService = notificationService;
                _completionStatusService = completionStatusService;
                _hangfireService = hangfireService;
        }


        public async Task<List<JobInstanceDto>> GetJobInstancesByAssignmentAsync(int assignmentId)
        {

                return _mapper.Map<List<JobInstanceDto>>(await _repository.GetByAssignmentAsync(assignmentId));
        }

        public async Task<List<JobInstanceDto>> GetJobInstancesByStudentAsync(int studentId)
        {
                var jobInstances = await _repository.GetJobInstancesByStudentAsync(studentId);
                return _mapper.Map<List<JobInstanceDto>>(jobInstances);
        }

        public async Task<List<JobInstanceDto>> GetJobInstances()
        {
                var jobInstances = await _repository.GetJobInstances();
                return _mapper.Map<List<JobInstanceDto>>(jobInstances);
        }

        public async Task<List<JobInstanceDto>> GetSeniorCompletedJobInstances(int seniorId)
        {
                var jobInstances = await _repository.GetSeniorCompletedJobInstances(seniorId);
                return _mapper.Map<List<JobInstanceDto>>(jobInstances);
        }
        public async Task<List<JobInstanceDto>> GetStudentCompletedJobInstances(int studentId)
        {
                var jobInstances = await _repository.GetStudentCompletedJobInstances(studentId);
                return _mapper.Map<List<JobInstanceDto>>(jobInstances);
        }
        public async Task<List<JobInstanceDto>> GetStudentUpComingJobInstances(int studentId)
        {
                var jobInstances = await _repository.GetStudentUpComingJobInstances(studentId);
                return _mapper.Map<List<JobInstanceDto>>(jobInstances);
        }


        public async Task UpdateToInProgressAsync(int jobInstanceId)
        {

                var instance = await _repository.UpdateToInProgressAsync(jobInstanceId);

                if (instance == null) return;

                if (instance.Status != JobInstanceStatus.InProgress) return;


                var assignedStudentId = instance.Assignment.StudentId;
                await _notificationService.SendJobStartedNotificationAsync(assignedStudentId, instance);

                var customerId = instance.Senior.CustomerId;
                await _notificationService.SendJobCompletedNotificationAsync(customerId, instance);

        }

        public async Task UpdateToCompletedAsync(int jobInstanceId)
        {
                var instance = await _repository.UpdateToCompletedAsync(jobInstanceId);

                if (instance == null) return;

                if (instance.Status != JobInstanceStatus.Completed) return;

                var assignedStudent = instance.Assignment;
                await _notificationService.SendJobCompletedNotificationAsync(assignedStudent.Id, instance);

                var customerId = instance.Senior.CustomerId;
                await _notificationService.SendJobCompletedNotificationAsync(customerId, instance);

                await _completionStatusService.ProcessCompletionStatuses(instance.OrderId);

                var reviewRequestTime = DateTime.UtcNow.AddMinutes(5);

                _hangfireService.Schedule<IJobInstanceService>(
               s => s.RequestJobReviewAsync(instance.Id),
               reviewRequestTime
           );

        }

        public async Task RequestJobReviewAsync(int jobInstanceId)
        {
                var instance = await _repository.GetByIdAsync(jobInstanceId);

                if (instance == null) return;

                if (instance.Status != JobInstanceStatus.Completed) return;

                var customerId = instance.Senior.CustomerId;

                var notification = new HNotification
                {
                        RecieverUserId = customerId,
                        Title = "Review",
                        Body = "How was your expirence? ",
                        Type = NotificationType.ReviewRequest,
                        Payload = JsonSerializer.Serialize(new
                        {
                                RecieverUserId = customerId,
                                JobInstanceId = jobInstanceId,
                                SeniorId = instance.SeniorId,
                                SeniorFullName = instance.Senior.Contact.FullName,
                                StudentId = instance.Assignment.StudentId,
                                StudentFullName = instance.Assignment.Student.Contact.FullName,
                        })
                };


                await _notificationService.SendPushNotificationAsync(customerId, notification);
        }

}
