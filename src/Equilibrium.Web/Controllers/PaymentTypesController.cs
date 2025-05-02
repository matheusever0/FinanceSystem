using Equilibrium.Resources.Web.Enums;
using Equilibrium.Resources.Web.Helpers;
using Equilibrium.Web.Extensions;
using Equilibrium.Web.Filters;
using Equilibrium.Web.Models;
using Equilibrium.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Equilibrium.Web.Controllers
{
    [Authorize]
    [RequirePermission("paymenttypes.view")]
    public class PaymentTypesController : Controller
    {
        private readonly IPaymentTypeService _paymentTypeService;

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
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.PaymentType, ex);
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
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.PaymentType, ex);
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
                var token = HttpContext.GetJwtToken();
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
                var token = HttpContext.GetJwtToken();
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
                var token = HttpContext.GetJwtToken();
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
                var token = HttpContext.GetJwtToken();
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
                    TempData["ErrorMessage"] = MessageHelper.GetCannotDeleteSystemEntityMessage(EntityNames.PaymentType);
                    return RedirectToAction(nameof(Details), new { id });
                }

                return View(paymentType);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.PaymentType, ex);
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
                    TempData["ErrorMessage"] = MessageHelper.GetCannotDeleteSystemEntityMessage(EntityNames.PaymentType);
                    return RedirectToAction(nameof(Details), new { id });
                }

                await _paymentTypeService.DeletePaymentTypeAsync(id, token);
                TempData["SuccessMessage"] = MessageHelper.GetDeletionSuccessMessage(EntityNames.PaymentType);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetDeletionErrorMessage(EntityNames.PaymentType, ex);
                return RedirectToAction(nameof(Index));
            }
        }
    }
}