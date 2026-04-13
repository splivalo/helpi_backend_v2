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


    public HNotification JobRescheduledNotification(
        int receiverUserId,
        JobInstance originalJobInstance,
        JobInstance updatedJobInstance,
        string culture)
    {
        return new HNotification
        {
            RecieverUserId = receiverUserId,
            TranslationKey = "Notifications.JobRescheduled",
            Title = _loc.GetString("Notifications.JobRescheduled.Title", culture),
            Body = _loc.GetString(
                "Notifications.JobRescheduled.Body",
                culture,
                originalJobInstance.ScheduledDate,
                originalJobInstance.StartTime,
                updatedJobInstance.ScheduledDate,
                updatedJobInstance.StartTime),
            Type = NotificationType.JobRescheduled,
            SeniorId = updatedJobInstance.SeniorId,
            OrderId = updatedJobInstance.OrderId,
            OrderScheduleId = updatedJobInstance.OrderScheduleId,
            JobInstanceId = updatedJobInstance.Id,
            Payload = JsonSerializer.Serialize(new
            {
                originalJobInstanceId = originalJobInstance.Id,
                updatedJobInstanceId = updatedJobInstance.Id,
                previousDate = originalJobInstance.ScheduledDate,
                previousStartTime = originalJobInstance.StartTime,
                previousEndTime = originalJobInstance.EndTime,
                newDate = updatedJobInstance.ScheduledDate,
                newStartTime = updatedJobInstance.StartTime,
                newEndTime = updatedJobInstance.EndTime,
                studentId = updatedJobInstance.ScheduleAssignment?.StudentId
            })
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
            OrderId = order.Id,
            Payload = JsonSerializer.Serialize(new
            {
                order = order.Id,
                orderNumber = order.OrderNumber
            })
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
            OrderId = jobInstance.OrderId,
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
               culture);


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
    public HNotification SeniorOrderCancelledNotification(int receiverUserId, Order order, string culture)
    {

        var desc = _loc.GetString("Entities.Order", culture, order.OrderNumber);

        return new HNotification
        {
            RecieverUserId = receiverUserId,
            TranslationKey = "Notifications.OrderCancelled",
            Title = _loc.GetString("Notifications.OrderCancelled.Title", culture),
            Body = desc,
            Type = NotificationType.OrderCancelled,
            SeniorId = order.SeniorId,
            OrderId = order.Id,
            Payload = JsonSerializer.Serialize(new
            {
                order = order.Id,
                orderNumber = order.OrderNumber
            })
        };
    }

    public HNotification UserDeletedNotification(int receiverUserId, int deletedUserId, string deletedUserName, NotificationType type)
    {
        // Use Entities key based on type for clearer messaging
        var entityKey = type switch
        {
            NotificationType.StudentDeleted => "Entities.Student",
            NotificationType.SeniorDeleted => "Entities.Senior",
            NotificationType.CustomerDeleted => "Entities.Senior",
            NotificationType.AdminDeleted => "Entities.Admin",
            _ => "Entities.Unknown"
        };

        var entityDescription = _loc.GetString(entityKey, null, deletedUserName);

        var notification = new HNotification
        {
            RecieverUserId = receiverUserId,
            TranslationKey = "Notifications.UserDeleted",
            Title = _loc.GetString("Notifications.UserDeleted.Title"),
            Body = _loc.GetString("Notifications.UserDeleted.Body", null, entityDescription, deletedUserId),
            Type = type,
            Payload = JsonSerializer.Serialize(new { deletedUserId, deletedUserName, userType = type.ToString() })
        };

        // Assign StudentId or SeniorId based on notification type
        if (type == NotificationType.StudentDeleted)
        {
            notification.StudentId = deletedUserId;
        }
        else if (type == NotificationType.SeniorDeleted)
        {
            notification.SeniorId = deletedUserId;
        }

        return notification;
    }

    public HNotification AdminNewOrderNotification(int adminId, Order order)
    {
        return new HNotification
        {
            RecieverUserId = adminId,
            TranslationKey = "Notifications.NewOrder",
            Title = _loc.GetString("Notifications.NewOrder.Title"),
            Body = _loc.GetString("Notifications.NewOrder.Body"),
            Type = NotificationType.NewOrderAdded,
            SeniorId = order.SeniorId,
            OrderId = order.Id,
            Payload = JsonSerializer.Serialize(new { orderId = order.Id, seniorId = order.SeniorId, orderNumber = order.OrderNumber })
        };
    }

    public HNotification AvailabilityChangedNotification(int adminId, string studentName, int orderId, string scheduleDescription)
    {
        var body = $"{studentName}, Pogođena narudžba #{orderId} ({scheduleDescription})";

        return new HNotification
        {
            RecieverUserId = adminId,
            TranslationKey = "Notifications.AvailabilityChanged",
            Title = _loc.GetString("Notifications.AvailabilityChanged.Title"),
            Body = _loc.GetString("Notifications.AvailabilityChanged.Body", null, body),
            Type = NotificationType.AvailabilityChanged,
            OrderId = orderId,
            Payload = JsonSerializer.Serialize(new { studentName, orderId, scheduleDescription })
        };
    }

    public HNotification OrderBackToProcessingNotification(int seniorUserId, int orderId, string culture)
    {
        return new HNotification
        {
            RecieverUserId = seniorUserId,
            TranslationKey = "Notifications.OrderBackToProcessing",
            Title = _loc.GetString("Notifications.OrderBackToProcessing.Title", culture),
            Body = _loc.GetString("Notifications.OrderBackToProcessing.Body", culture),
            Type = NotificationType.OrderBackToProcessing,
            OrderId = orderId,
            Payload = JsonSerializer.Serialize(new { orderId })
        };
    }

}
