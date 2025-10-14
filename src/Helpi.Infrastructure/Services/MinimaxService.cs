
using System.Text.Json;
using Helpi.Application.DTOs.Minimax;
using Helpi.Application.Interfaces;
using Helpi.Application.Interfaces.Services;
using Helpi.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using File = System.IO.File;

namespace Helpi.Infrastructure.Services;

public class MinimaxService : IMinimaxService
{
    private readonly IApiService _apiService;
    private readonly IPaymentProfileRepository _paymentProfileRepo;
    private readonly IConfiguration _configuration;

    private readonly ILogger<MinimaxService> _logger;


    private readonly string _baseUrl = "https://moj.minimax.hr/HR/api/api";
    private readonly string _authUrl = "https://moj.minimax.hr/HR/AUT/oauth20/token";


    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _username;
    private readonly string _password;

    private string? _cachedAccessToken;

    /// <summary>
    /// -----------------
    /// </summary>

    private int _organisationId = 41293; // Apoyo

    private static readonly MinimaxEntityReference Country_HR = new() { Id = 95, Name = "HRVATSKA" };
    private static readonly MinimaxEntityReference Currency_EUR = new() { Id = 7, Name = "Euro" };
    private static readonly MinimaxEntityReference Vatrate_0 = new() { Id = 6, Name = "N" };
    private static readonly MinimaxEntityReference PaymentMethod_Transactional = new() { Id = 173739, Name = "Transactional Account" };
    private static readonly MinimaxEntityReference DocumentNumbering_Default = new() { Id = 40866 };

    /// ---------------------------
    public MinimaxService(IApiService apiService,
    IPaymentProfileRepository paymentProfileRepo,
     IConfiguration configuration, ILogger<MinimaxService> logger)
    {

        _apiService = apiService;
        _paymentProfileRepo = paymentProfileRepo;
        _configuration = configuration;
        _logger = logger;

        var minimaxCredentialsPath = Environment.GetEnvironmentVariable("MINIMAX_CREDENTIALS_JSON")
                           ?? configuration["MINIMAX:CredentialsJson"];

        if (string.IsNullOrEmpty(minimaxCredentialsPath))
        {
            throw new InvalidOperationException("MINIMAX credentials not found in environment variables.");
        }

        if (!File.Exists(minimaxCredentialsPath))
        {
            throw new FileNotFoundException($"MINIMAX credentials file not found at {minimaxCredentialsPath}");
        }

        var minimaxCredentialsJson = File.ReadAllText(minimaxCredentialsPath);
        using var jsonDoc = JsonDocument.Parse(minimaxCredentialsJson);
        var root = jsonDoc.RootElement;

        //-- 

        _clientId = root.GetProperty("ClientID").GetString()
        ?? throw new ArgumentNullException("Minimax:ClientID");

        _clientSecret = root.GetProperty("ClientSecret").GetString()
        ?? throw new ArgumentNullException("Minimax:ClientSecret");

        _username = root.GetProperty("Username").GetString()
        ?? throw new ArgumentNullException("Minimax:Username");

        _password = root.GetProperty("Password").GetString()
        ?? throw new ArgumentNullException("Minimax:Password");
    }


    public async Task<MinimaxIssuedInvoice?> ProcessIssuedInvoice(JobInstance job, ContactInfo contact, PaymentProfile paymentProfile)
    {
        try
        {
            _logger.LogInformation("🔧 [Invoice Start] Processing invoice for JobInstance #{JobId}", job.Id);

            var minimaxCustomerId = paymentProfile.MinimaxCustomerId;


            if (minimaxCustomerId == null)
            {
                minimaxCustomerId = await CreateCustomerAndContact(contact);

                if (minimaxCustomerId == null)
                {
                    return null;
                }

                paymentProfile.MinimaxCustomerId = minimaxCustomerId;
                await _paymentProfileRepo.UpdateAsync(paymentProfile);
            }



            var item = await GetItemAsync(); // Simulated 
            var employee = await GetCashierAsync();

            var invoice = BuildIssuedInvoice(job, contact, (int)minimaxCustomerId, item, employee);

            _logger.LogInformation("🧾 [Invoice Constructed] Starting submission to Minimax API...");

            var invoiceId = await CreateIssuedInvoice(invoice);

            if (invoiceId == null)
            {
                return null;
            }

            var fullInvoice = await GetIssuedInvoiceByIdAsync((int)invoiceId);

            if (fullInvoice == null)
            {
                return null;
            }

            _logger.LogInformation("🚀 [Issuing Invoice] ...");

            fullInvoice = await CustomActionIssuedInvoice((int)invoiceId, fullInvoice.RowVersion!, "issue");


            return fullInvoice;
        }
        catch (Exception)
        {
            _logger.LogInformation("❌ [Failed] to proccess minimax invoice ");
            return null;
        }
    }


