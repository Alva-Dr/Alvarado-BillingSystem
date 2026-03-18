namespace SarEquipEnterprise.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public string PaymentNumber { get; set; } = string.Empty;
        public int InvoiceId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; } = string.Empty; // Cash, Credit Card, Bank Transfer, Check, etc.
        public string PaymentStatus { get; set; } = "Pending"; // Pending, Completed, Failed, Refunded
        public string ReferenceNumber { get; set; } = string.Empty;
        public bool IsArchived { get; set; } = false;
        public string Notes { get; set; } = string.Empty;
        public string ProcessedBy { get; set; } = string.Empty;
        public Invoice? Invoice { get; set; }
    }
}
