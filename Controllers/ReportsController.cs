using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SarEquipEnterprise.Models;
using SarEquipEnterprise.Services;

namespace SarEquipEnterprise.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly IBillingReportService _reportService;

        public ReportsController(IBillingReportService reportService)
        {
            _reportService = reportService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Revenue(DateTime? startDate, DateTime? endDate)
        {
            var start = startDate ?? DateTime.Now.AddMonths(-1);
            var end = endDate ?? DateTime.Now;

            var report = await _reportService.GenerateRevenueReportAsync(start, end);
            return View(report);
        }

        public async Task<IActionResult> InvoiceStatus(DateTime? startDate, DateTime? endDate)
        {
            var start = startDate ?? DateTime.Now.AddMonths(-1);
            var end = endDate ?? DateTime.Now;

            var report = await _reportService.GenerateInvoiceStatusReportAsync(start, end);
            return View(report);
        }

        public async Task<IActionResult> Payment(DateTime? startDate, DateTime? endDate)
        {
            var start = startDate ?? DateTime.Now.AddMonths(-1);
            var end = endDate ?? DateTime.Now;

            var report = await _reportService.GeneratePaymentReportAsync(start, end);
            return View(report);
        }

        public async Task<IActionResult> Outstanding()
        {
            var report = await _reportService.GenerateOutstandingReportAsync();
            return View(report);
        }
    }
}
