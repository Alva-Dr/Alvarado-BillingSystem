using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SarEquipEnterprise.Models;
using SarEquipEnterprise.Services;

namespace SarEquipEnterprise.Controllers
{
    [Authorize]
    public class IntegrationController : Controller
    {
        private readonly IAccountingIntegrationService _integrationService;

        public IntegrationController(IAccountingIntegrationService integrationService)
        {
            _integrationService = integrationService;
        }

        public async Task<IActionResult> Index()
        {
            var integrations = await _integrationService.GetAllIntegrationsAsync();
            return View(integrations);
        }

        public async Task<IActionResult> Details(int id)
        {
            var integration = await _integrationService.GetIntegrationByIdAsync(id);
            if (integration == null)
                return NotFound();

            return View(integration);
        }

        public IActionResult Create()
        {
            return View(new AccountingIntegration());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AccountingIntegration integration)
        {
            if (ModelState.IsValid)
            {
                await _integrationService.CreateIntegrationAsync(integration);
                return RedirectToAction(nameof(Index));
            }
            return View(integration);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var integration = await _integrationService.GetIntegrationByIdAsync(id);
            if (integration == null)
                return NotFound();

            return View(integration);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AccountingIntegration integration)
        {
            if (id != integration.IntegrationId)
                return NotFound();

            if (ModelState.IsValid)
            {
                await _integrationService.UpdateIntegrationAsync(integration);
                return RedirectToAction(nameof(Index));
            }
            return View(integration);
        }

        [HttpPost]
        public async Task<IActionResult> TestConnection(int id)
        {
            var result = await _integrationService.TestConnectionAsync(id);
            return Json(new { success = result });
        }

        [HttpPost]
        public async Task<IActionResult> SyncInvoice(int invoiceId, int integrationId)
        {
            var result = await _integrationService.SyncInvoiceAsync(invoiceId, integrationId);
            return Json(new { success = result });
        }

        [HttpPost]
        public async Task<IActionResult> SyncPayment(int paymentId, int integrationId)
        {
            var result = await _integrationService.SyncPaymentAsync(paymentId, integrationId);
            return Json(new { success = result });
        }
    }
}
