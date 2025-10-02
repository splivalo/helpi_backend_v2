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
}