    ////
    /// ---------------------------------
    /// 


    private async Task<int?> CreateCustomerAndContact(ContactInfo contact)
    {
        try
        {
            _logger.LogInformation("🧑‍💼 [Customer Check] No MinimaxCustomerId found, creating new customer...");

            var customerId = contact.Id;
            var code = GenerateCustomerCode(customerId);

            var newCustomer = new MinimaxCustomer
            {
                Code = code,
                Name = contact.FullName,
                Address = contact.FullAddress,
                PostalCode = contact.PostalCode ?? "",
                City = contact.CityName,
                Country = Country_HR
            };

            var minimaxCustomerId = await CreateCustomer(newCustomer);

            if (minimaxCustomerId == null)
            {
                return null;
            }



            MinimaxContact minimaxContact = new MinimaxContact
            {
                ContactId = 0,
                Customer = new MinimaxEntityReference
                {
                    Id = (int)minimaxCustomerId,
                },
                FullName = contact.FullName,
                Email = contact.Email ?? ""
            };

            var minimaxContactId = await AddCustomerContact(minimaxContact);

            if (minimaxContactId == null)
            {
                return null;
            }




            return minimaxCustomerId;
        }
        catch (Exception)
        {

            _logger.LogInformation("❌ Failed to create customer and contact");
            return null;
        }
    }

    private string GenerateCustomerCode(int customerId)
    {
        return $"CUST{DateTime.UtcNow:yyMMdd}-{customerId:000}";
    }




    public async Task<string> getAccessToken()
    {
        if (!string.IsNullOrEmpty(_cachedAccessToken))
        {
            return _cachedAccessToken;
        }


        var tokenResponse = await _apiService.AuthenticateAsync(
             _authUrl,
             _clientId,
             _clientSecret,
             _username,
             _password
         );

        _cachedAccessToken = tokenResponse.AccessToken;

        return _cachedAccessToken;
    }

    public async Task<int?> CreateCustomer(MinimaxCustomer minimaxCustomer)
    {
        try
        {
            _cachedAccessToken = await getAccessToken();


            string json = JsonConvert.SerializeObject(minimaxCustomer, Formatting.Indented);

            var url = $"{_baseUrl}/orgs/{_organisationId}/customers";


            var result = await _apiService.MinimaxPostRawAsync(
                      url,
                   _cachedAccessToken,
                 json
                   );

            int customerId = int.Parse(result);

            _logger.LogInformation($"✅ New Customer {customerId}");

            return customerId;
        }
        catch (Exception)
        {
            _logger.LogInformation($"❌ Failed top to create New Customer");
            return null;
        }

    }

    public async Task<MinimaxCustomer?> GetCustomerById(int id)
    {

        _cachedAccessToken = await getAccessToken();

        var url = $"{_baseUrl}/orgs/{_organisationId}/customers/{id}";

        var result = await _apiService.GetRawAsync(url, _cachedAccessToken);

        return JsonConvert.DeserializeObject<MinimaxCustomer>(result);
    }
    public async Task<MinimaxCustomer?> GetCustomerByCode(string code)
    {

        _cachedAccessToken = await getAccessToken();

        var url = $"{_baseUrl}/orgs/{_organisationId}/customers/code({code})";

        var result = await _apiService.GetRawAsync(url, _cachedAccessToken);

        return JsonConvert.DeserializeObject<MinimaxCustomer>(result);
    }

