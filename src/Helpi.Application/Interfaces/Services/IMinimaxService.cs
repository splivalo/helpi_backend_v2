using Helpi.Application.DTOs.Minimax;
using Helpi.Domain.Entities;

namespace Helpi.Application.Interfaces.Services;

public interface IMinimaxService
{

    Task<MinimaxIssuedInvoice?> ProcessIssuedInvoice(JobInstance job, ContactInfo contact, PaymentProfile paymentProfile);
    public Task<string> getAccessToken();
    public Task<int?> CreateCustomer(MinimaxCustomer minimaxCustomer);
    public Task<MinimaxCustomer?> GetCustomerByCode(string code);
    public Task<MinimaxCustomer?> GetCustomerById(int id);
    public Task<int?> AddCustomerContact(MinimaxContact minimaxContact);

    public Task<int?> CreateIssuedInvoice(MinimaxIssuedInvoice issuedInvoice);

    public Task<MinimaxIssuedInvoice?> GetIssuedInvoiceByIdAsync(int issuedInvoiceId);
    public Task<bool> CustomActionIssuedInvoice(int issuedInvoiceId, string rowVersion, string actionName);
    public Task<List<MinimaxCurrency>> GetCurrencies();
    public Task<MinimaxCurrency?> GetCurrencyByCode(string code);
    public Task<List<MinimaxCountry>> GetCountries();
    public Task<MinimaxCountry?> GetCountryByCode(string code);
    public Task<List<MinimaxItem>> GetItems();
    public Task<List<MinimaxEmployee>> GetEmployees();
    public Task<List<MinimaxPaymentMethod>> GetPaymentMethods();
    public Task<List<MinimaxReportTemplate>> GetReportTemplates();
    public Task<List<MinimaxVatRate>> GetVatRates();
    public Task<List<MinimaxDocumentNumbering>> GetDocumentNumbering();



}