using FinanceSystem.Web.Extensions;
using FinanceSystem.Web.Filters;
using FinanceSystem.Web.Models.PaymentMethod;
using FinanceSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FinanceSystem.Web.Controllers
{
    [Authorize]
    [RequirePermission("paymentmethods.view")]
    public class PaymentMethodsController : Controller
    {
        private readonly IPaymentMethodService _paymentMethodService;
        private readonly ILogger<PaymentMethodsController> _logger;

        public PaymentMethodsController(
            IPaymentMethodService paymentMethodService,
            ILogger<PaymentMethodsController> logger)
        {
            _paymentMethodService = paymentMethodService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var paymentMethods = await _paymentMethodService.GetAllPaymentMethodsAsync(token);
                return View(paymentMethods);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar métodos de pagamento");
                TempData["ErrorMessage"] = $"Erro ao carregar métodos de pagamento: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> System()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var paymentMethods = await _paymentMethodService.GetSystemPaymentMethodsAsync(token);
                ViewBag.IsSystemView = true;
                ViewBag.Title = "Métodos de Pagamento do Sistema";
                return View("Index", paymentMethods);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar métodos de pagamento do sistema");
                TempData["ErrorMessage"] = $"Erro ao carregar métodos de pagamento do sistema: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> User()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var paymentMethods = await _paymentMethodService.GetUserPaymentMethodsAsync(token);
                ViewBag.IsUserView = true;
                ViewBag.Title = "Meus Métodos de Pagamento";
                return View("Index", paymentMethods);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar métodos de pagamento do usuário");
                TempData["ErrorMessage"] = $"Erro ao carregar métodos de pagamento do usuário: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> ByType(int type)
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var paymentMethods = await _paymentMethodService.GetByTypeAsync(type, token);

                string typeDescription = GetPaymentMethodTypeDescription(type);
                ViewBag.Title = $"Métodos de Pagamento do Tipo: {typeDescription}";
                ViewBag.Type = type;

                return View("Index", paymentMethods);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar métodos de pagamento por tipo");
                TempData["ErrorMessage"] = $"Erro ao carregar métodos de pagamento por tipo: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Details(string id)
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var paymentMethod = await _paymentMethodService.GetPaymentMethodByIdAsync(id, token);
                return View(paymentMethod);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar detalhes do método de pagamento");
                TempData["ErrorMessage"] = $"Erro ao carregar detalhes do método de pagamento: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [RequirePermission("paymentmethods.create")]
        public IActionResult Create()
        {
            ViewBag.PaymentMethodTypes = GetPaymentMethodTypes();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("paymentmethods.create")]
        public async Task<IActionResult> Create(CreatePaymentMethodModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var token = HttpContext.GetJwtToken();
                    var paymentMethod = await _paymentMethodService.CreatePaymentMethodAsync(model, token);
                    TempData["SuccessMessage"] = "Método de pagamento criado com sucesso!";
                    return RedirectToAction(nameof(Details), new { id = paymentMethod.Id });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar método de pagamento");
                ModelState.AddModelError(string.Empty, $"Erro ao criar método de pagamento: {ex.Message}");
            }

            ViewBag.PaymentMethodTypes = GetPaymentMethodTypes();
            return View(model);
        }

        [RequirePermission("paymentmethods.edit")]
        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var paymentMethod = await _paymentMethodService.GetPaymentMethodByIdAsync(id, token);

                if (paymentMethod.IsSystem)
                {
                    TempData["ErrorMessage"] = "Não é possível editar métodos de pagamento do sistema";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var model = new UpdatePaymentMethodModel
                {
                    Name = paymentMethod.Name,
                    Description = paymentMethod.Description
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar método de pagamento para edição");
                TempData["ErrorMessage"] = $"Erro ao carregar método de pagamento para edição: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("paymentmethods.edit")]
        public async Task<IActionResult> Edit(string id, UpdatePaymentMethodModel model)
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var paymentMethod = await _paymentMethodService.GetPaymentMethodByIdAsync(id, token);

                if (paymentMethod.IsSystem)
                {
                    TempData["ErrorMessage"] = "Não é possível editar métodos de pagamento do sistema";
                    return RedirectToAction(nameof(Details), new { id });
                }

                if (ModelState.IsValid)
                {
                    await _paymentMethodService.UpdatePaymentMethodAsync(id, model, token);
                    TempData["SuccessMessage"] = "Método de pagamento atualizado com sucesso!";
                    return RedirectToAction(nameof(Details), new { id });
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar método de pagamento");
                ModelState.AddModelError(string.Empty, $"Erro ao atualizar método de pagamento: {ex.Message}");
                return View(model);
            }
        }

        [RequirePermission("paymentmethods.delete")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var paymentMethod = await _paymentMethodService.GetPaymentMethodByIdAsync(id, token);

                if (paymentMethod.IsSystem)
                {
                    TempData["ErrorMessage"] = "Não é possível excluir métodos de pagamento do sistema";
                    return RedirectToAction(nameof(Details), new { id });
                }

                return View(paymentMethod);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar método de pagamento para exclusão");
                TempData["ErrorMessage"] = $"Erro ao carregar método de pagamento para exclusão: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [RequirePermission("paymentmethods.delete")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var paymentMethod = await _paymentMethodService.GetPaymentMethodByIdAsync(id, token);

                if (paymentMethod.IsSystem)
                {
                    TempData["ErrorMessage"] = "Não é possível excluir métodos de pagamento do sistema";
                    return RedirectToAction(nameof(Details), new { id });
                }

                await _paymentMethodService.DeletePaymentMethodAsync(id, token);
                TempData["SuccessMessage"] = "Método de pagamento excluído com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir método de pagamento");
                TempData["ErrorMessage"] = $"Erro ao excluir método de pagamento: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        private List<SelectListItem> GetPaymentMethodTypes()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Dinheiro" },
                new SelectListItem { Value = "2", Text = "Cartão de Crédito" },
                new SelectListItem { Value = "3", Text = "Cartão de Débito" },
                new SelectListItem { Value = "4", Text = "Transferência Bancária" },
                new SelectListItem { Value = "5", Text = "Carteira Digital" },
                new SelectListItem { Value = "6", Text = "Cheque" },
                new SelectListItem { Value = "7", Text = "Outro" }
            };
        }

        private static string GetPaymentMethodTypeDescription(int type)
        {
            return type switch
            {
                1 => "Dinheiro",
                2 => "Cartão de Crédito",
                3 => "Cartão de Débito",
                4 => "Transferência Bancária",
                5 => "Carteira Digital",
                6 => "Cheque",
                7 => "Outro",
                _ => "Desconhecido"
            };
        }
    }
}