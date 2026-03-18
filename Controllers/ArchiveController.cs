using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SarEquipEnterprise.Services;

namespace SarEquipEnterprise.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class ArchiveController : Controller
    {
        private readonly IPaymentService _paymentService;

        public ArchiveController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        public async Task<IActionResult> Index()
        {
            var all = await _paymentService.GetAllPaymentsAsync();
            var archived = all.Where(p => p.IsArchived).ToList();
            return View(archived);
        }
    }
}
