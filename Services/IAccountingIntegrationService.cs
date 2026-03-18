using SarEquipEnterprise.Models;

namespace SarEquipEnterprise.Services
{
    public interface IAccountingIntegrationService
    {
        Task<List<AccountingIntegration>> GetAllIntegrationsAsync();
        Task<AccountingIntegration?> GetIntegrationByIdAsync(int id);
        Task<AccountingIntegration> CreateIntegrationAsync(AccountingIntegration integration);
        Task<AccountingIntegration> UpdateIntegrationAsync(AccountingIntegration integration);
        Task<bool> SyncInvoiceAsync(int invoiceId, int integrationId);
        Task<bool> SyncPaymentAsync(int paymentId, int integrationId);
        Task<bool> TestConnectionAsync(int integrationId);
    }
}
