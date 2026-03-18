using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SarEquipEnterprise.Models;
using SarEquipEnterprise.Services;

namespace SarEquipEnterprise.Controllers
{
    [Authorize]
    public class BillingController : Controller
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IPaymentService _paymentService;
        private readonly ITransactionService _transactionService;
        private readonly IBillingReportService _reportService;

        public BillingController(
            IInvoiceService invoiceService,
            IPaymentService paymentService,
            ITransactionService transactionService,
            IBillingReportService reportService)
        {
            _invoiceService = invoiceService;
            _paymentService = paymentService;
            _transactionService = transactionService;
            _reportService = reportService;
        }

        public async Task<IActionResult> Index()
        {
            var invoices = await _invoiceService.GetAllInvoicesAsync();
            var payments = await _paymentService.GetAllPaymentsAsync();
            var transactions = await _transactionService.GetAllTransactionsAsync();
            var outstandingReport = await _reportService.GenerateOutstandingReportAsync();

            var activeInvoices = invoices.Where(i => i.Status != "Archived").ToList();
            var activePayments = payments.Where(p => !p.IsArchived && p.PaymentStatus != "Archived").ToList();

            var dashboard = new BillingDashboardViewModel
            {
                TotalInvoices = activeInvoices.Count,
                PaidInvoices = activeInvoices.Count(i => i.Status == "Paid"),
                PendingInvoices = activeInvoices.Count(i => i.Status == "Sent" || i.Status == "Draft"),
                OverdueInvoices = activeInvoices.Count(i => i.Status == "Overdue"),
                TotalRevenue = activeInvoices.Sum(i => i.TotalAmount),
                TotalPayments = activePayments.Where(p => p.PaymentStatus == "Completed").Sum(p => p.Amount),
                TotalOutstanding = outstandingReport.TotalOutstanding,
                RecentInvoices = activeInvoices.OrderByDescending(i => i.InvoiceDate).Take(5).ToList(),
                RecentPayments = activePayments.OrderByDescending(p => p.PaymentDate).Take(5).ToList(),
                AccountBalance = await _transactionService.GetAccountBalanceAsync()
            };


            return View(dashboard);
        }
    }

    public class BillingDashboardViewModel
    {
        public int TotalInvoices { get; set; }
        public int PaidInvoices { get; set; }
        public int PendingInvoices { get; set; }
        public int OverdueInvoices { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalPayments { get; set; }
        public decimal TotalOutstanding { get; set; }
        public decimal AccountBalance { get; set; }
        public List<Invoice> RecentInvoices { get; set; } = new();
        public List<Payment> RecentPayments { get; set; } = new();
    }
}
