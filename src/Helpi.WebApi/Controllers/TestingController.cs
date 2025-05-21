
using System.Text.Json;
using Helpi.Application.DTOs;
using Helpi.Application.DTOs.JobRequest;
using Helpi.Application.Interfaces.Services;
using Helpi.Application.Services;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Helpi.WebApi.Controllers;


[ApiController]
[Route("api/testing")]
public class TestingController : ControllerBase
{
    private readonly INotificationService _notificationService;


    private readonly OrdersService _ordersService;
    private readonly JobRequestService _jobRequestService;


    public TestingController(INotificationService notificationService,
     JobRequestService jobRequestService,
      OrdersService ordersService)
    {
        _notificationService = notificationService;
        _jobRequestService = jobRequestService;
        _ordersService = ordersService;
    }

    [HttpGet("send-job-request-notification")]
    public async Task SendNotficication()
    {

        var studentId = 2;
        var expiresAt = DateTime.UtcNow.AddMinutes(10);

        var jobRequestNotification = new HNotification
        {
            RecieverUserId = studentId,
            Title = "Job request",
            Body = $"Expires: {expiresAt:MMM dd, yyyy hh:mm tt}",
            Type = NotificationType.JobRequest,
            Payload = JsonSerializer.Serialize(new
            {
                OrderSchedule = 1,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10)
            })
        };


        bool notificationSent = await _notificationService.SendPushNotificationAsync(
            studentId,
            jobRequestNotification);
    }

    [HttpGet("job-requests/student/{studentId}")]
    public async Task<ActionResult<List<JobRequestDto>>> GetJobRequests(int studentId)
    {
        var requests = await _jobRequestService.GetStudentRequests(studentId);
        return Ok(requests);
    }

    [HttpGet("order/{id}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        var order = await _ordersService.GetOrderByIdAsync(id);
        if (order == null) return NotFound();
        return Ok(order);
    }

    [HttpGet("pending/student/{studentId}")]
    public async Task<ActionResult<List<JobRequestDto>>> GetStudentPendingRequests(int studentId)
    {
        var requests = await _jobRequestService.GetStudentPendingRequests(studentId);
        return Ok(requests);
    }


}