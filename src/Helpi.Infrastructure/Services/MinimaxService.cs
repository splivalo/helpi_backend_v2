
using Helpi.Application.DTOs.Minimax;
using Helpi.Application.Interfaces;
using Helpi.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stripe;

namespace Helpi.Infrastructure.Services;

public class MinimaxService : IMinimaxService
{
    private readonly IApiService _apiService;
    private readonly IConfiguration _configuration;

    private readonly ILogger<MinimaxService> _logger;


    private readonly string _baseUrl = "https://moj.minimax.hr/HR/api/api";
    private readonly string _authUrl = "https://moj.minimax.hr/HR/AUT/oauth20/token";


    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _username;
    private readonly string _password;

    private string? _cachedAccessToken;
    private int _organisationId = 39503; // Apoyo
    private int _itemId = 2878516;

    public MinimaxService(IApiService apiService, IConfiguration configuration, ILogger<MinimaxService> logger)
    {

        _apiService = apiService;
        _configuration = configuration;
        _logger = logger;


        _clientId = Environment.GetEnvironmentVariable("Minimax:ClientID")
        ?? _configuration["Minimax:ClientID"]
        ?? throw new ArgumentNullException("Minimax:ClientID");

        _clientSecret = Environment.GetEnvironmentVariable("Minimax:ClientSecret")
         ?? _configuration["Minimax:ClientSecret"]
         ?? throw new ArgumentNullException("Minimax:ClientSecret");

        _username = Environment.GetEnvironmentVariable("Minimax:Username")
         ?? _configuration["Minimax:Username"]
         ?? throw new ArgumentNullException("Minimax:Username");

        _password = Environment.GetEnvironmentVariable("Minimax:Password")
        ?? _configuration["Minimax:Password"]
        ?? throw new ArgumentNullException("Minimax:Password");
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

    public async Task<MinimaxCustomer?> CreateCustomer(MinimaxCustomer minimaxCustomer)
    {
        _cachedAccessToken = await getAccessToken();


        string json = JsonConvert.SerializeObject(minimaxCustomer, Formatting.Indented);

        var url = $"{_baseUrl}/orgs/{_organisationId}/customers";


        var result = await _apiService.PostRawAsync(
                  url,
               _cachedAccessToken,
             json
               );

        var customer = JsonConvert.DeserializeObject<MinimaxCustomer>(result);

        return customer;

    }

    public async Task<MinimaxCustomer?> GetCustomerByCode(string code)
    {

        _cachedAccessToken = await getAccessToken();

        var url = $"{_baseUrl}/orgs/{_organisationId}/customers/code({code})";

        var result = await _apiService.GetRawAsync(url, _cachedAccessToken);

        return JsonConvert.DeserializeObject<MinimaxCustomer>(result);
    }

    public async Task<MinimaxContact?> AddCustomerContact(MinimaxContact minimaxContact)
    {
        _cachedAccessToken = await getAccessToken();


        string json = JsonConvert.SerializeObject(minimaxContact, Formatting.Indented);

        var customerId = minimaxContact.Customer.Id;

        var url = $"{_baseUrl}/orgs/{_organisationId}/customers/{customerId}/contacts";


        var result = await _apiService.PostRawAsync(
                  url,
               _cachedAccessToken,
             json
               );

        var contact = JsonConvert.DeserializeObject<MinimaxContact>(result);

        return contact;
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

    public async Task<MinimaxIssuedInvoice?> CreateIssuedInvoice(MinimaxIssuedInvoice issuedInvoice)
    {

        _cachedAccessToken = await getAccessToken();


        string json = JsonConvert.SerializeObject(issuedInvoice, Formatting.Indented);



        var url = $"{_baseUrl}/orgs/{_organisationId}/issuedInvoices";


        var result = await _apiService.PostRawAsync(
                  url,
               _cachedAccessToken,
             json
               );

        _logger.LogInformation(JsonConvert.SerializeObject(result, Formatting.Indented));

        var invoice = JsonConvert.DeserializeObject<MinimaxIssuedInvoice>(result);

        return invoice;
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

}