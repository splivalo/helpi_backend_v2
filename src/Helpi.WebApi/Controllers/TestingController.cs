
using System.Text.Json;
using Helpi.Application.DTOs.JobRequest;
using Helpi.Application.DTOs.Minimax;
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
    private readonly IMinimaxService _minimaxService;


    public TestingController(INotificationService notificationService,
     JobRequestService jobRequestService,
      OrdersService ordersService,
       IMinimaxService minimaxService
      )
    {
        _notificationService = notificationService;
        _jobRequestService = jobRequestService;
        _ordersService = ordersService;
        _minimaxService = minimaxService;
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


    [HttpGet("minimax/getAccessToken")]
    public async Task<ActionResult<string>> GetAccessToken()
    {
        var token = await _minimaxService.getAccessToken();
        return Ok(token);
    }
    [HttpGet("minimax/getCurrencies")]
    public async Task<ActionResult<string>> GetCurrencies()
    {
        var c = await _minimaxService.GetCurrencies();
        return Ok(c);
    }
    [HttpGet("minimax/getCurencyByCode")]
    public async Task<ActionResult<MinimaxCurrency>> GetCurrencyByCode()
    {

        var c = await _minimaxService.GetCurrencyByCode("eur");
        return Ok(c);
    }
    [HttpGet("minimax/getCountries")]
    public async Task<ActionResult<string>> GetCountries()
    {
        var c = await _minimaxService.GetCountries();
        return Ok(c);
    }

    [HttpGet("minimax/getCountryByCode")]
    public async Task<ActionResult<MinimaxCountry>> GetCountryByCode()
    {
        ///3520788
        var c = await _minimaxService.GetCountryByCode("hu");
        return Ok(c);
    }

    [HttpGet("minimax/getItems")]
    public async Task<ActionResult<string>> GetItems()
    {
        var c = await _minimaxService.GetItems();
        return Ok(c);
    }
    [HttpGet("minimax/getCustomerByCode")]
    public async Task<ActionResult<MinimaxCustomer>> GetCustomerByCode()
    {
        ///3520788
        var c = await _minimaxService.GetCustomerByCode("cust001");
        return Ok(c);
    }
    [HttpGet("minimax/paymentMethods")]
    public async Task<ActionResult<MinimaxCustomer>> GetPaymentMethods()
    {

        var c = await _minimaxService.GetPaymentMethods();
        return Ok(c);
    }

    [HttpPost("minimax/createCustomer")]
    public async Task<ActionResult<MinimaxCustomer>> CreateCustomer()
    {

        // {
        //   "countryId": 95,
        //   "code": "HR",
        //   "name": "HRVATSKA"
        // }
        MinimaxEntityReference country = new MinimaxEntityReference
        {
            Id = 95,
            Name = "HRVATSKA"
        };

        MinimaxCustomer minimaxCustomer = new MinimaxCustomer
        {
            CustomerId = 0,
            Code = "CUST002",
            Name = "Sidney Test2",
            Address = "123 Sidney Street2",
            PostalCode = "10000",
            City = "Zagreb",
            Country = country
        };

        var customer = await _minimaxService.CreateCustomer(minimaxCustomer);
        return Ok(customer);
    }

    [HttpPost("minimax/addCustomerContact")]
    public async Task<ActionResult<MinimaxCustomer>> AddCustomerContact()
    {


        MinimaxContact minimaxCustomer = new MinimaxContact
        {
            ContactId = 0,
            Customer = new MinimaxEntityReference
            {
                Id = 3520788,
                Name = "Sidney Test",
            },
            FullName = "Sidney Test",
            Email = "sidneymachara@gmail.com"
        };

        var customer = await _minimaxService.AddCustomerContact(minimaxCustomer);
        return Ok(customer);
    }


    [HttpPost("minimax/createIssuedInvoice")]
    public async Task<ActionResult<MinimaxCustomer>> CreateIssuedInvoice()
    {


        MinimaxEntityReference country = new MinimaxEntityReference
        {
            Id = 97,
            Name = "HRVATSKA"
        };
        MinimaxEntityReference currency = new MinimaxEntityReference
        {
            Id = 7,
            Name = "Euro"
        };


        var minimaxItem = new MinimaxItem
        {
            ItemId = 2878516,
            Title = "Helpi usluga",
            Code = "1",
            UnitOfMeasurement = "Sat",
            MassPerUnit = 0,
            ItemType = "AS",
            VatRate = new MinimaxEntityReference
            {
                Id = 1,
                Name = "S",
                ResourceUrl = "/api/orgs/39503/vatrates/1"
            },
            Price = 0,
            Currency = null,
            RevenueAccountDomestic = null,
            RevenueAccountOutsideEU = null,
            RevenueAccountEU = null,
            StocksAccount = null,
            ProductGroup = null
        };

        var paymentMethod = new MinimaxPaymentMethod
        {
            PaymentMethodId = 165189,
            Name = "Transactional Account",
            Type = "T",
            Usage = "D",
            Default = "Y"
        };


        var minimaxCustomer = new MinimaxIssuedInvoice
        {
            InvoiceType = "R",
            Customer = new MinimaxEntityReference
            {
                Id = 3520788,
            },
            DateDue = DateTime.UtcNow,
            DateIssued = DateTime.UtcNow,
            DateTransaction = DateTime.UtcNow,
            AddresseeName = " sidney test",
            AddresseeAddress = "addy",
            AddresseeCity = "city",
            AddresseeCountryName = "countryname",
            AddresseePostalCode = "1000",
            RecipientAddress = "aa",
            RecipientCity = "aa",
            RecipientName = "aa",
            RecipientCountry = country,

            Currency = currency,

            IssuedInvoiceRows = [
                new MinimaxIssuedInvoiceRow
                {
                    SerialNumber ="J1", // concider orderId-jobInstanceId
                    RowNumber = 1,
                    Item = new MinimaxEntityReference
                    {
                      Id = minimaxItem.ItemId,
                      Name = minimaxItem.Title
                    },
                    Quantity = 1,
                    VatRate = new MinimaxEntityReference
                    {
                        Id = 1,
                    },
                },
            ],
            IssuedInvoicePaymentMethods = [
                new MinimaxIssuedInvoicePaymentMethod
                {
                    PaymentMethod = new MinimaxEntityReference {
                        Id =  paymentMethod.PaymentMethodId
                    },
                    Amount =  150616.47,
                    AlreadyPaid = "D"
                }
            ]
        };

        var customer = await _minimaxService.CreateIssuedInvoice(minimaxCustomer);
        return Ok(customer);
    }


}