    public async Task<int?> AddCustomerContact(MinimaxContact minimaxContact)
    {
        try
        {
            _cachedAccessToken = await getAccessToken();


            string json = JsonConvert.SerializeObject(minimaxContact, Formatting.Indented);

            var customerId = minimaxContact.Customer.Id;

            var url = $"{_baseUrl}/orgs/{_organisationId}/customers/{customerId}/contacts";


            var result = await _apiService.MinimaxPostRawAsync(
                      url,
                   _cachedAccessToken,
                 json
                   );


            int contactId = int.Parse(result);

            _logger.LogInformation($"✅ New Contact {contactId}");

            return contactId;
        }
        catch (Exception)
        {
            _logger.LogInformation($"❌ Failed top to create New Contact");
            return null;
        }
    }

    public async Task<List<MinimaxCurrency>> GetCurrencies()
    {

        _cachedAccessToken = await getAccessToken();

        var url = $"{_baseUrl}/orgs/{_organisationId}/currencies";

        var result = await _apiService.GetAsync(url, _cachedAccessToken);

        return JsonConvert.DeserializeObject<List<MinimaxCurrency>>($"{result["Rows"]}") ?? [];
    }

    public async Task<MinimaxCurrency?> GetCurrencyByCode(string code)
    {
        _cachedAccessToken = await getAccessToken();

        var url = $"{_baseUrl}/orgs/{_organisationId}/currencies/code({code})";

        var result = await _apiService.GetRawAsync(url, _cachedAccessToken);

        return JsonConvert.DeserializeObject<MinimaxCurrency>(result);
    }

    public async Task<List<MinimaxCountry>> GetCountries()
    {
        _cachedAccessToken = await getAccessToken();

        var url = $"{_baseUrl}/orgs/{_organisationId}/countries";

        var result = await _apiService.GetAsync(url, _cachedAccessToken);

        return JsonConvert.DeserializeObject<List<MinimaxCountry>>($"{result["Rows"]}") ?? [];
    }

    public async Task<MinimaxCountry?> GetCountryByCode(string code)
    {
        _cachedAccessToken = await getAccessToken();

        var url = $"{_baseUrl}/orgs/{_organisationId}/countries/code({code})";

        var result = await _apiService.GetRawAsync(url, _cachedAccessToken);

        return JsonConvert.DeserializeObject<MinimaxCountry>(result);
    }

    public async Task<List<MinimaxItem>> GetItems()
    {
        _cachedAccessToken = await getAccessToken();

        var url = $"{_baseUrl}/orgs/{_organisationId}/items";

        var result = await _apiService.GetAsync(url, _cachedAccessToken);

        return JsonConvert.DeserializeObject<List<MinimaxItem>>($"{result["Rows"]}") ?? [];
    }

    public async Task<int?> CreateIssuedInvoice(MinimaxIssuedInvoice issuedInvoice)
    {
        try
        {

            _cachedAccessToken = await getAccessToken();


            string json = JsonConvert.SerializeObject(issuedInvoice, Formatting.Indented);



            var url = $"{_baseUrl}/orgs/{_organisationId}/issuedInvoices";


            var result = await _apiService.MinimaxPostRawAsync(
                      url,
                   _cachedAccessToken,
                 json
                   );

            int invoiceId = int.Parse(result);

            _logger.LogInformation("📨 [Invoice Created] ID: {InvoiceId}", invoiceId);


            return invoiceId;
        }
        catch (Exception)
        {
            _logger.LogInformation($"❌  Failed to create issued invoice");
            return null;
        }
    }

    public async Task<MinimaxIssuedInvoice?> GetIssuedInvoiceByIdAsync(int issuedInvoiceId)
    {
        try
        {
            _cachedAccessToken = await getAccessToken();
            var url = $"{_baseUrl}/orgs/{_organisationId}/issuedinvoices/{issuedInvoiceId}";

            var result = await _apiService.GetAsync(url, _cachedAccessToken);

            return JsonConvert.DeserializeObject<MinimaxIssuedInvoice>(result.ToString()) ?? null;
        }
        catch (Exception)
        {
            _logger.LogInformation($"❌  Failed to GET issued invoice");
            return null;
        }
    }



