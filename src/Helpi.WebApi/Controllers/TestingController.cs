
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
    private readonly ILogger<TestingController> _logger;


    public TestingController(INotificationService notificationService,
     JobRequestService jobRequestService,
      OrdersService ordersService,
       IMinimaxService minimaxService,
      ILogger<TestingController> logger
      )
    {
        _notificationService = notificationService;
        _jobRequestService = jobRequestService;
        _ordersService = ordersService;
        _minimaxService = minimaxService;
        _logger = logger;
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
    [HttpGet("minimax/report-templates")]
    public async Task<ActionResult<MinimaxReportTemplate>> GetReportTemplates()
    {

        var c = await _minimaxService.GetReportTemplates();
        return Ok(c);
    }
    [HttpGet("minimax/vatrates")]
    public async Task<ActionResult<MinimaxVatRate>> GetVatRates()
    {

        var c = await _minimaxService.GetVatRates();
        return Ok(c);
    }
    [HttpGet("minimax/document-numbering")]
    public async Task<ActionResult<List<MinimaxDocumentNumbering>>> GetDocumentNumbering()
    {

        var c = await _minimaxService.GetDocumentNumbering();
        return Ok(c);
    }
    [HttpGet("minimax/employees")]
    public async Task<ActionResult<List<MinimaxEmployee>>> GetEmployees()
    {

        var c = await _minimaxService.GetEmployees();
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
    public async Task<ActionResult<MinimaxIssuedInvoice>> CreateIssuedInvoice()
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
                Id = 6,
                Name = "N",
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

        var employee = new MinimaxEmployee
        {
            EmployeeId = 205878,
            FirstName = "Marko",
            LastName = "Strugar",
            DateOfBirth = null,
            TaxNumber = null,
            EmploymentType = "ZD",
            EmploymentStartDate = null,
            EmploymentEndDate = null,
            Country = new MinimaxEntityReference
            {
                Id = 95,
                Name = "HR",
                ResourceUrl = "/api/orgs/39503/countries/95"
            },
            CountryOfResidence = new MinimaxEntityReference
            {
                Id = 95,
                Name = "HR",
                ResourceUrl = "/api/orgs/39503/countries/95"
            }
        };


        double hourlyPrice = 500;
        double jobHours = 3;

        var documentNumbering = new MinimaxDocumentNumbering
        {
            DocumentNumberingId = 39152,
            Document = "IR",
            Code = "/AK/1",
            Name = "Fiskalni lažni",
            Default = "D",
            Reverse = null,
            ReferenceNumber = "00",
            PackagingDepositReturnIncludedInPrice = null,
            Usage = "D",
            RecordDtModified = DateTime.Parse("2025-05-29T11:20:58.82"),
            RowVersion = "AAAAAWlW5aE="
        };


        var vatrate = new MinimaxVatRate
        {
            VatRateId = 6,
            Code = "N",
            Percent = 0,
            VatRatePercentage = new MinimaxEntityReference
            {
                Id = 12
            }
        };
        var minimaxCustomer = new MinimaxIssuedInvoice
        {
            InvoiceType = "R",
            PaymentType = "T",
            // InvoiceNumber = "236541",
            DocumentNumbering = new MinimaxEntityReference
            {
                Id = documentNumbering.DocumentNumberingId
            },
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
            AddresseeCountryName = country.Name,
            AddresseePostalCode = "1000",
            AddresseeCountry = country,

            Currency = currency,
            Employee = new MinimaxEntityReference
            {
                Id = employee.EmployeeId
            },
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
                    ItemCode = minimaxItem.Code,
                    Quantity = jobHours,
                    UnitOfMeasurement = minimaxItem.UnitOfMeasurement,
                    Price = hourlyPrice,
                    VatRate = new MinimaxEntityReference
                    {
                        Id = vatrate.VatRateId,
                    },
                },
            ],
            IssuedInvoicePaymentMethods = [
                new MinimaxIssuedInvoicePaymentMethod
                {
                    PaymentMethod = new MinimaxEntityReference {
                        Id =  paymentMethod.PaymentMethodId
                    },
                    Amount =  hourlyPrice,
                    AlreadyPaid = "D"
                }
            ]
        };



        var issuedInvoiceId = await _minimaxService.CreateIssuedInvoice(minimaxCustomer);

        var invoice = await _minimaxService.GetIssuedInvoiceByIdAsync((int)issuedInvoiceId);


        var issueSuccess = await _minimaxService.CustomActionIssuedInvoice((int)issuedInvoiceId, invoice!.RowVersion!, "issue");

        var emailSuccess = await _minimaxService.CustomActionIssuedInvoice((int)issuedInvoiceId, invoice!.RowVersion!, "sendEInvoice");


        return Ok(invoice);
    }


}



