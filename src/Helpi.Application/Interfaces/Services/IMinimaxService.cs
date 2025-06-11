using Helpi.Application.DTOs.Minimax;

namespace Helpi.Application.Interfaces.Services;

public interface IMinimaxService
{

    public Task<string> getAccessToken();
    public Task<MinimaxCustomer?> CreateCustomer(MinimaxCustomer minimaxCustomer);
    public Task<MinimaxCustomer?> GetCustomerByCode(string code);
    public Task<MinimaxContact?> AddCustomerContact(MinimaxContact minimaxContact);

    public Task<MinimaxIssuedInvoice?> CreateIssuedInvoice(MinimaxIssuedInvoice issuedInvoice);
    public Task<List<MinimaxCurrency>> GetCurrencies();
    public Task<MinimaxCurrency?> GetCurrencyByCode(string code);
    public Task<List<MinimaxCountry>> GetCountries();
    public Task<MinimaxCountry?> GetCountryByCode(string code);
    public Task<List<MinimaxItem>> GetItems();
    public Task<List<MinimaxPaymentMethod>> GetPaymentMethods();
    public Task<List<MinimaxReportTemplate>> GetReportTemplates();
    public Task<List<MinimaxVatRate>> GetVatRates();
    public Task<List<MinimaxDocumentNumbering>> GetDocumentNumbering();

    // - get paymentmethods
    // - add customer
    // - add contact
    // - get customer by id
    // - create Invoice
    // - sendEInvoice

}