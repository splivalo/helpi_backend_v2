using System.Text.Json;
using Helpi.Application.Common.Interfaces;
using Helpi.Application.Utilities;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;

namespace Helpi.Application.Services;

public class NotificationFactory : INotificationFactory
{
    private readonly ILocalizationService _loc;

    public NotificationFactory(ILocalizationService loc)
    {
        _loc = loc;
    }

    public HNotification CreateNewStudentNotification(int receiverUserId, int studentId)
    {

        return new HNotification
        {
            RecieverUserId = receiverUserId,
            TranslationKey = "Notifications.NewStudent",
            Title = _loc.GetString("Notifications.NewStudent.Title"),
            Body = _loc.GetString("Notifications.NewStudent.Body"),
            Type = NotificationType.NewStudentAdded,
            StudentId = studentId,
            Payload = JsonSerializer.Serialize(new { receiverUserId, studentId })
        };
    }

    public HNotification CreateNewSeniorNotification(int receiverUserId, int seniorId)
    {
        return new HNotification
        {
            RecieverUserId = receiverUserId,
            TranslationKey = "Notifications.NewSenior",
            Title = _loc.GetString("Notifications.NewSenior.Title"),
            Body = _loc.GetString("Notifications.NewSenior.Body"),
            Type = NotificationType.NewSeniorAdded,
            SeniorId = seniorId,
            Payload = JsonSerializer.Serialize(new { receiverUserId, seniorId })
        };
    }

    public HNotification CreatePaymentFailedNotification(int receiverUserId, int seniorId, int orderId, int jobInstanceId, string culture)
    {
        return new HNotification
        {
            RecieverUserId = receiverUserId,
            TranslationKey = "Notifications.PaymentFailed",
            Title = _loc.GetString("Notifications.PaymentFailed.Title", culture),
            Body = _loc.GetString("Notifications.PaymentFailed.Body", culture),
            Type = NotificationType.PaymentFailed,
            SeniorId = seniorId,
            Payload = JsonSerializer.Serialize(new { receiverUserId, seniorId, orderId, jobInstanceId })
        };
    }

    public HNotification CreatePaymentSuccessNotification(int receiverUserId, int seniorId, int orderId, int jobInstanceId, string culture)
    {
        return new HNotification
        {
            RecieverUserId = receiverUserId,
            TranslationKey = "Notifications.PaymentSuccess",
            Title = _loc.GetString("Notifications.PaymentSuccess.Title", culture),
            Body = _loc.GetString("Notifications.PaymentSuccess.Body", culture),
            Type = NotificationType.PaymentSuccess,
            SeniorId = seniorId,
            Payload = JsonSerializer.Serialize(new { receiverUserId, seniorId, orderId, jobInstanceId })
        };
    }


    public HNotification CreateStudentJobReminderNotification(JobInstance jobInstance, string culture)
    {
        return new HNotification
        {
            RecieverUserId = jobInstance.ScheduleAssignment!.StudentId,
            TranslationKey = "Notifications.JobReminder",
            Title = _loc.GetString("Notifications.JobReminder.Title", culture),
            Body = _loc.GetString("Notifications.JobReminder.Body", culture, jobInstance.StartTime),
            Type = NotificationType.JobStartReminder,
            Payload = JsonSerializer.Serialize(new { jobInstance.StartTime })
        };
    }

    public HNotification? ReassignmentStartNotification(
      int recieverId,
      ReassignmentRecord record,
      int seniorId,
      NotificationType type,
      string culture = "en")
    {
        int orderId = record.OrderId;
        int orderScheduleId = record.OrderScheduleId;
        int? jobInstanceId = record.ReassignJobInstanceId;

        var description = LocalizationUtils.GetEntityDescription(_loc,
                orderId: orderId,
                scheduleId: orderScheduleId,
                jobInstanceId: jobInstanceId,
                "en");

        return new HNotification
        {
            RecieverUserId = recieverId,
            TranslationKey = $"Notifications.ReassignmentStarted",
            Title = _loc.GetString($"Notifications.ReassignmentStarted.Title", culture),
            Body = _loc.GetString($"Notifications.ReassignmentStarted.Body", culture, description),
            Type = type,
            Payload = JsonSerializer.Serialize(new
            {
                record.ReassignmentType,
                EntityType = record.ReassignJobInstanceId.HasValue ? "JobInstance" : "Assignment",
                EntityId = record.ReassignJobInstanceId ?? record.ReassignAssignmentId,
                ReassignmentRecordId = record.Id
            }),
            SeniorId = seniorId,
            OrderId = orderId,
            OrderScheduleId = orderScheduleId,
            JobInstanceId = jobInstanceId
        };
    }

