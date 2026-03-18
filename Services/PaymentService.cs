using SarEquipEnterprise.Models;
using SarEquipEnterprise.Data;
using Microsoft.EntityFrameworkCore;

namespace SarEquipEnterprise.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly BillingSystemDbContext _context;
        private readonly IInvoiceService _invoiceService;

        public PaymentService(BillingSystemDbContext context, IInvoiceService invoiceService)
        {
            _context = context;
            _invoiceService = invoiceService;
        }

        public async Task<List<Payment>> GetAllPaymentsAsync()
        {
            return await _context.Payments.ToListAsync();
        }

        public async Task<Payment?> GetPaymentByIdAsync(int id)
        {
            return await _context.Payments.FirstOrDefaultAsync(p => p.PaymentId == id);
        }

        public async Task<List<Payment>> GetPaymentsByInvoiceIdAsync(int invoiceId)
        {
            return await _context.Payments.Where(p => p.InvoiceId == invoiceId).ToListAsync();
        }

        public async Task<Payment> ProcessPaymentAsync(Payment payment)
        {
            payment.PaymentDate = DateTime.Now;
            payment.PaymentStatus = "Completed";

            if (string.IsNullOrEmpty(payment.ReferenceNumber))
            {
                var random = new Random();
                payment.ReferenceNumber = random.Next(100000000, 1000000000).ToString();
            }

            if (string.IsNullOrEmpty(payment.PaymentNumber))
            {
                payment.PaymentNumber = await GeneratePaymentNumberAsync();
            }

            // Update invoice status if fully paid
            var invoice = await _invoiceService.GetInvoiceByIdAsync(payment.InvoiceId);
            if (invoice != null)
            {
                var totalPaid = await _context.Payments
                    .Where(p => p.InvoiceId == payment.InvoiceId && p.PaymentStatus == "Completed")
                    .SumAsync(p => p.Amount) + payment.Amount;

                if (totalPaid >= invoice.TotalAmount)
                {
                    invoice.Status = "Paid";
                    await _invoiceService.UpdateInvoiceAsync(invoice);
                }
            }

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return payment;
        }

        public async Task<Payment> RefundPaymentAsync(int paymentId, decimal refundAmount)
        {
            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.PaymentId == paymentId);
            if (payment == null)
                throw new ArgumentException("Payment not found");

            if (payment.PaymentStatus != "Completed")
                throw new InvalidOperationException("Only completed payments can be refunded");

            payment.PaymentStatus = "Refunded";
            await _context.SaveChangesAsync();
            return payment;
        }

        public async Task<Payment> ArchivePaymentAsync(int paymentId)
        {
            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.PaymentId == paymentId);
            if (payment == null)
                throw new ArgumentException("Payment not found");

            payment.IsArchived = true;
            payment.PaymentStatus = "Archived";
            
            // Cascade archive status to related invoice
            if (payment.InvoiceId > 0)
            {
                var invoice = await _invoiceService.GetInvoiceByIdAsync(payment.InvoiceId);
                if (invoice != null && invoice.Status != "Archived")
                {
                    invoice.Status = "Archived";
                    await _invoiceService.UpdateInvoiceAsync(invoice);
                }
            }

            await _context.SaveChangesAsync();
            return payment;
        }

        public async Task<string> GeneratePaymentNumberAsync()
        {
            var prefix = "PAY";
            var year = DateTime.Now.Year;
            var month = DateTime.Now.Month.ToString("D2");
            var count = await _context.Payments
                .Where(p => p.PaymentDate.Year == year && p.PaymentDate.Month == DateTime.Now.Month)
                .CountAsync() + 1;
            return $"{prefix}-{year}{month}-{count:D4}";
        }
    }
}
