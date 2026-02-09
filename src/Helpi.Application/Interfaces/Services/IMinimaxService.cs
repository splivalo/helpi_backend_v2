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
    public Task<bool> UpdateCustomer(int customerId, MinimaxCustomer customer);

    public Task<int?> CreateIssuedInvoice(MinimaxIssuedInvoice issuedInvoice);

    public Task<MinimaxIssuedInvoice?> GetIssuedInvoiceByIdAsync(int issuedInvoiceId);
    public Task<MinimaxIssuedInvoice?> CustomActionIssuedInvoice(int issuedInvoiceId, string rowVersion, string actionName);
    public Task<List<MinimaxCurrency>> GetCurrencies();
    public Task<MinimaxCurrency?> GetCurrencyByCode(string code);
    public Task<List<MinimaxCountry>> GetCountries();
    public Task<MinimaxCountry?> GetCountryByCode(string code);
    public Task<List<MinimaxItem>> GetItems();
    public Task<List<MinimaxEmployee>> GetEmployees();
    public Task<List<MinimaxPaymentMethod>> GetPaymentMethods();
    public Task<List<dynamic>> GetCurrentUserOrganisations();
    public Task<List<MinimaxReportTemplate>> GetReportTemplates();
    public Task<List<MinimaxVatRate>> GetVatRates();
    public Task<List<MinimaxDocumentNumbering>> GetDocumentNumbering();

    public Task<List<MinimaxIssuedInvoice>> GetAllIssuedInvoicesAsync();
    public Task<MinimaxAttachment?> GenerateInvoicePdf(int issuedInvoiceId, string rowVersion);

    /// <summary>
    /// Anonymizes a customer in Minimax by replacing personal data with anonymized values.
    /// </summary>
    public Task AnonymizeCustomerAsync(int minimaxCustomerId);
}