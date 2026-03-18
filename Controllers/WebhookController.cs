using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SarEquipEnterprise.Models;
using SarEquipEnterprise.Services;

namespace SarEquipEnterprise.Controllers
{
    [Route("webhook")]
    [ApiController]
    public class WebhookController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IPaymentService _paymentService;
        private readonly IInvoiceService _invoiceService;

        public WebhookController(IConfiguration configuration, IPaymentService paymentService, IInvoiceService invoiceService)
        {
            _configuration = configuration;
            _paymentService = paymentService;
            _invoiceService = invoiceService;
        }

        [HttpPost]
        public async Task<IActionResult> PayMongoWebhook()
        {
            try
            {
                using var reader = new StreamReader(Request.Body);
                var webhookBody = await reader.ReadToEndAsync();
                
                // TODO: Verify webhook signature using _configuration["PayMongo:WebhookSecret"]
                // var signature = Request.Headers["PayMongo-Signature"].FirstOrDefault();
                
                var webhookData = System.Text.Json.JsonSerializer.Deserialize<PayMongoWebhookData>(webhookBody);
                
                if (webhookData?.data?.attributes?.type == "checkout_session.payment.paid")
                {
                    // The reference_number was set to the invoiceId
                    var invoiceIdStr = webhookData.data.attributes.data?.attributes?.reference_number;
                    if (int.TryParse(invoiceIdStr, out int invoiceId))
                    {
                        var invoice = await _invoiceService.GetInvoiceByIdAsync(invoiceId);
                        if (invoice != null && invoice.Status != "Paid")
                        {
                            var payment = new Payment
                            {
                                InvoiceId = invoiceId,
                                Amount = invoice.TotalAmount,
                                PaymentMethod = "PayMongo",
                                ReferenceNumber = webhookData.data.attributes.data?.id ?? "PayMongo",
                                Notes = "Paid via PayMongo Checkout"
                            };

                            // This will also update the invoice status to Paid if the amount is fully covered
                            await _paymentService.ProcessPaymentAsync(payment);
                        }
                    }
                    
                    return Ok();
                }
                
                return Ok();
            }
            catch (Exception ex)
            {
                // Log exception
                return StatusCode(500, ex.Message);
            }
        }
    }
}
