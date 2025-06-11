using Equilibrium.Resources.Web.Enums;
using Equilibrium.Resources.Web.Helpers;
using Equilibrium.Web.Filters;
using Equilibrium.Web.Interfaces;
using Equilibrium.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Equilibrium.Web.Controllers
{
    [Authorize]
    [RequirePermission("paymenttypes.view")]
    public class PaymentTypesController : BaseController
    {
        private readonly IPaymentTypeService _paymentTypeService;
        private readonly IPaymentService _paymentService;

        public PaymentTypesController(IPaymentTypeService paymentTypeService, IPaymentService paymentService)
        {
            _paymentTypeService = paymentTypeService;
            _paymentService = paymentService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var token = GetToken();
                var paymentTypes = await _paymentTypeService.GetAllPaymentTypesAsync(token);
                return View(paymentTypes);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.PaymentType, ex);
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> System()
        {
            try
            {
                var token = GetToken();
                var paymentTypes = await _paymentTypeService.GetSystemPaymentTypesAsync(token);
                ViewBag.IsSystemView = true;
                ViewBag.Title = "Tipos de Pagamento do Sistema";
                return View("Index", paymentTypes);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.PaymentType, ex);
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> User()
        {
            try
            {
                var token = GetToken();
                var paymentTypes = await _paymentTypeService.GetUserPaymentTypesAsync(token);
                ViewBag.IsUserView = true;
                ViewBag.Title = "Meus Tipos de Pagamento";
                return View("Index", paymentTypes);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.PaymentType, ex);
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
                var token = GetToken();
                var paymentType = await _paymentTypeService.GetPaymentTypeByIdAsync(id, token);

                return paymentType == null ? NotFound("Tipo de pagamento não encontrado") : View(paymentType);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.PaymentType, ex);
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
                var token = GetToken();
                var paymentType = await _paymentTypeService.CreatePaymentTypeAsync(model, token);
                TempData["SuccessMessage"] = MessageHelper.GetCreationSuccessMessage(EntityNames.PaymentType);
                return RedirectToAction(nameof(Details), new { id = paymentType.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, MessageHelper.GetCreationErrorMessage(EntityNames.PaymentType, ex));
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
                var token = GetToken();
                var paymentType = await _paymentTypeService.GetPaymentTypeByIdAsync(id, token);

                if (paymentType == null)
                {
                    return NotFound("Tipo de pagamento não encontrado");
                }

                if (paymentType.IsSystem)
                {
                    TempData["ErrorMessage"] = MessageHelper.GetCannotEditSystemEntityMessage(EntityNames.PaymentType);
                    return RedirectToAction(nameof(Details), new { id });
                }

                var model = new UpdatePaymentTypeModel
                {
                    Name = paymentType.Name,
                    Description = paymentType.Description,
                    IsFinancingType = paymentType.IsFinancingType
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.PaymentType, ex);
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
                var token = GetToken();
                var paymentType = await _paymentTypeService.GetPaymentTypeByIdAsync(id, token);

                if (paymentType == null)
                {
                    return NotFound("Tipo de pagamento não encontrado");
                }

                if (paymentType.IsSystem)
                {
                    TempData["ErrorMessage"] = MessageHelper.GetCannotEditSystemEntityMessage(EntityNames.PaymentType);
                    return RedirectToAction(nameof(Details), new { id });
                }

                await _paymentTypeService.UpdatePaymentTypeAsync(id, model, token);
                TempData["SuccessMessage"] = MessageHelper.GetUpdateSuccessMessage(EntityNames.PaymentType);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, MessageHelper.GetUpdateErrorMessage(EntityNames.PaymentType, ex));
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("paymenttypes.delete")]
        public async Task<IActionResult> Delete(string id)
        {
            return await HandleGenericDelete(
                id,
                _paymentTypeService,
                async (service, itemId, token) => await service.DeletePaymentTypeAsync(itemId, token),
                async (service, itemId, token) => await service.GetPaymentTypeByIdAsync(itemId, token),
                "tipo de pagamento",
                null,
                async (item) => {
                    if (item is PaymentTypeModel paymentType)
                    {
                        var paymentsCount = await _paymentService.GetPaymentsByTypeAsync(paymentType.Id, GetToken());
                        if (paymentsCount.Any())
                        {
                            return (false, $"Este tipo possui {paymentsCount.Count()} pagamentos associados. Não é possível excluir.");
                        }
                    }
                    return (true, null);
                }
            );
        }
    }
}
