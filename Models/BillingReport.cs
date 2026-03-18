namespace SarEquipEnterprise.Models
{
    public class BillingReport
    {
        public int ReportId { get; set; }
        public string ReportName { get; set; } = string.Empty;
        public DateTime ReportDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalPayments { get; set; }
        public decimal TotalOutstanding { get; set; }
        public int TotalInvoices { get; set; }
        public int PaidInvoices { get; set; }
        public int PendingInvoices { get; set; }
        public int OverdueInvoices { get; set; }
        public List<ReportDetail> Details { get; set; } = new();
    }

    public class ReportDetail
    {
        public int DetailId { get; set; }
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int Count { get; set; }
    }
}
