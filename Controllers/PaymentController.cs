using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using SarEquipEnterprise.Models;
using SarEquipEnterprise.Services;

namespace SarEquipEnterprise.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IInvoiceService _invoiceService;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public PaymentController(IPaymentService paymentService, IInvoiceService invoiceService, IConfiguration configuration)
        {
            _paymentService = paymentService;
            _invoiceService = invoiceService;
            _configuration = configuration;
            _httpClient = new HttpClient();
        }

        public async Task<IActionResult> Index()
        {
            var payments = await _paymentService.GetAllPaymentsAsync();
            var activePayments = payments.Where(p => !p.IsArchived && p.PaymentStatus != "Archived").ToList();
            return View(activePayments);
        }

        public async Task<IActionResult> Details(int id)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            if (payment == null)
                return NotFound();

            return View(payment);
        }

        public async Task<IActionResult> Create(int? invoiceId)
        {
            var payment = new Payment
            {
                PaymentDate = DateTime.Now,
                PaymentStatus = "Pending"
            };

            if (invoiceId.HasValue)
            {
                var invoice = await _invoiceService.GetInvoiceByIdAsync(invoiceId.Value);
                if (invoice != null)
                {
                    payment.InvoiceId = invoice.InvoiceId;
                    payment.Amount = invoice.TotalAmount;
                    payment.Invoice = invoice;
                }
            }

            var invoices = await _invoiceService.GetAllInvoicesAsync();
            ViewBag.Invoices = invoices.Where(i => i.Status != "Archived").ToList();
            return View(payment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Payment payment)
        {
            if (ModelState.IsValid)
            {
                await _paymentService.ProcessPaymentAsync(payment);
                return RedirectToAction(nameof(Index));
            }

            var invoices = await _invoiceService.GetAllInvoicesAsync();
            ViewBag.Invoices = invoices.Where(i => i.Status != "Archived").ToList();
            return View(payment);
        }

        public async Task<IActionResult> Refund(int id)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            if (payment == null)
                return NotFound();

            return View(payment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Refund(int id, decimal refundAmount)
        {
            await _paymentService.RefundPaymentAsync(id, refundAmount);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Archive(int id)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            if (payment == null)
                return NotFound();

            return View("Archive", payment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            await _paymentService.ArchivePaymentAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayWithPayMongo(int invoiceId)
        {
            try
            {
                var invoice = await _invoiceService.GetInvoiceByIdAsync(invoiceId);
                if (invoice == null || invoice.Status == "Paid")
                {
                    return Json(new { success = false, message = "Invalid or already paid invoice" });
                }

                var checkoutSession = await CreatePayMongoCheckout(invoice.TotalAmount, $"Payment for Invoice #{invoice.InvoiceNumber}", invoiceId);
        
                if (checkoutSession?.data?.attributes?.checkout_url == null)
                {
                    return Json(new { success = false, message = "Failed to create payment session" });
                }

                // Redirect user to PayMongo Secure Checkout
                return Redirect(checkoutSession.data.attributes.checkout_url);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        public IActionResult PaymentSuccess(int invoiceId, string? sessionId)
        {
            ViewBag.Message = $"Payment sequence initiated for Invoice ID: {invoiceId}. The payment will be verified shortly.";
            return View(); // Assuming a PaymentSuccess view will be created or you can return Content/Redirect
        }

        private async Task<PayMongoCheckoutResponse?> CreatePayMongoCheckout(decimal amount, string description, int invoiceId)
        {
            var secretKey = _configuration["PayMongo:SecretKey"];
            var baseUrl = "https://api.paymongo.com/v1";
            
            var authToken = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{secretKey}:"));
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);

            var payload = new
            {
                data = new
                {
                    attributes = new
                    {
                        line_items = new[]
                        {
                            new
                            {
                                name = description,
                                amount = (int)(amount * 100), // Convert to centavos
                                currency = "PHP",
                                quantity = 1
                            }
                        },
                        payment_method_types = new[] { "card", "gcash", "paymaya" },
                        // reference_number is used to identify the invoice in the webhook
                        reference_number = invoiceId.ToString(), 
                        success_url = $"{Request.Scheme}://{Request.Host}/Payment/PaymentSuccess?invoiceId={invoiceId}",
                        cancel_url = $"{Request.Scheme}://{Request.Host}/Payment/Create?invoiceId={invoiceId}",
                        description = description
                    }
                }
            };

            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(payload),
                System.Text.Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync($"{baseUrl}/checkout_sessions", content);
            
            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var checkoutData = System.Text.Json.JsonSerializer.Deserialize<PayMongoCheckoutResponse>(jsonResponse);
                return checkoutData;
            }

            return null;
        }
    }
}