    public HNotification AdminOrderScheduleCancelledNotification(
   int adminId, OrderSchedule orderSchedule, int seniorId)
    {


        return new HNotification
        {
            RecieverUserId = adminId,
            TranslationKey = "Notifications.OrderScheduleCancelled",
            Title = _loc.GetString("Notifications.OrderScheduleCancelled.Title"),
            Body = _loc.GetString("Notifications.OrderScheduleCancelled.Body", null),
            Type = NotificationType.ScheduleAssignmentCancelled,
            SeniorId = seniorId,
            Payload = JsonSerializer.Serialize(new
            {
                OrderScheduleId = orderSchedule.Id,
                DayOfWeek = orderSchedule.DayOfWeek
            })
        };
    }
    public HNotification AdminOrderCancelledNotification(
   int adminId, Order order)
    {


        return new HNotification
        {
            RecieverUserId = adminId,
            TranslationKey = "Notifications.OrderCancelled",
            Title = _loc.GetString("Notifications.OrderCancelled.Title"),
            Body = _loc.GetString("Notifications.OrderCancelled.Body", null),
            Type = NotificationType.OrderCancelled,
            SeniorId = order.SeniorId,
            Payload = JsonSerializer.Serialize(new
            {
                order = order.Id
            })
        };
    }

    public HNotification NoEligibleStudentsNotification(
        int recieverUserId,
        Order order,
       OrderSchedule schedule,
       ReassignmentRecord? reassignment = null)
    {

        int orderId = order.Id;
        int orderScheduleId = schedule.Id;
        int? jobInstanceId = reassignment?.ReassignJobInstanceId;

        var description = LocalizationUtils.GetEntityDescription(_loc,
                orderId: orderId,
                scheduleId: orderScheduleId,
                jobInstanceId: jobInstanceId,
                "en");

        return new HNotification
        {
            RecieverUserId = recieverUserId,
            TranslationKey = "Notifications.NoEligibleStudents",
            Title = _loc.GetString("Notifications.NoEligibleStudents.Title"),
            Body = _loc.GetString("Notifications.NoEligibleStudents.Body", null, description),
            Type = NotificationType.NoEligibleStudents,
            Payload = JsonSerializer.Serialize(new
            {
                Order = order.Id,
                Schedule = schedule.Id,
                ReassignmentRecordId = reassignment?.Id,
            }),
            SeniorId = order.SeniorId,
            OrderId = orderId,
            OrderScheduleId = orderScheduleId,
            JobInstanceId = jobInstanceId
        };
    }
    public HNotification AllEligibleStudentsNotified(
        int recieverUserId,
        Order order,
       OrderSchedule schedule,
       ReassignmentRecord? reassignment = null)
    {

        int orderId = order.Id;
        int orderScheduleId = schedule.Id;
        int? jobInstanceId = reassignment?.ReassignJobInstanceId;

        var description = LocalizationUtils.GetEntityDescription(_loc,
                orderId: orderId,
                scheduleId: orderScheduleId,
                jobInstanceId: jobInstanceId,
                "en");

        return new HNotification
        {
            RecieverUserId = recieverUserId,
            TranslationKey = "Notifications.AllEligibleStudentsNotified",
            Title = _loc.GetString("Notifications.AllEligibleStudentsNotified.Title"),
            Body = _loc.GetString("Notifications.AllEligibleStudentsNotified.Body", description),
            Type = NotificationType.AllEligableStudentNotified,
            Payload = JsonSerializer.Serialize(new
            {
                Order = order.Id,
                Schedule = schedule.Id,
                ReassignmentRecordId = reassignment?.Id,
            }),
            SeniorId = order.SeniorId,
            OrderId = orderId,
            OrderScheduleId = orderScheduleId,
            JobInstanceId = jobInstanceId
        };
    }



    public HNotification JobCancelledNotification(
    int recieverId,
    JobInstance jobInstance, string culture)
    {
        var date = jobInstance.ScheduledDate;

        return new HNotification
        {
            RecieverUserId = recieverId,
            TranslationKey = "Notifications.JobCancelled",
            Title = _loc.GetString("Notifications.JobCancelled.Title", culture),
            Body = _loc.GetString("Notifications.JobCancelled.Body", culture, date),
            Type = NotificationType.JobCancelled,
            SeniorId = jobInstance.SeniorId,
            Payload = JsonSerializer.Serialize(new
            {
                jobInstanceId = jobInstance.Id,
                SeniorId = jobInstance.SeniorId,
            })
        };
    }

    public HNotification ScheduleAssignmentCancelledNotification(
       int recieverId, ScheduleAssignment scheduleAssignment, int seniorId, string culture)
    {

        int orderId = scheduleAssignment.OrderId;
        int orderScheduleId = scheduleAssignment.OrderScheduleId;

        var description = LocalizationUtils.GetEntityDescription(_loc,
               orderId: orderId,
               scheduleId: orderScheduleId,
               jobInstanceId: null,
               "en");


        return new HNotification
        {
            RecieverUserId = recieverId,
            TranslationKey = "Notifications.ScheduleAssignmentCancelled",
            Title = _loc.GetString("Notifications.ScheduleAssignmentCancelled.Title", culture),
            Body = _loc.GetString("Notifications.ScheduleAssignmentCancelled.Body", culture, description),
            Type = NotificationType.ScheduleAssignmentCancelled,
            SeniorId = seniorId,
            OrderId = orderId,
            OrderScheduleId = orderScheduleId,
            Payload = JsonSerializer.Serialize(new
            {
                ScheduleAssignmentId = scheduleAssignment.Id,
                OrderId = scheduleAssignment.OrderId,
                OrderSchedule = scheduleAssignment.OrderScheduleId
            })
        };
    }



