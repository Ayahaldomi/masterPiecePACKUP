using PayPal.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterPiece.PayPal
{
    public class PayPalHelper
    {
        public static Payment CreatePayment(string redirectUrl, string cancelUrl, decimal amount)
        {
            var apiContext = GetApiContext();
            var payer = new Payer() { payment_method = "paypal" };

            var redirectUrls = new RedirectUrls()
            {
                cancel_url = cancelUrl,
                return_url = redirectUrl
            };

            var details = new Details() { tax = "0", shipping = "0", subtotal = amount.ToString("F") };

            var amountToPay = new Amount()
            {
                currency = "USD",
                total = amount.ToString("F"), // Total must be equal to the sum of details
                details = details
            };

            var transactionList = new List<Transaction>
        {
            new Transaction
            {
                description = "Payment for medical services",
                invoice_number = new Random().Next(100000).ToString(), // Random invoice number
                amount = amountToPay
            }
        };

            var payment = new Payment()
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirectUrls
            };

            return payment.Create(apiContext);
        }

        public static Payment ExecutePayment(string paymentId, string payerId)
        {
            var apiContext = GetApiContext();
            var paymentExecution = new PaymentExecution() { payer_id = payerId };
            var payment = new Payment() { id = paymentId };

            return payment.Execute(apiContext, paymentExecution);
        }

        private static APIContext GetApiContext()
        {
            var config = new Dictionary<string, string>
        {
            { "clientId", System.Configuration.ConfigurationManager.AppSettings["PayPalClientId"] },
            { "clientSecret", System.Configuration.ConfigurationManager.AppSettings["PayPalClientSecret"] },
            { "mode", System.Configuration.ConfigurationManager.AppSettings["PayPalMode"] }
        };

            var accessToken = new OAuthTokenCredential(config).GetAccessToken();
            return new APIContext(accessToken);
        }
    }
}