    public async Task<List<MinimaxPaymentMethod>> GetPaymentMethods()
    {

        _cachedAccessToken = await getAccessToken();

        var url = $"{_baseUrl}/orgs/{_organisationId}/paymentMethods";

        var result = await _apiService.GetAsync(url, _cachedAccessToken);

        return JsonConvert.DeserializeObject<List<MinimaxPaymentMethod>>($"{result["Rows"]}") ?? [];
    }

    public async Task<List<MinimaxReportTemplate>> GetReportTemplates()
    {
        _cachedAccessToken = await getAccessToken();

        var url = $"{_baseUrl}/orgs/{_organisationId}/report-templates";

        var result = await _apiService.GetAsync(url, _cachedAccessToken);

        return JsonConvert.DeserializeObject<List<MinimaxReportTemplate>>($"{result["Rows"]}") ?? [];
    }

    public async Task<List<MinimaxVatRate>> GetVatRates()
    {
        _cachedAccessToken = await getAccessToken();

        var url = $"{_baseUrl}/orgs/{_organisationId}/vatrates";

        var result = await _apiService.GetAsync(url, _cachedAccessToken);


        return JsonConvert.DeserializeObject<List<MinimaxVatRate>>($"{result["Rows"]}") ?? [];
    }

    public async Task<List<MinimaxDocumentNumbering>> GetDocumentNumbering()
    {
        _cachedAccessToken = await getAccessToken();

        var url = $"{_baseUrl}/orgs/{_organisationId}/document-numbering";

        var result = await _apiService.GetAsync(url, _cachedAccessToken);

        return JsonConvert.DeserializeObject<List<MinimaxDocumentNumbering>>($"{result["Rows"]}") ?? [];
    }


