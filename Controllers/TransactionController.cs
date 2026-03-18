using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SarEquipEnterprise.Models;
using SarEquipEnterprise.Services;

namespace SarEquipEnterprise.Controllers
{
    [Authorize]
    public class TransactionController : Controller
    {
        private readonly ITransactionService _transactionService;
        private readonly IInvoiceService _invoiceService;

        public TransactionController(ITransactionService transactionService, IInvoiceService invoiceService)
        {
            _transactionService = transactionService;
            _invoiceService = invoiceService;
        }

        public async Task<IActionResult> Index(string? type, DateTime? startDate, DateTime? endDate)
        {
            List<Transaction> transactions;

            if (startDate.HasValue && endDate.HasValue)
            {
                transactions = await _transactionService.GetTransactionsByDateRangeAsync(startDate.Value, endDate.Value);
            }
            else if (!string.IsNullOrEmpty(type))
            {
                transactions = await _transactionService.GetTransactionsByTypeAsync(type);
            }
            else
            {
                transactions = await _transactionService.GetAllTransactionsAsync();
            }

            ViewBag.CurrentBalance = await _transactionService.GetAccountBalanceAsync();
            ViewBag.SelectedType = type;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            return View(transactions);
        }

        public async Task<IActionResult> Details(int id)
        {
            var transaction = await _transactionService.GetTransactionByIdAsync(id);
            if (transaction == null)
                return NotFound();

            return View(transaction);
        }

        public async Task<IActionResult> Create()
        {
            var transaction = new Transaction
            {
                TransactionDate = DateTime.Now,
                Status = "Completed"
            };
            ViewBag.Invoices = await _invoiceService.GetAllInvoicesAsync();
            return View(transaction);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                await _transactionService.CreateTransactionAsync(transaction);
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Invoices = await _invoiceService.GetAllInvoicesAsync();
            return View(transaction);
        }
    }
}
