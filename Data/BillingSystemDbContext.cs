using Microsoft.EntityFrameworkCore;
using SarEquipEnterprise.Models;

namespace SarEquipEnterprise.Data
{
    public class BillingSystemDbContext : DbContext
    {
        public BillingSystemDbContext(DbContextOptions<BillingSystemDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<BillingReport> BillingReports { get; set; }
        public DbSet<ReportDetail> ReportDetails { get; set; }
        public DbSet<AccountingIntegration> AccountingIntegrations { get; set; }
        public DbSet<Item> Items { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>()
                .HasKey(u => u.UserId);
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .IsRequired();
            modelBuilder.Entity<User>()
                .Property(u => u.PasswordHash)
                .IsRequired();

            // Configure Invoice entity
            modelBuilder.Entity<Invoice>()
                .HasKey(i => i.InvoiceId);
            modelBuilder.Entity<Invoice>()
                .Property(i => i.InvoiceNumber)
                .IsRequired();

            // Configure InvoiceItem entity
            modelBuilder.Entity<InvoiceItem>()
                .HasKey(ii => ii.ItemId);

            // Configure Payment entity
            modelBuilder.Entity<Payment>()
                .HasKey(p => p.PaymentId);

            // Configure Transaction entity
            modelBuilder.Entity<Transaction>()
                .HasKey(t => t.TransactionId);

            // Configure BillingReport entity
            modelBuilder.Entity<BillingReport>()
                .HasKey(b => b.ReportId);

            // Configure ReportDetail entity
            modelBuilder.Entity<ReportDetail>()
                .HasKey(rd => rd.DetailId);

            // Configure AccountingIntegration entity
            modelBuilder.Entity<AccountingIntegration>()
                .HasKey(a => a.IntegrationId);
        }
    }
}
