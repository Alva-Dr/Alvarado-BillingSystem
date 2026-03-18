using SarEquipEnterprise.Models;

namespace SarEquipEnterprise.Services
{
    public interface IBillingReportService
    {
        Task<BillingReport> GenerateRevenueReportAsync(DateTime startDate, DateTime endDate);
        Task<BillingReport> GenerateInvoiceStatusReportAsync(DateTime startDate, DateTime endDate);
        Task<BillingReport> GeneratePaymentReportAsync(DateTime startDate, DateTime endDate);
        Task<BillingReport> GenerateOutstandingReportAsync();
    }
}
