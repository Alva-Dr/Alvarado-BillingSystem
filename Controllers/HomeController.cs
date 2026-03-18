using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SarEquipEnterprise.Models;
using SarEquipEnterprise.Services;

namespace SarEquipEnterprise.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IPaymentService _paymentService;
        private readonly ITransactionService _transactionService;
        private readonly IBillingReportService _reportService;
        private readonly ICloudinaryService _cloudinaryService;

        public HomeController(
            IInvoiceService invoiceService,
            IPaymentService paymentService,
            ITransactionService transactionService,
            IBillingReportService reportService,
            ICloudinaryService cloudinaryService)
        {
            _invoiceService = invoiceService;
            _paymentService = paymentService;
            _transactionService = transactionService;
            _reportService = reportService;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<IActionResult> Index()
        {
            var invoices = await _invoiceService.GetAllInvoicesAsync();
            var payments = await _paymentService.GetAllPaymentsAsync();
            var transactions = await _transactionService.GetAllTransactionsAsync();
            var outstandingReport = await _reportService.GenerateOutstandingReportAsync();
            var revenueReport = await _reportService.GenerateRevenueReportAsync(DateTime.Now.AddMonths(-1), DateTime.Now);

            var activeInvoices = invoices.Where(i => i.Status != "Archived").ToList();
            var activePayments = payments.Where(p => !p.IsArchived && p.PaymentStatus != "Archived").ToList();

            var dashboard = new DashboardViewModel
            {
                TotalInvoices = activeInvoices.Count,
                PaidInvoices = activeInvoices.Count(i => i.Status == "Paid"),
                PendingInvoices = activeInvoices.Count(i => i.Status == "Sent" || i.Status == "Draft"),
                OverdueInvoices = activeInvoices.Count(i => i.Status == "Overdue"),
                TotalRevenue = activeInvoices.Sum(i => i.TotalAmount),
                TotalPayments = activePayments.Where(p => p.PaymentStatus == "Completed").Sum(p => p.Amount),
                TotalOutstanding = outstandingReport.TotalOutstanding,
                AccountBalance = await _transactionService.GetAccountBalanceAsync(),
                RecentInvoices = activeInvoices.OrderByDescending(i => i.InvoiceDate).Take(5).ToList(),
                RecentPayments = activePayments.OrderByDescending(p => p.PaymentDate).Take(5).ToList(),
                RecentTransactions = transactions.Take(5).ToList(),
                MonthlyRevenue = revenueReport.TotalRevenue,
                MonthlyPayments = revenueReport.TotalPayments
            };


            return View(dashboard);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> TestCloudinaryConnection()
        {
            try
            {
                bool isConnected = await _cloudinaryService.TestConnectionAsync();
                if (isConnected)
                {
                    return Ok(new { success = true, message = "✅ Cloudinary is connected successfully!" });
                }
                else
                {
                    return BadRequest(new { success = false, message = "❌ Cloudinary connection failed." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class DashboardViewModel
    {
        public int TotalInvoices { get; set; }
        public int PaidInvoices { get; set; }
        public int PendingInvoices { get; set; }
        public int OverdueInvoices { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalPayments { get; set; }
        public decimal TotalOutstanding { get; set; }
        public decimal AccountBalance { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal MonthlyPayments { get; set; }
        public List<Invoice> RecentInvoices { get; set; } = new();
        public List<Payment> RecentPayments { get; set; } = new();
        public List<Transaction> RecentTransactions { get; set; } = new();
    }
}
