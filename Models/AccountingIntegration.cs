namespace SarEquipEnterprise.Models
{
    public class AccountingIntegration
    {
        public int IntegrationId { get; set; }
        public string IntegrationType { get; set; } = string.Empty; // QuickBooks, Xero, Sage, Custom API
        public string ApiEndpoint { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime LastSyncDate { get; set; }
        public string SyncStatus { get; set; } = "Pending"; // Pending, Success, Failed
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class SyncRequest
    {
        public string EntityType { get; set; } = string.Empty; // Invoice, Payment, Transaction
        public int EntityId { get; set; }
        public DateTime SyncDate { get; set; }
        public string Status { get; set; } = "Pending";
    }
}
