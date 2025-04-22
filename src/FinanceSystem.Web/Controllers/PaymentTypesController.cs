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

        private const string ERROR_LOADING_PAYMENT_TYPES = "Erro ao carregar tipos de pagamento: {0}";
        private const string ERROR_LOADING_SYSTEM_PAYMENT_TYPES = "Erro ao carregar tipos de pagamento do sistema: {0}";
        private const string ERROR_LOADING_USER_PAYMENT_TYPES = "Erro ao carregar tipos de pagamento do usuário: {0}";
        private const string ERROR_LOADING_PAYMENT_TYPE_DETAILS = "Erro ao carregar detalhes do tipo de pagamento: {0}";
        private const string ERROR_CREATING_PAYMENT_TYPE = "Erro ao criar tipo de pagamento: {0}";
        private const string ERROR_LOADING_PAYMENT_TYPE_EDIT = "Erro ao carregar tipo de pagamento para edição: {0}";
        private const string ERROR_UPDATING_PAYMENT_TYPE = "Erro ao atualizar tipo de pagamento: {0}";
        private const string ERROR_LOADING_PAYMENT_TYPE_DELETE = "Erro ao carregar tipo de pagamento para exclusão: {0}";
        private const string ERROR_DELETING_PAYMENT_TYPE = "Erro ao excluir tipo de pagamento: {0}";
        private const string ERROR_CANNOT_EDIT_SYSTEM_TYPE = "Não é possível editar tipos de pagamento do sistema";
        private const string ERROR_CANNOT_DELETE_SYSTEM_TYPE = "Não é possível excluir tipos de pagamento do sistema";

        private const string SUCCESS_CREATE_PAYMENT_TYPE = "Tipo de pagamento criado com sucesso!";
        private const string SUCCESS_UPDATE_PAYMENT_TYPE = "Tipo de pagamento atualizado com sucesso!";
        private const string SUCCESS_DELETE_PAYMENT_TYPE = "Tipo de pagamento excluído com sucesso!";

        public PaymentTypesController(IPaymentTypeService paymentTypeService)
        {
            _paymentTypeService = paymentTypeService ?? throw new ArgumentNullException(nameof(paymentTypeService));
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
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_PAYMENT_TYPES, ex.Message);
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
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_SYSTEM_PAYMENT_TYPES, ex.Message);
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
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_USER_PAYMENT_TYPES, ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do tipo de pagamento não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var paymentType = await _paymentTypeService.GetPaymentTypeByIdAsync(id, token);

                if (paymentType == null)
                {
                    return NotFound("Tipo de pagamento não encontrado");
                }

                return View(paymentType);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_PAYMENT_TYPE_DETAILS, ex.Message);
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
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var paymentType = await _paymentTypeService.CreatePaymentTypeAsync(model, token);
                TempData["SuccessMessage"] = SUCCESS_CREATE_PAYMENT_TYPE;
                return RedirectToAction(nameof(Details), new { id = paymentType.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, string.Format(ERROR_CREATING_PAYMENT_TYPE, ex.Message));
                return View(model);
            }
        }

        [RequirePermission("paymenttypes.edit")]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do tipo de pagamento não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var paymentType = await _paymentTypeService.GetPaymentTypeByIdAsync(id, token);

                if (paymentType == null)
                {
                    return NotFound("Tipo de pagamento não encontrado");
                }

                if (paymentType.IsSystem)
                {
                    TempData["ErrorMessage"] = ERROR_CANNOT_EDIT_SYSTEM_TYPE;
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
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_PAYMENT_TYPE_EDIT, ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("paymenttypes.edit")]
        public async Task<IActionResult> Edit(string id, UpdatePaymentTypeModel model)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do tipo de pagamento não fornecido");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var paymentType = await _paymentTypeService.GetPaymentTypeByIdAsync(id, token);

                if (paymentType == null)
                {
                    return NotFound("Tipo de pagamento não encontrado");
                }

                if (paymentType.IsSystem)
                {
                    TempData["ErrorMessage"] = ERROR_CANNOT_EDIT_SYSTEM_TYPE;
                    return RedirectToAction(nameof(Details), new { id });
                }

                await _paymentTypeService.UpdatePaymentTypeAsync(id, model, token);
                TempData["SuccessMessage"] = SUCCESS_UPDATE_PAYMENT_TYPE;
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, string.Format(ERROR_UPDATING_PAYMENT_TYPE, ex.Message));
                return View(model);
            }
        }

        [RequirePermission("paymenttypes.delete")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do tipo de pagamento não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var paymentType = await _paymentTypeService.GetPaymentTypeByIdAsync(id, token);

                if (paymentType == null)
                {
                    return NotFound("Tipo de pagamento não encontrado");
                }

                if (paymentType.IsSystem)
                {
                    TempData["ErrorMessage"] = ERROR_CANNOT_DELETE_SYSTEM_TYPE;
                    return RedirectToAction(nameof(Details), new { id });
                }

                return View(paymentType);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_PAYMENT_TYPE_DELETE, ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [RequirePermission("paymenttypes.delete")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do tipo de pagamento não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var paymentType = await _paymentTypeService.GetPaymentTypeByIdAsync(id, token);

                if (paymentType == null)
                {
                    return NotFound("Tipo de pagamento não encontrado");
                }

                if (paymentType.IsSystem)
                {
                    TempData["ErrorMessage"] = ERROR_CANNOT_DELETE_SYSTEM_TYPE;
                    return RedirectToAction(nameof(Details), new { id });
                }

                await _paymentTypeService.DeletePaymentTypeAsync(id, token);
                TempData["SuccessMessage"] = SUCCESS_DELETE_PAYMENT_TYPE;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_DELETING_PAYMENT_TYPE, ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }
    }
}