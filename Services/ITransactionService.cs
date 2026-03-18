using SarEquipEnterprise.Models;

namespace SarEquipEnterprise.Services
{
    public interface ITransactionService
    {
        Task<List<Transaction>> GetAllTransactionsAsync();
        Task<Transaction?> GetTransactionByIdAsync(int id);
        Task<Transaction> CreateTransactionAsync(Transaction transaction);
        Task<List<Transaction>> GetTransactionsByTypeAsync(string type);
        Task<decimal> GetAccountBalanceAsync();
        Task<List<Transaction>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}
