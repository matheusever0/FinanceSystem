using FinanceSystem.Web.Extensions;
using FinanceSystem.Web.Filters;
using FinanceSystem.Web.Models;
using FinanceSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceSystem.Web.Controllers
{
    [Authorize]
    [RequirePermission("paymenttypes.view")]
    public class PaymentTypesController : Controller
    {
        private readonly IPaymentTypeService _paymentTypeService;
        private readonly ILogger<PaymentTypesController> _logger;

        public PaymentTypesController(
            IPaymentTypeService paymentTypeService,
            ILogger<PaymentTypesController> logger)
        {
            _paymentTypeService = paymentTypeService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var paymentTypes = await _paymentTypeService.GetAllPaymentTypesAsync(token);
                return View(paymentTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar tipos de pagamento");
                TempData["ErrorMessage"] = $"Erro ao carregar tipos de pagamento: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> System()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var paymentTypes = await _paymentTypeService.GetSystemPaymentTypesAsync(token);
                ViewBag.IsSystemView = true;
                ViewBag.Title = "Tipos de Pagamento do Sistema";
                return View("Index", paymentTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar tipos de pagamento do sistema");
                TempData["ErrorMessage"] = $"Erro ao carregar tipos de pagamento do sistema: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> User()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var paymentTypes = await _paymentTypeService.GetUserPaymentTypesAsync(token);
                ViewBag.IsUserView = true;
                ViewBag.Title = "Meus Tipos de Pagamento";
                return View("Index", paymentTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar tipos de pagamento do usuário");
                TempData["ErrorMessage"] = $"Erro ao carregar tipos de pagamento do usuário: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Details(string id)
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var paymentType = await _paymentTypeService.GetPaymentTypeByIdAsync(id, token);
                return View(paymentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar detalhes do tipo de pagamento");
                TempData["ErrorMessage"] = $"Erro ao carregar detalhes do tipo de pagamento: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [RequirePermission("paymenttypes.create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("paymenttypes.create")]
        public async Task<IActionResult> Create(CreatePaymentTypeModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var token = HttpContext.GetJwtToken();
                    var paymentType = await _paymentTypeService.CreatePaymentTypeAsync(model, token);
                    TempData["SuccessMessage"] = "Tipo de pagamento criado com sucesso!";
                    return RedirectToAction(nameof(Details), new { id = paymentType.Id });
                }
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar tipo de pagamento");
                ModelState.AddModelError(string.Empty, $"Erro ao criar tipo de pagamento: {ex.Message}");
                return View(model);
            }
        }

        [RequirePermission("paymenttypes.edit")]
        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var paymentType = await _paymentTypeService.GetPaymentTypeByIdAsync(id, token);

                if (paymentType.IsSystem)
                {
                    TempData["ErrorMessage"] = "Não é possível editar tipos de pagamento do sistema";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var model = new UpdatePaymentTypeModel
                {
                    Name = paymentType.Name,
                    Description = paymentType.Description
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar tipo de pagamento para edição");
                TempData["ErrorMessage"] = $"Erro ao carregar tipo de pagamento para edição: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("paymenttypes.edit")]
        public async Task<IActionResult> Edit(string id, UpdatePaymentTypeModel model)
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var paymentType = await _paymentTypeService.GetPaymentTypeByIdAsync(id, token);

                if (paymentType.IsSystem)
                {
                    TempData["ErrorMessage"] = "Não é possível editar tipos de pagamento do sistema";
                    return RedirectToAction(nameof(Details), new { id });
                }

                if (ModelState.IsValid)
                {
                    await _paymentTypeService.UpdatePaymentTypeAsync(id, model, token);
                    TempData["SuccessMessage"] = "Tipo de pagamento atualizado com sucesso!";
                    return RedirectToAction(nameof(Details), new { id });
                }
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar tipo de pagamento");
                ModelState.AddModelError(string.Empty, $"Erro ao atualizar tipo de pagamento: {ex.Message}");
                return View(model);
            }
        }

        [RequirePermission("paymenttypes.delete")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var paymentType = await _paymentTypeService.GetPaymentTypeByIdAsync(id, token);

                if (paymentType.IsSystem)
                {
                    TempData["ErrorMessage"] = "Não é possível excluir tipos de pagamento do sistema";
                    return RedirectToAction(nameof(Details), new { id });
                }

                return View(paymentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar tipo de pagamento para exclusão");
                TempData["ErrorMessage"] = $"Erro ao carregar tipo de pagamento para exclusão: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [RequirePermission("paymenttypes.delete")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var paymentType = await _paymentTypeService.GetPaymentTypeByIdAsync(id, token);

                if (paymentType.IsSystem)
                {
                    TempData["ErrorMessage"] = "Não é possível excluir tipos de pagamento do sistema";
                    return RedirectToAction(nameof(Details), new { id });
                }

                await _paymentTypeService.DeletePaymentTypeAsync(id, token);
                TempData["SuccessMessage"] = "Tipo de pagamento excluído com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir tipo de pagamento");
                TempData["ErrorMessage"] = $"Erro ao excluir tipo de pagamento: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}