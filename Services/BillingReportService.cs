using SarEquipEnterprise.Models;

namespace SarEquipEnterprise.Services
{
    public class BillingReportService : IBillingReportService
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IPaymentService _paymentService;
        private readonly ITransactionService _transactionService;

        public BillingReportService(
            IInvoiceService invoiceService,
            IPaymentService paymentService,
            ITransactionService transactionService)
        {
            _invoiceService = invoiceService;
            _paymentService = paymentService;
            _transactionService = transactionService;
        }

        public async Task<BillingReport> GenerateRevenueReportAsync(DateTime startDate, DateTime endDate)
        {
            var invoices = await _invoiceService.GetAllInvoicesAsync();
            var payments = await _paymentService.GetAllPaymentsAsync();
            var transactions = await _transactionService.GetTransactionsByDateRangeAsync(startDate, endDate);

            var report = new BillingReport
            {
                ReportId = 1,
                ReportName = "Revenue Report",
                ReportDate = DateTime.Now,
                StartDate = startDate,
                EndDate = endDate,
                TotalRevenue = invoices.Where(i => i.InvoiceDate >= startDate && i.InvoiceDate <= endDate)
                    .Sum(i => i.TotalAmount),
                TotalPayments = payments.Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate && p.PaymentStatus == "Completed")
                    .Sum(p => p.Amount),
                TotalInvoices = invoices.Count(i => i.InvoiceDate >= startDate && i.InvoiceDate <= endDate),
                PaidInvoices = invoices.Count(i => i.InvoiceDate >= startDate && i.InvoiceDate <= endDate && i.Status == "Paid"),
                PendingInvoices = invoices.Count(i => i.InvoiceDate >= startDate && i.InvoiceDate <= endDate && i.Status == "Sent"),
                OverdueInvoices = invoices.Count(i => i.InvoiceDate >= startDate && i.InvoiceDate <= endDate && i.Status == "Overdue")
            };

            report.Details.Add(new ReportDetail { Category = "Total Revenue", Amount = report.TotalRevenue, Count = report.TotalInvoices });
            report.Details.Add(new ReportDetail { Category = "Total Payments", Amount = report.TotalPayments, Count = payments.Count });

            return report;
        }

        public async Task<BillingReport> GenerateInvoiceStatusReportAsync(DateTime startDate, DateTime endDate)
        {
            var invoices = await _invoiceService.GetAllInvoicesAsync();
            var filteredInvoices = invoices.Where(i => i.InvoiceDate >= startDate && i.InvoiceDate <= endDate).ToList();

            var report = new BillingReport
            {
                ReportId = 2,
                ReportName = "Invoice Status Report",
                ReportDate = DateTime.Now,
                StartDate = startDate,
                EndDate = endDate,
                TotalInvoices = filteredInvoices.Count,
                PaidInvoices = filteredInvoices.Count(i => i.Status == "Paid"),
                PendingInvoices = filteredInvoices.Count(i => i.Status == "Sent" || i.Status == "Draft"),
                OverdueInvoices = filteredInvoices.Count(i => i.Status == "Overdue"),
                TotalRevenue = filteredInvoices.Sum(i => i.TotalAmount)
            };

            report.Details.Add(new ReportDetail { Category = "Paid", Amount = filteredInvoices.Where(i => i.Status == "Paid").Sum(i => i.TotalAmount), Count = report.PaidInvoices });
            report.Details.Add(new ReportDetail { Category = "Pending", Amount = filteredInvoices.Where(i => i.Status == "Sent" || i.Status == "Draft").Sum(i => i.TotalAmount), Count = report.PendingInvoices });
            report.Details.Add(new ReportDetail { Category = "Overdue", Amount = filteredInvoices.Where(i => i.Status == "Overdue").Sum(i => i.TotalAmount), Count = report.OverdueInvoices });

            return report;
        }

        public async Task<BillingReport> GeneratePaymentReportAsync(DateTime startDate, DateTime endDate)
        {
            var payments = await _paymentService.GetAllPaymentsAsync();
            var filteredPayments = payments.Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate).ToList();

            var report = new BillingReport
            {
                ReportId = 3,
                ReportName = "Payment Report",
                ReportDate = DateTime.Now,
                StartDate = startDate,
                EndDate = endDate,
                TotalPayments = filteredPayments.Where(p => p.PaymentStatus == "Completed").Sum(p => p.Amount),
                TotalInvoices = filteredPayments.Count
            };

            var paymentMethods = filteredPayments.GroupBy(p => p.PaymentMethod);
            foreach (var method in paymentMethods)
            {
                report.Details.Add(new ReportDetail
                {
                    Category = method.Key,
                    Amount = method.Where(p => p.PaymentStatus == "Completed").Sum(p => p.Amount),
                    Count = method.Count()
                });
            }

            return report;
        }

        public async Task<BillingReport> GenerateOutstandingReportAsync()
        {
            var invoices = await _invoiceService.GetAllInvoicesAsync();
            var outstandingInvoices = invoices.Where(i => i.Status != "Paid" && i.Status != "Cancelled").ToList();

            var report = new BillingReport
            {
                ReportId = 4,
                ReportName = "Outstanding Invoices Report",
                ReportDate = DateTime.Now,
                StartDate = DateTime.MinValue,
                EndDate = DateTime.Now,
                TotalOutstanding = outstandingInvoices.Sum(i => i.TotalAmount),
                TotalInvoices = outstandingInvoices.Count,
                OverdueInvoices = outstandingInvoices.Count(i => i.Status == "Overdue" || i.DueDate < DateTime.Now)
            };

            return report;
        }
    }
}
