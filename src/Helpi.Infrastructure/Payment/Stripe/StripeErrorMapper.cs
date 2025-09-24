
using Helpi.Application.Interfaces.Services;
using Stripe;

namespace Helpi.Infrastructure.Payment.Stripe
{
    public class StripeErrorMapper : IPaymentErrorMapper
    {
        public StripeErrorMapper() { }

        public string GetLocalizedErrorMessage(StripeException ex, string language = "en")
        {

            return language switch
            {
                "hr" => GetCroatianErrorMessage(ex),
                _ => GetEnglishErrorMessage(ex)
            };
        }


        private static string GetEnglishErrorMessage(StripeException ex)
        {
            return ex.StripeError?.Code switch
            {
                "card_declined" => "Your card was declined. Please try a different payment method.",
                "insufficient_funds" => "Your card has insufficient funds. Please try a different payment method.",
                "expired_card" => "Your card has expired. Please update your payment information.",
                "incorrect_cvc" => "Your card's security code is incorrect. Please check and try again.",
                "processing_error" => "An error occurred while processing your card. Please try again.",
                "rate_limit" => "Too many requests. Please wait a moment and try again.",
                _ => "Your payment could not be processed. Please try again or contact support."
            };
        }
        public static string GetCroatianErrorMessage(StripeException ex)
        {
            return ex.StripeError?.Code switch
            {
                "card_declined" => "Vaša kartica je odbijena. Pokušajte s drugom karticom.",
                "insufficient_funds" => "Na kartici nemate dovoljno sredstava. Pokušajte s drugom karticom.",
                "expired_card" => "Vaša kartica je istekla. Ažurirajte podatke o plaćanju.",
                "incorrect_cvc" => "Sigurnosni kod kartice nije ispravan. Provjerite i pokušajte ponovno.",
                "processing_error" => "Došlo je do pogreške prilikom obrade kartice. Pokušajte ponovno.",
                "rate_limit" => "Previše zahtjeva. Pričekajte malo i pokušajte ponovno.",
                _ => "Plaćanje nije uspjelo. Pokušajte ponovno ili kontaktirajte podršku."
            };
        }



    }
}