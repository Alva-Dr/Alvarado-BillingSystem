using SarEquipEnterprise.Models;
using SarEquipEnterprise.Data;
using Microsoft.EntityFrameworkCore;

namespace SarEquipEnterprise.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly BillingSystemDbContext _context;

        public TransactionService(BillingSystemDbContext context)
        {
            _context = context;
        }

        public async Task<List<Transaction>> GetAllTransactionsAsync()
        {
            return await _context.Transactions.OrderByDescending(t => t.TransactionDate).ToListAsync();
        }

        public async Task<Transaction?> GetTransactionByIdAsync(int id)
        {
            return await _context.Transactions.FirstOrDefaultAsync(t => t.TransactionId == id);
        }

        public async Task<Transaction> CreateTransactionAsync(Transaction transaction)
        {
            transaction.TransactionDate = DateTime.Now;
            transaction.Status = "Completed";

            if (string.IsNullOrEmpty(transaction.TransactionNumber))
            {
                var prefix = transaction.TransactionType == "Credit" ? "CR" : "DB";
                var year = DateTime.Now.Year;
                var count = await _context.Transactions
                    .Where(t => t.TransactionDate.Year == year)
                    .CountAsync() + 1;
                transaction.TransactionNumber = $"{prefix}-{year}-{count:D4}";
            }

            if (string.IsNullOrEmpty(transaction.Reference))
            {
                var random = new Random();
                transaction.Reference = random.Next(100000000, 1000000000).ToString();
            }

            // Calculate balance after transaction
            var currentBalance = await GetAccountBalanceAsync();
            if (transaction.TransactionType == "Credit")
                transaction.BalanceAfter = currentBalance + transaction.Amount;
            else
                transaction.BalanceAfter = currentBalance - transaction.Amount;

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }

        public async Task<List<Transaction>> GetTransactionsByTypeAsync(string type)
        {
            return await _context.Transactions.Where(t => t.TransactionType == type).ToListAsync();
        }

        public async Task<decimal> GetAccountBalanceAsync()
        {
            var credits = await _context.Transactions
                .Where(t => t.TransactionType == "Credit" && t.Status == "Completed")
                .SumAsync(t => t.Amount);
            var debits = await _context.Transactions
                .Where(t => t.TransactionType == "Debit" && t.Status == "Completed")
                .SumAsync(t => t.Amount);
            return credits - debits;
        }

        public async Task<List<Transaction>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Transactions
                .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }
    }
}
