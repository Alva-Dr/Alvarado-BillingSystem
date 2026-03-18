using SarEquipEnterprise.Models;
using SarEquipEnterprise.Data;
using Microsoft.EntityFrameworkCore;

namespace SarEquipEnterprise.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly BillingSystemDbContext _context;

        public InvoiceService(BillingSystemDbContext context)
        {
            _context = context;
        }

        public async Task<List<Invoice>> GetAllInvoicesAsync()
        {
            return await _context.Invoices.Include(i => i.Items).ToListAsync();
        }

        public async Task<Invoice?> GetInvoiceByIdAsync(int id)
        {
            return await _context.Invoices.Include(i => i.Items).FirstOrDefaultAsync(i => i.InvoiceId == id);
        }

        public async Task<Invoice> CreateInvoiceAsync(Invoice invoice)
        {
            invoice.InvoiceDate = DateTime.Now;
            invoice.DueDate = invoice.DueDate == default ? invoice.InvoiceDate.AddDays(30) : invoice.DueDate;
            
            // Calculate totals
            invoice.SubTotal = invoice.Items?.Sum(item => item.LineTotal) ?? 0;
            invoice.TotalAmount = invoice.SubTotal + invoice.TaxAmount - invoice.DiscountAmount;
            
            if (string.IsNullOrEmpty(invoice.InvoiceNumber))
            {
                invoice.InvoiceNumber = await GenerateInvoiceNumberAsync();
            }

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();
            return invoice;
        }

        public async Task<Invoice> UpdateInvoiceAsync(Invoice invoice)
        {
            var existing = await _context.Invoices
                .Include(i => i.Items)
                .FirstOrDefaultAsync(i => i.InvoiceId == invoice.InvoiceId);
                
            if (existing == null)
                throw new ArgumentException("Invoice not found");

            // Update scalar properties
            _context.Entry(existing).CurrentValues.SetValues(invoice);

            // Update items collection
            existing.Items.Clear();
            if (invoice.Items != null)
            {
                foreach (var item in invoice.Items)
                {
                    existing.Items.Add(item);
                }
            }

            // Recalculate totals
            existing.SubTotal = existing.Items?.Sum(item => item.LineTotal) ?? 0;
            existing.TotalAmount = existing.SubTotal + existing.TaxAmount - existing.DiscountAmount;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteInvoiceAsync(int id)
        {
            var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.InvoiceId == id);
            if (invoice == null)
                return false;

            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string> GenerateInvoiceNumberAsync()
        {
            var prefix = "INV";
            var year = DateTime.Now.Year;
            var month = DateTime.Now.Month.ToString("D2");
            var count = await _context.Invoices
                .Where(i => i.InvoiceDate.Year == year && i.InvoiceDate.Month == DateTime.Now.Month)
                .CountAsync() + 1;
            return $"{prefix}-{year}{month}-{count:D4}";
        }

        public async Task<Invoice> SendInvoiceAsync(int invoiceId)
        {
            var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);
            if (invoice == null)
                throw new ArgumentException("Invoice not found");

            invoice.Status = "Sent";
            await _context.SaveChangesAsync();
            return invoice;
        }

        public async Task<List<Invoice>> GetInvoicesByStatusAsync(string status)
        {
            return await _context.Invoices.Where(i => i.Status == status).ToListAsync();
        }

        public async Task<List<Item>> GetItemsAsync()
        {
            return await _context.Items.ToListAsync();
        }
    }
}
