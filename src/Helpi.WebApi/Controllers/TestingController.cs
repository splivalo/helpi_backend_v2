using System.Text.Json;
using Helpi.Application.DTOs.JobRequest;
using Helpi.Application.DTOs.Minimax;
using Helpi.Application.Interfaces;
using Helpi.Application.Interfaces.Services;
using Helpi.Application.Services;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Helpi.Domain.Events;
using Microsoft.AspNetCore.Mvc;

namespace Helpi.WebApi.Controllers;

[ApiController]
[Route("api/testing")]
public class TestingController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly INotificationService _notificationService;
    private readonly OrdersService _ordersService;
    private readonly JobRequestService _jobRequestService;
    private readonly IJobInstanceService _jobInstanceService;
    private readonly IMinimaxService _minimaxService;
    private readonly IMailgunService _mailgunService;
    private readonly ILogger<TestingController> _logger;
    private readonly IEventMediator _mediator;

    public TestingController(
        IWebHostEnvironment env,
        INotificationService notificationService,
        JobRequestService jobRequestService,
        IJobInstanceService jobInstanceService,
        OrdersService ordersService,
        IMinimaxService minimaxService,
        ILogger<TestingController> logger,
        IEventMediator mediator,
        IMailgunService mailgunService
    )
    {
        _env = env;
        _notificationService = notificationService;
        _jobRequestService = jobRequestService;
        _jobInstanceService = jobInstanceService;
        _ordersService = ordersService;
        _minimaxService = minimaxService;
        _logger = logger;
        _mediator = mediator;
        _mailgunService = mailgunService;
    }

    private bool IsDev() => _env.IsDevelopment();

    private IActionResult DevOnly()
    {
        if (!IsDev()) return Forbid();
        return null;
    }

    [HttpGet("jobInstance/{jobInstanceId}/request-review")]
    public async Task<IActionResult> ReinitiateAllFailedMatches(int jobInstanceId)
    {
        var forbidden = DevOnly();
        if (forbidden != null) return forbidden;

        try
        {
            await _jobInstanceService.RequestJobReviewAsync(jobInstanceId);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to request review");
            return StatusCode(500);
        }
    }

    [HttpGet("ReinitiateAllFailedMatches")]
    public async Task<IActionResult> ReinitiateAllFailedMatches()
    {
        var forbidden = DevOnly();
        if (forbidden != null) return forbidden;

        try
        {
            await _mediator.Publish(new ReinitiateAllFailedMatchesEvent());
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to reinitiate failed matches");
            return StatusCode(500);
        }
    }

    [HttpGet("send-job-request-notification")]
    public async Task<IActionResult> SendNotficication()
    {
        var forbidden = DevOnly();
        if (forbidden != null) return forbidden;

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

        bool notificationSent = await _notificationService.SendNotificationAsync(studentId, jobRequestNotification);
        return Ok(new { Success = notificationSent });
    }

    [HttpGet("job-requests/student/{studentId}")]
    public async Task<IActionResult> GetJobRequests(int studentId)
    {
        var forbidden = DevOnly();
        if (forbidden != null) return forbidden;

        var requests = await _jobRequestService.GetStudentRequests(studentId);
        return Ok(requests);
    }

    [HttpGet("pending/student/{studentId}")]
    public async Task<IActionResult> GetStudentPendingRequests(int studentId)
    {
        var forbidden = DevOnly();
        if (forbidden != null) return forbidden;

        var requests = await _jobRequestService.GetStudentPendingRequests(studentId);
        return Ok(requests);
    }

    [HttpGet("order/{id}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        var forbidden = DevOnly();
        if (forbidden != null) return forbidden;

        var order = await _ordersService.GetOrderByIdAsync(id);
        if (order == null) return NotFound();
        return Ok(order);
    }

    [HttpGet("minimax/getAccessToken")]
    public async Task<IActionResult> GetAccessToken()
    {
        var forbidden = DevOnly();
        if (forbidden != null) return forbidden;

        var token = await _minimaxService.getAccessToken();
        return Ok(token);
    }

    [HttpGet("minimax/getCurrentUserOrganisations")]
    public async Task<IActionResult> GetCurrentUserOrganisations()
    {
        var forbidden = DevOnly();
        if (forbidden != null) return forbidden;

        var orgs = await _minimaxService.GetCurrentUserOrganisations();
        return Ok(orgs);
    }

    [HttpGet("minimax/getCurrencies")]
    public async Task<IActionResult> GetCurrencies()
    {
        var forbidden = DevOnly();
        if (forbidden != null) return forbidden;

        var currencies = await _minimaxService.GetCurrencies();
        return Ok(currencies);
    }

    [HttpGet("minimax/getCurencyByCode")]
    public async Task<IActionResult> GetCurrencyByCode()
    {
        var forbidden = DevOnly();
        if (forbidden != null) return forbidden;

        var currency = await _minimaxService.GetCurrencyByCode("eur");
        return Ok(currency);
    }

    [HttpGet("minimax/getCountries")]
    public async Task<IActionResult> GetCountries()
    {
        var forbidden = DevOnly();
        if (forbidden != null) return forbidden;

        var countries = await _minimaxService.GetCountries();
        return Ok(countries);
    }

    [HttpGet("minimax/getCountryByCode")]
    public async Task<IActionResult> GetCountryByCode()
    {
        var forbidden = DevOnly();
        if (forbidden != null) return forbidden;

        var country = await _minimaxService.GetCountryByCode("hu");
        return Ok(country);
    }

    [HttpGet("minimax/getItems")]
    public async Task<IActionResult> GetItems()
    {
        var forbidden = DevOnly();
        if (forbidden != null) return forbidden;

        var items = await _minimaxService.GetItems();
        return Ok(items);
    }

    [HttpGet("minimax/getCustomerByCode")]
    public async Task<IActionResult> GetCustomerByCode()
    {
        var forbidden = DevOnly();
        if (forbidden != null) return forbidden;

        var customer = await _minimaxService.GetCustomerByCode("cust001");
        return Ok(customer);
    }

    [HttpGet("minimax/paymentMethods")]
    public async Task<IActionResult> GetPaymentMethods()
    {
        var forbidden = DevOnly();
        if (forbidden != null) return forbidden;

        var methods = await _minimaxService.GetPaymentMethods();
        return Ok(methods);
    }

    [HttpGet("minimax/report-templates")]
    public async Task<IActionResult> GetReportTemplates()
    {
        var forbidden = DevOnly();
        if (forbidden != null) return forbidden;

        var templates = await _minimaxService.GetReportTemplates();
        return Ok(templates);
    }

    [HttpGet("minimax/vatrates")]
    public async Task<IActionResult> GetVatRates()
    {
        var forbidden = DevOnly();
        if (forbidden != null) return forbidden;

        var rates = await _minimaxService.GetVatRates();
        return Ok(rates);
    }

    [HttpGet("minimax/document-numbering")]
    public async Task<IActionResult> GetDocumentNumbering()
    {
        var forbidden = DevOnly();
        if (forbidden != null) return forbidden;

        var numbering = await _minimaxService.GetDocumentNumbering();
        return Ok(numbering);
    }

    [HttpGet("minimax/employees")]
    public async Task<IActionResult> GetEmployees()
    {
        var forbidden = DevOnly();
        if (forbidden != null) return forbidden;

        var employees = await _minimaxService.GetEmployees();
        return Ok(employees);
    }

    [HttpPost("minimax/createCustomer")]
    public async Task<IActionResult> CreateCustomer()
    {
        var forbidden = DevOnly();
        if (forbidden != null) return forbidden;

        MinimaxEntityReference country = new MinimaxEntityReference { Id = 95, Name = "HRVATSKA" };
        MinimaxCustomer customer = new MinimaxCustomer
        {
            CustomerId = 0,
            Code = "CUST002",
            Name = "Sidney Test2",
            Address = "123 Sidney Street2",
            PostalCode = "10000",
            City = "Zagreb",
            Country = country
        };

        var created = await _minimaxService.CreateCustomer(customer);
        return Ok(created);
    }

    [HttpPost("minimax/addCustomerContact")]
    public async Task<IActionResult> AddCustomerContact()
    {
        var forbidden = DevOnly();
        if (forbidden != null) return forbidden;

        MinimaxContact contact = new MinimaxContact
        {
            ContactId = 0,
            Customer = new MinimaxEntityReference { Id = 3520788, Name = "Sidney Test" },
            FullName = "Sidney Test",
            Email = "sidneymachara@gmail.com"
        };

        var added = await _minimaxService.AddCustomerContact(contact);
        return Ok(added);
    }

    [HttpPut("mailgun/sendInvoice")]
    public async Task<IActionResult> SendInvoice()
    {
        var forbidden = DevOnly();
        if (forbidden != null) return forbidden;

        var invoices = await _minimaxService.GetAllIssuedInvoicesAsync();
        var invoice = invoices.First();

        var attachment = await _minimaxService.GenerateInvoicePdf((int)invoice!.IssuedInvoiceId!, invoice.RowVersion);

        var attachmentData = new Dictionary<string, string>
        {
            { attachment!.AttachmentFileName, attachment.AttachmentData }
        };



        await _mailgunService.SendEmailAsync(
            to: "sidneymachara@gmail.com",
            subject: "Your Invoice",
            htmlBody: "<h1>Thank you for your order!</h1><p>Invoice attached.</p>",
            attachments: attachmentData
        );

        return Ok();
    }
}
