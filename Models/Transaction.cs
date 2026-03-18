namespace SarEquipEnterprise.Models
{
    public class Transaction
    {
        public int TransactionId { get; set; }
        public string TransactionNumber { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public string TransactionType { get; set; } = string.Empty; // Credit, Debit
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // Revenue, Expense, Refund, Adjustment, etc.
        public int? InvoiceId { get; set; }
        public int? PaymentId { get; set; }
        public string Reference { get; set; } = string.Empty;
        public decimal BalanceAfter { get; set; }
        public string Status { get; set; } = "Completed"; // Completed, Pending, Reversed
    }
}
