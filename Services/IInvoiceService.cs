using SarEquipEnterprise.Models;

namespace SarEquipEnterprise.Services
{
    public interface IInvoiceService
    {
        Task<List<Invoice>> GetAllInvoicesAsync();
        Task<Invoice?> GetInvoiceByIdAsync(int id);
        Task<Invoice> CreateInvoiceAsync(Invoice invoice);
        Task<Invoice> UpdateInvoiceAsync(Invoice invoice);
        Task<bool> DeleteInvoiceAsync(int id);
        Task<string> GenerateInvoiceNumberAsync();
        Task<Invoice> SendInvoiceAsync(int invoiceId);
        Task<List<Invoice>> GetInvoicesByStatusAsync(string status);
        Task<List<Item>> GetItemsAsync();
    }
}
