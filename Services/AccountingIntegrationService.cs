using SarEquipEnterprise.Models;
using SarEquipEnterprise.Data;
using Microsoft.EntityFrameworkCore;

namespace SarEquipEnterprise.Services
{
    public class AccountingIntegrationService : IAccountingIntegrationService
    {
        private readonly BillingSystemDbContext _context;

        public AccountingIntegrationService(BillingSystemDbContext context)
        {
            _context = context;
        }

        public async Task<List<AccountingIntegration>> GetAllIntegrationsAsync()
        {
            return await _context.AccountingIntegrations.ToListAsync();
        }

        public async Task<AccountingIntegration?> GetIntegrationByIdAsync(int id)
        {
            return await _context.AccountingIntegrations.FirstOrDefaultAsync(i => i.IntegrationId == id);
        }

        public async Task<AccountingIntegration> CreateIntegrationAsync(AccountingIntegration integration)
        {
            integration.LastSyncDate = DateTime.MinValue;
            integration.SyncStatus = "Pending";
            _context.AccountingIntegrations.Add(integration);
            await _context.SaveChangesAsync();
            return integration;
        }

        public async Task<AccountingIntegration> UpdateIntegrationAsync(AccountingIntegration integration)
        {
            var existing = await _context.AccountingIntegrations.FirstOrDefaultAsync(i => i.IntegrationId == integration.IntegrationId);
            if (existing == null)
                throw new ArgumentException("Integration not found");

            _context.Entry(existing).CurrentValues.SetValues(integration);
            await _context.SaveChangesAsync();
            return integration;
        }

        public async Task<bool> SyncInvoiceAsync(int invoiceId, int integrationId)
        {
            var integration = await GetIntegrationByIdAsync(integrationId);
            if (integration == null || !integration.IsActive)
                return false;

            try
            {
                // Simulate API call to accounting system
                await Task.Delay(100); // Simulate network delay

                integration.LastSyncDate = DateTime.Now;
                integration.SyncStatus = "Success";
                integration.ErrorMessage = string.Empty;
                await UpdateIntegrationAsync(integration);

                return true;
            }
            catch (Exception ex)
            {
                integration.SyncStatus = "Failed";
                integration.ErrorMessage = ex.Message;
                await UpdateIntegrationAsync(integration);
                return false;
            }
        }

        public async Task<bool> SyncPaymentAsync(int paymentId, int integrationId)
        {
            var integration = await GetIntegrationByIdAsync(integrationId);
            if (integration == null || !integration.IsActive)
                return false;

            try
            {
                // Simulate API call to accounting system
                await Task.Delay(100); // Simulate network delay

                integration.LastSyncDate = DateTime.Now;
                integration.SyncStatus = "Success";
                integration.ErrorMessage = string.Empty;
                await UpdateIntegrationAsync(integration);

                return true;
            }
            catch (Exception ex)
            {
                integration.SyncStatus = "Failed";
                integration.ErrorMessage = ex.Message;
                await UpdateIntegrationAsync(integration);
                return false;
            }
        }

        public async Task<bool> TestConnectionAsync(int integrationId)
        {
            var integration = await GetIntegrationByIdAsync(integrationId);
            if (integration == null)
                return false;

            try
            {
                // Simulate connection test
                await Task.Delay(200);
                return !string.IsNullOrEmpty(integration.ApiEndpoint) && !string.IsNullOrEmpty(integration.ApiKey);
            }
            catch
            {
                return false;
            }
        }
    }
}
