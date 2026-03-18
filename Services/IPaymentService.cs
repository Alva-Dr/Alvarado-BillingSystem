using SarEquipEnterprise.Models;

namespace SarEquipEnterprise.Services
{
    public interface IPaymentService
    {
        Task<List<Payment>> GetAllPaymentsAsync();
        Task<Payment?> GetPaymentByIdAsync(int id);
        Task<List<Payment>> GetPaymentsByInvoiceIdAsync(int invoiceId);
        Task<Payment> ProcessPaymentAsync(Payment payment);
        Task<Payment> RefundPaymentAsync(int paymentId, decimal refundAmount);
        Task<Payment> ArchivePaymentAsync(int paymentId);
        Task<string> GeneratePaymentNumberAsync();
    }
}
