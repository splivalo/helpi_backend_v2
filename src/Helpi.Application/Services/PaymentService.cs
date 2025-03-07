// // Application/Services/PaymentService.cs
// using Helpi.Application.Interfaces;
// using Helpi.Domain.Enums;

// public class PaymentService
// {
//     private readonly IPaymentTransactionRepository _transactionRepository;
//     // private readonly IPaymentGateway _paymentGateway;

//     public PaymentService(
//         IPaymentTransactionRepository transactionRepository
//         // IPaymentGateway paymentGateway
//         )
//     {
//         _transactionRepository = transactionRepository;
//         // _paymentGateway = paymentGateway;
//     }

//     public async Task ProcessPaymentAsync(PaymentTransactionCreateDto dto)
//     {
//         var transaction = new PaymentTransaction
//         {
//             Amount = dto.Amount,
//             Status = PaymentStatus.Pending,
//             CustomerId = dto.CustomerId
//         };

//         try
//         {
//             var result = await _paymentGateway.ProcessPaymentAsync(dto.Amount, dto.PaymentMethodId);
//             transaction.Status = result.Succeeded ? PaymentStatus.Succeeded : PaymentStatus.Failed;
//         }
//         catch
//         {
//             transaction.Status = PaymentStatus.Failed;
//         }

//         await _transactionRepository.AddAsync(transaction);
//     }
// }