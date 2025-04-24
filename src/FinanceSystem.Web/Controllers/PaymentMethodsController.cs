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

        private const string ERROR_LOADING_PAYMENT_METHODS = "Erro ao carregar métodos de pagamento: {0}";
        private const string ERROR_LOADING_SYSTEM_PAYMENT_METHODS = "Erro ao carregar métodos de pagamento do sistema: {0}";
        private const string ERROR_LOADING_USER_PAYMENT_METHODS = "Erro ao carregar métodos de pagamento do usuário: {0}";
        private const string ERROR_LOADING_PAYMENT_METHODS_BY_TYPE = "Erro ao carregar métodos de pagamento por tipo: {0}";
        private const string ERROR_LOADING_PAYMENT_METHOD_DETAILS = "Erro ao carregar detalhes do método de pagamento: {0}";
        private const string ERROR_CREATING_PAYMENT_METHOD = "Erro ao criar método de pagamento: {0}";
        private const string ERROR_LOADING_PAYMENT_METHOD_EDIT = "Erro ao carregar método de pagamento para edição: {0}";
        private const string ERROR_UPDATING_PAYMENT_METHOD = "Erro ao atualizar método de pagamento: {0}";
        private const string ERROR_LOADING_PAYMENT_METHOD_DELETE = "Erro ao carregar método de pagamento para exclusão: {0}";
        private const string ERROR_DELETING_PAYMENT_METHOD = "Erro ao excluir método de pagamento: {0}";
        private const string ERROR_CANNOT_EDIT_SYSTEM_METHOD = "Não é possível editar métodos de pagamento do sistema";
        private const string ERROR_CANNOT_DELETE_SYSTEM_METHOD = "Não é possível excluir métodos de pagamento do sistema";

        private const string SUCCESS_CREATE_PAYMENT_METHOD = "Método de pagamento criado com sucesso!";
        private const string SUCCESS_UPDATE_PAYMENT_METHOD = "Método de pagamento atualizado com sucesso!";
        private const string SUCCESS_DELETE_PAYMENT_METHOD = "Método de pagamento excluído com sucesso!";

        public PaymentMethodsController(IPaymentMethodService paymentMethodService)
        {
            _paymentMethodService = paymentMethodService ?? throw new ArgumentNullException(nameof(paymentMethodService));
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
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_PAYMENT_METHODS, ex.Message);
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
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_SYSTEM_PAYMENT_METHODS, ex.Message);
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
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_USER_PAYMENT_METHODS, ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> ByType(int type)
        {
            if (type <= 0)
            {
                return BadRequest("Tipo de método de pagamento inválido");
            }

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
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_PAYMENT_METHODS_BY_TYPE, ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do método de pagamento não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var paymentMethod = await _paymentMethodService.GetPaymentMethodByIdAsync(id, token);

                return paymentMethod == null ? NotFound("Método de pagamento não encontrado") : View(paymentMethod);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_PAYMENT_METHOD_DETAILS, ex.Message);
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
            if (!ModelState.IsValid)
            {
                ViewBag.PaymentMethodTypes = GetPaymentMethodTypes();
                return View(model);
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var paymentMethod = await _paymentMethodService.CreatePaymentMethodAsync(model, token);
                TempData["SuccessMessage"] = SUCCESS_CREATE_PAYMENT_METHOD;
                return RedirectToAction(nameof(Details), new { id = paymentMethod.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, string.Format(ERROR_CREATING_PAYMENT_METHOD, ex.Message));
                ViewBag.PaymentMethodTypes = GetPaymentMethodTypes();
                return View(model);
            }
        }

        [RequirePermission("paymentmethods.edit")]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do método de pagamento não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var paymentMethod = await _paymentMethodService.GetPaymentMethodByIdAsync(id, token);

                if (paymentMethod == null)
                {
                    return NotFound("Método de pagamento não encontrado");
                }

                if (paymentMethod.IsSystem)
                {
                    TempData["ErrorMessage"] = ERROR_CANNOT_EDIT_SYSTEM_METHOD;
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
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_PAYMENT_METHOD_EDIT, ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("paymentmethods.edit")]
        public async Task<IActionResult> Edit(string id, UpdatePaymentMethodModel model)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do método de pagamento não fornecido");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var paymentMethod = await _paymentMethodService.GetPaymentMethodByIdAsync(id, token);

                if (paymentMethod == null)
                {
                    return NotFound("Método de pagamento não encontrado");
                }

                if (paymentMethod.IsSystem)
                {
                    TempData["ErrorMessage"] = ERROR_CANNOT_EDIT_SYSTEM_METHOD;
                    return RedirectToAction(nameof(Details), new { id });
                }

                await _paymentMethodService.UpdatePaymentMethodAsync(id, model, token);
                TempData["SuccessMessage"] = SUCCESS_UPDATE_PAYMENT_METHOD;
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, string.Format(ERROR_UPDATING_PAYMENT_METHOD, ex.Message));
                return View(model);
            }
        }

        [RequirePermission("paymentmethods.delete")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do método de pagamento não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var paymentMethod = await _paymentMethodService.GetPaymentMethodByIdAsync(id, token);

                if (paymentMethod == null)
                {
                    return NotFound("Método de pagamento não encontrado");
                }

                if (paymentMethod.IsSystem)
                {
                    TempData["ErrorMessage"] = ERROR_CANNOT_DELETE_SYSTEM_METHOD;
                    return RedirectToAction(nameof(Details), new { id });
                }

                return View(paymentMethod);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_PAYMENT_METHOD_DELETE, ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [RequirePermission("paymentmethods.delete")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do método de pagamento não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var paymentMethod = await _paymentMethodService.GetPaymentMethodByIdAsync(id, token);

                if (paymentMethod == null)
                {
                    return NotFound("Método de pagamento não encontrado");
                }

                if (paymentMethod.IsSystem)
                {
                    TempData["ErrorMessage"] = ERROR_CANNOT_DELETE_SYSTEM_METHOD;
                    return RedirectToAction(nameof(Details), new { id });
                }

                await _paymentMethodService.DeletePaymentMethodAsync(id, token);
                TempData["SuccessMessage"] = SUCCESS_DELETE_PAYMENT_METHOD;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_DELETING_PAYMENT_METHOD, ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        private List<SelectListItem> GetPaymentMethodTypes()
        {
            return
            [
                new() { Value = "1", Text = "Dinheiro" },
                new() { Value = "2", Text = "Cartão de Crédito" },
                new() { Value = "3", Text = "Cartão de Débito" },
                new() { Value = "4", Text = "Transferência Bancária" },
                new() { Value = "5", Text = "Carteira Digital" },
                new() { Value = "6", Text = "Cheque" },
                new() { Value = "7", Text = "Outro" }
            ];
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