    public HNotification StudentContractAboutToExpire(int studentId, int contractId, string culture)
    {
        return new HNotification
        {
            RecieverUserId = studentId,
            TranslationKey = "Notifications.ContractAboutToExpire",
            Title = _loc.GetString("Notifications.ContractAboutToExpire.Title", culture),
            Body = _loc.GetString("Notifications.ContractAboutToExpire.Body", culture),
            Type = NotificationType.ContractAboutToExpire,
            StudentId = studentId,
            Payload = JsonSerializer.Serialize(new { studentId, contractId })
        };
    }
    public HNotification StudentContractAdded(int studentId, int contractId, string culture)
    {
        return new HNotification
        {
            RecieverUserId = studentId,
            TranslationKey = "Notifications.ContractAdded",
            Title = _loc.GetString("Notifications.ContractAdded.Title", culture),
            Body = _loc.GetString("Notifications.ContractAdded.Body", culture),
            Type = NotificationType.ContractAdded,
            StudentId = studentId,
            Payload = JsonSerializer.Serialize(new { studentId, contractId })
        };
    }
    public HNotification StudentContractUpdated(int studentId, int contractId, string culture)
    {
        return new HNotification
        {
            RecieverUserId = studentId,
            TranslationKey = "Notifications.ContractUpdated",
            Title = _loc.GetString("Notifications.ContractUpdated.Title", culture),
            Body = _loc.GetString("Notifications.ContractUpdated.Body", culture),
            Type = NotificationType.ContractAdded,
            StudentId = studentId,
            Payload = JsonSerializer.Serialize(new { studentId, contractId })
        };
    }
    public HNotification StudentContractDeleted(int studentId, int contractId, string culture)
    {
        return new HNotification
        {
            RecieverUserId = studentId,
            TranslationKey = "Notifications.ContractDeleted",
            Title = _loc.GetString("Notifications.ContractDeleted.Title", culture),
            Body = _loc.GetString("Notifications.ContractDeleted.Body", culture),
            Type = NotificationType.ContractAdded,
            StudentId = studentId,
            Payload = JsonSerializer.Serialize(new { studentId, contractId })
        };
    }

    public HNotification StudentContractExpired(int studentId, int contractId, string culture)
    {
        return new HNotification
        {
            RecieverUserId = studentId,
            TranslationKey = "Notifications.ContractExpired",
            Title = _loc.GetString("Notifications.ContractExpired.Title", culture),
            Body = _loc.GetString("Notifications.ContractExpired.Body", culture),
            Type = NotificationType.ContractExpired,
            StudentId = studentId,
            Payload = JsonSerializer.Serialize(new { studentId, contractId })
        };
    }
    public HNotification ReviewRequestNotification(int recieverId, Review review, JobInstance jobInstance, string culture)
    {
        return new HNotification
        {
            RecieverUserId = recieverId,
            TranslationKey = "Notifications.ReviewRequest",
            Title = _loc.GetString("Notifications.ReviewRequest.Title", culture),
            Body = _loc.GetString("Notifications.ReviewRequest.Body", culture),
            Type = NotificationType.ReviewRequest,
            Payload = JsonSerializer.Serialize(new
            {
                ReviewId = review.Id, // include review id for Flutter side
                RecieverUserId = recieverId,
                JobInstanceId = jobInstance.Id,
                SeniorId = jobInstance.SeniorId,
                SeniorFullName = jobInstance.Senior.Contact.FullName,
                StudentId = jobInstance!.ScheduleAssignment!.StudentId!,
                StudentFullName = jobInstance.ScheduleAssignment.Student.Contact.FullName,
            })
        };
    }
    public HNotification JobRequestNotification(int recieverId,
    OrderSchedule orderSchedule,
     ReassignmentRecord? reassignmentRecord,
      string culture)
    {
        return new HNotification
        {
            RecieverUserId = recieverId,
            TranslationKey = "Notifications.JobRequest",
            Title = _loc.GetString("Notifications.JobRequest.Title", culture),
            Body = _loc.GetString("Notifications.JobRequest.Body", culture),
            Type = NotificationType.JobRequest,
            Payload = JsonSerializer.Serialize(new
            {
                OrderSchedule = orderSchedule.Id,
                IsReassignment = reassignmentRecord != null,
                ReassignmentType = reassignmentRecord?.ReassignmentType.ToString(),
                ReassignmentRecordId = reassignmentRecord?.Id
            })
        };
    }


}