    public async Task<MinimaxIssuedInvoice?> CustomActionIssuedInvoice(int issuedInvoiceId, string rowVersion, string actionName)
    {
        try
        {

            _cachedAccessToken = await getAccessToken();

            var url = $"{_baseUrl}/orgs/{_organisationId}/issuedinvoices/{issuedInvoiceId}/actions/{actionName}?rowVersion={rowVersion}";

            string json = JsonConvert.SerializeObject(new { }, Formatting.Indented);
            var result = await _apiService.PutAsync(url, _cachedAccessToken, json);


            return JsonConvert.DeserializeObject<MinimaxIssuedInvoice>($"{result["Data"]}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"❌ [CustomActionIssuedInvoice] {actionName}");
            return null;
        }
    }

    public async Task<List<MinimaxEmployee>> GetEmployees()
    {
        _cachedAccessToken = await getAccessToken();

        var url = $"{_baseUrl}/orgs/{_organisationId}/employees";

        var result = await _apiService.GetAsync(url, _cachedAccessToken);

        return JsonConvert.DeserializeObject<List<MinimaxEmployee>>($"{result["Rows"]}") ?? [];
    }

    public async Task<List<dynamic>> GetCurrentUserOrganisations()
    {

        _cachedAccessToken = await getAccessToken();

        var url = $"{_baseUrl}/currentuser/orgs";

        var result = await _apiService.GetAsync(url, _cachedAccessToken);

        // _logger.LogInformation("📡 GET {Url}", result);

        /// TODO:  returns corrent data but not parsed , so look at result in debugger
        return JsonConvert.DeserializeObject<List<dynamic>>($"{result["Rows"]}") ?? [];
    }







    private MinimaxIssuedInvoice BuildIssuedInvoice(JobInstance job, ContactInfo contact, int minimaxCustomerId, MinimaxItem item,
    MinimaxEmployee employee)
    {
        var now = DateTime.UtcNow;
        double duration = (double)job.DurationHours;
        double amount = (double)job.TotalAmount;

        return new MinimaxIssuedInvoice
        {
            InvoiceType = "R",
            PaymentType = "T",
            DocumentNumbering = DocumentNumbering_Default,
            Customer = new MinimaxEntityReference { Id = minimaxCustomerId },
            DateDue = now,
            DateIssued = now,
            DateTransaction = now,
            AddresseeName = contact.FullName,
            AddresseeAddress = contact.FullAddress,
            AddresseeCity = contact.CityName,
            AddresseePostalCode = contact.PostalCode ?? "",
            AddresseeCountry = Country_HR,
            AddresseeCountryName = Country_HR.Id == 95 ? null : Country_HR.Name,
            Currency = Currency_EUR,
            Employee = new MinimaxEntityReference { Id = employee.EmployeeId },
            IssuedInvoiceRows = [
                new MinimaxIssuedInvoiceRow
                {
                    SerialNumber = $"O{job.OrderId}-J{job.Id}",
                    RowNumber = 1,
                    Item = new MinimaxEntityReference { Id = item.ItemId, Name = item.Title },
                    ItemCode = item.Code,
                    Quantity = duration,
                    UnitOfMeasurement = item.UnitOfMeasurement,
                    Price = (double)job.HourlyRate,
                    VatRate = Vatrate_0
                }
            ]
            ,
            IssuedInvoicePaymentMethods = [
                new MinimaxIssuedInvoicePaymentMethod
                {
                    PaymentMethod = PaymentMethod_Transactional,
                    Amount = amount,
                    AlreadyPaid = "D"
                }
            ]
        };
    }

    private async Task<MinimaxItem> GetItemAsync()
    {
        try
        {

            var items = await GetItems();

            var item = items.First();
            // 
            return new MinimaxItem
            {
                ItemId = item.ItemId,
                Title = item.Title,
                Code = item.Code,
                UnitOfMeasurement = item.UnitOfMeasurement,
                ItemType = item.ItemType,
                VatRate = Vatrate_0,
                Price = item.Price,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting minimax item");
            throw;
        }
    }
    private async Task<MinimaxEmployee> GetCashierAsync()
    {

        try
        {
            var employees = await GetEmployees();

            var employee = employees.Find(e => e.FirstName.ToLower() == "marko" && e.LastName.ToLower() == "strugar");

            return employee!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting minimax cashier");
            throw;
        }
        // 
        // return new MinimaxEmployee
        // {
        //     EmployeeId = 225220,
        //     FirstName = "Marko",
        //     LastName = "Strugar",
        //     DateOfBirth = null,
        //     TaxNumber = null,
        //     EmploymentType = "ZD",
        //     EmploymentStartDate = null,
        //     EmploymentEndDate = null,
        //     Country = new MinimaxEntityReference
        //     {
        //         Id = 95,
        //         Name = "HR",
        //         ResourceUrl = $"/api/orgs/{_organisationId}/countries/95"
        //     },
        //     CountryOfResidence = new MinimaxEntityReference
        //     {
        //         Id = 95,
        //         Name = "HR",
        //         ResourceUrl = $"/api/orgs/{_organisationId}/countries/95"
        //     }
        // };
    }

    public async Task<MinimaxAttachment?> GenerateInvoicePdf(int issuedInvoiceId, string rowVersion)
    {
        var actionName = "generatepdf";
        try
        {


            _cachedAccessToken = await getAccessToken();

            var url = $"{_baseUrl}/orgs/{_organisationId}/issuedinvoices/{issuedInvoiceId}/actions/{actionName}?rowVersion={rowVersion}";

            string json = JsonConvert.SerializeObject(new { }, Formatting.Indented);
            var result = await _apiService.PutAsync(url, _cachedAccessToken, json);

            _logger.LogInformation($"{result}");
            return JsonConvert.DeserializeObject<MinimaxAttachment>($"{result["Data"]}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"❌ [GetInvoicePdf] {actionName}");
            return null;
        }
    }

    public async Task<List<MinimaxIssuedInvoice>> GetAllIssuedInvoicesAsync()
    {
        try
        {
            _cachedAccessToken = await getAccessToken();
            var url = $"{_baseUrl}/orgs/{_organisationId}/issuedinvoices";

            var result = await _apiService.GetAsync(url, _cachedAccessToken);

            return JsonConvert.DeserializeObject<List<MinimaxIssuedInvoice>>($"{result["Rows"]}") ?? null;
        }
        catch (Exception)
        {
            _logger.LogInformation($"❌  Failed to GET all issued invoicea");
            return null;
        }
    }



}