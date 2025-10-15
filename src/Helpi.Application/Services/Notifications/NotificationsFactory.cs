using System.Text.Json;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;


namespace Helpi.Application.Services;

public static class NotificationFactory
{
    public static HNotification CreateNewStudentNotification(int recieverUserId, int studentId)
    {
        return new HNotification
        {
            RecieverUserId = recieverUserId,
            Title = "New Student Added",
            Body = "A new student has been added",
            Type = NotificationType.NewStudentAdded,
            StudentId = studentId,
            Payload = JsonSerializer.Serialize(new
            {
                RecieverUserId = recieverUserId,
                StudentId = studentId
            })
        };
    }

    public static HNotification CreateNewSeniorNotification(int recieverUserId, int seniorId)
    {
        return new HNotification
        {
            RecieverUserId = recieverUserId,
            Title = "New Senior Added",
            Body = "A new senior has been added",
            Type = NotificationType.NewSeniorAdded,
            SeniorId = seniorId,
            Payload = JsonSerializer.Serialize(new
            {
                RecieverUserId = recieverUserId,
                SeniorId = seniorId
            })
        };
    }

    public static HNotification CreatePaymentFailedNotification(
        int recieverUserId,
         int seniorId,
         int orderId,
          int jobInstanceId)
    {
        return new HNotification
        {
            RecieverUserId = recieverUserId,
            Title = "Payment Failed",
            Body = "Payment failed",
            Type = NotificationType.PaymentFailed,
            SeniorId = seniorId,
            Payload = JsonSerializer.Serialize(new
            {
                RecieverUserId = recieverUserId,
                SeniorId = seniorId,
                OrderId = orderId,
                JobInstanceId = jobInstanceId,
            })
        };
    }
    public static HNotification CreatePaymentSuccessNotification(
        int recieverUserId,
         int seniorId,
         int orderId,
          int jobInstanceId)
    {
        return new HNotification
        {
            RecieverUserId = recieverUserId,
            Title = "Payment Success",
            Body = "Invoice emailed",
            Type = NotificationType.PaymentSuccess,
            SeniorId = seniorId,
            Payload = JsonSerializer.Serialize(new
            {
                RecieverUserId = recieverUserId,
                SeniorId = seniorId,
                OrderId = orderId,
                JobInstanceId = jobInstanceId,
            })
        };
    }
    public static HNotification CreateJobCancelledNotification(
        int recieverUserId,
         int seniorId,
         int orderId,
          int jobInstanceId)
    {
        return new HNotification
        {
            RecieverUserId = recieverUserId,
            Title = "Job Cancelled",
            Body = "Job cancelled",
            Type = NotificationType.JobCancelled,
            SeniorId = seniorId,
            Payload = JsonSerializer.Serialize(new
            {
                RecieverUserId = recieverUserId,
                SeniorId = seniorId,
                OrderId = orderId,
                JobInstanceId = jobInstanceId,
            })
        };
    }


    public static HNotification CreateJobRescheduledNotification(
        int recieverUserId,
        JobInstance originalInstance,
        JobInstance rescheduledInstance,
         string reason)
    {
        return new HNotification
        {
            RecieverUserId = recieverUserId,
            Title = "Schedule Changed",
            Body = $"Your job on {originalInstance.ScheduledDate} has been rescheduled",
            Type = NotificationType.JobRescheduled,
            Payload = JsonSerializer.Serialize(new
            {
                OriginalInstanceId = originalInstance.Id,
                NewInstanceId = rescheduledInstance.Id,
                NewDate = rescheduledInstance.ScheduledDate,
                NewStartTime = rescheduledInstance.StartTime,
                Reason = reason
            })
        };


    }

    public static HNotification CreateStudentJobReminderNotification(

        JobInstance jobInstance)
    {
        return new HNotification
        {
            RecieverUserId = jobInstance.ScheduleAssignment.StudentId,
            Title = "Job Reminder",
            Body = $"Your job starts at {jobInstance.StartTime}",
            Type = NotificationType.JobStartReminder,
            Payload = JsonSerializer.Serialize(new
            {
                jobInstance = jobInstance.StartTime,
                StartTime = jobInstance.StartTime,
            })
        };


    }

    public static HNotification AdminJobReassignmentNotification(int adminId, ReassignmentRecord reassignmentRecord, int seniorId, NotificationType type)
    {

        return new HNotification
        {
            RecieverUserId = adminId,
            Title = type.ToString(),
            Body = $"{GetEntityDescription(reassignmentRecord)}",
            Type = type,
            Payload = JsonSerializer.Serialize(new
            {
                ReassignmentId = reassignmentRecord.Id,
                Action = type,
                reassignmentRecord.ReassignmentType,
                EntityType = reassignmentRecord.ReassignJobInstanceId.HasValue ? "JobInstance" : "Assignment",
                EntityId = reassignmentRecord.ReassignJobInstanceId ?? reassignmentRecord.ReassignAssignmentId,
                ReassignmentRecordId = reassignmentRecord.Id
            }),
            SeniorId = seniorId
        };


    }
    public static HNotification StudentJobReassignmentNotification(
      int studentId,
      ReassignmentRecord reassignmentRecord,
      NotificationType type)
    {

        return new HNotification
        {
            RecieverUserId = studentId,
            Title = type.ToString(),
            Body = $"{GetEntityDescription(reassignmentRecord)}",
            Type = type,
            Payload = JsonSerializer.Serialize(new
            {
                reassignmentRecord.ReassignmentType,
                ReassignmentId = reassignmentRecord.Id,
                Action = type,
                EntityType = reassignmentRecord.ReassignJobInstanceId.HasValue ? "JobInstance" : "Assignment",
                EntityId = reassignmentRecord.ReassignJobInstanceId ?? reassignmentRecord.ReassignAssignmentId,
                ReassignmentRecordId = reassignmentRecord.Id
            }),
        };
    }




    private static string GetEntityDescription(ReassignmentRecord reassignmentRecord)
    {
        if (reassignmentRecord.ReassignJobInstanceId.HasValue)
        {
            return $"Job #{reassignmentRecord.ReassignJobInstanceId}";
        }
        else if (reassignmentRecord.ReassignmentType == ReassignmentType.CompleteTakeover)
        {
            return $"Schedule #{reassignmentRecord.OrderScheduleId}";
        }
        return "Unknown Entity";
    }


}
