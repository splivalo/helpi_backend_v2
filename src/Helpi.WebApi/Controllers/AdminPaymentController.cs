using Helpi.Application.Interfaces;
using Helpi.Application.Interfaces.Services;
using Helpi.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Helpi.WebApi.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin/payments")]
public class AdminPaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IPaymentTransactionRepository _transactionRepo;
    private readonly ILogger<AdminPaymentController> _logger;

    public AdminPaymentController(
        IPaymentService paymentService,
        IPaymentTransactionRepository transactionRepo,
        ILogger<AdminPaymentController> logger)
    {
        _paymentService = paymentService;
        _transactionRepo = transactionRepo;
        _logger = logger;
    }

    /// <summary>
    /// Retry Minimax invoice creation for a specific paid transaction.
    /// Safe to call multiple times — skips if invoice already created.
    /// </summary>
    [HttpPost("{transactionId}/retry-invoice")]
    public async Task<IActionResult> RetryInvoice(int transactionId)
    {
        var transaction = await _transactionRepo.GetByIdAsync(transactionId);
        if (transaction == null)
            return NotFound(new { error = "Transaction not found." });

        if (transaction.Status != PaymentStatus.Paid)
            return BadRequest(new { error = $"Transaction status is {transaction.Status}, not Paid." });

        if (transaction.InvoiceCreationStatus == InvoiceCreationStatus.Created)
            return Ok(new { message = "Invoice already created.", minimaxInvoiceId = transaction.MinimaxInvoiceId });

        var success = await _paymentService.RetryInvoiceAsync(transactionId);

        if (success)
            return Ok(new { message = "Invoice created successfully." });

        return StatusCode(502, new { error = "Minimax invoice creation failed. Check logs for details." });
    }

    /// <summary>
    /// Get all paid transactions where invoice creation failed.
    /// </summary>
    [HttpGet("failed-invoices")]
    public async Task<IActionResult> GetFailedInvoices()
    {
        var failed = await _transactionRepo.GetFailedInvoiceTransactionsAsync();
        var result = failed.Select(t => new
        {
            t.Id,
            t.OrderId,
            t.JobInstanceId,
            t.CustomerId,
            t.Amount,
            t.Currency,
            t.ProcessedAt,
            t.InvoiceCreationStatus,
            t.InvoiceRetryCount,
            t.MinimaxInvoiceId,
        });
        return Ok(result);
    }
}
