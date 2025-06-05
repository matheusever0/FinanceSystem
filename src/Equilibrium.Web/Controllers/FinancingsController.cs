using Equilibrium.Resources.Web;
using Equilibrium.Resources.Web.Enums;
using Equilibrium.Resources.Web.Helpers;
using Equilibrium.Web.Extensions;
using Equilibrium.Web.Filters;
using Equilibrium.Web.Interfaces;
using Equilibrium.Web.Models.Financing;
using Equilibrium.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Equilibrium.Web.Controllers
{
    [Authorize]
    [RequirePermission("financings.view")]
    public class FinancingsController : Controller
    {
        private readonly IFinancingService _financingService;
        private readonly IPaymentTypeService _paymentTypeService;

        public FinancingsController(
            IFinancingService financingService,
            IPaymentTypeService paymentTypeService)
        {
            _financingService = financingService;
            _paymentTypeService = paymentTypeService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var financings = await _financingService.GetAllFinancingsAsync(token);

                return View(financings);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Financing, ex);
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Active()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var financings = await _financingService.GetActiveFinancingsAsync(token);
                ViewBag.Title = "Financiamentos Ativos";
                return View("Index", financings);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Financing, ex);
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do financiamento não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var financing = await _financingService.GetFinancingDetailsAsync(id, token);
                return financing == null
                    ? NotFound("Financiamento não encontrado")
                    : View(financing);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Financing, ex);
                return RedirectToAction(nameof(Index));
            }
        }

        [RequirePermission("financings.create")]
        public async Task<IActionResult> Create()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var paymentTypes = await _paymentTypeService.GetAllPaymentTypesAsync(token);

                // Filtrar tipos de pagamento marcados como de financiamento
                var financingPaymentTypes = paymentTypes.Where(pt => pt.IsFinancingType).ToList();

                if (!financingPaymentTypes.Any())
                {
                    TempData["ErrorMessage"] = "Não há tipos de pagamento configurados para financiamento";
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.PaymentTypes = financingPaymentTypes;
                ViewBag.CorrectionIndexes = GetCorrectionIndexes();
                ViewBag.FinancingTypes = GetFinancingTypes();

                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ResourceFinanceWeb.Error_PreparingForm;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("financings.create")]
        public async Task<IActionResult> Create(CreateFinancingModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadFormDependencies();
                return View(model);
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var financing = await _financingService.CreateFinancingAsync(model, token);
                TempData["SuccessMessage"] = MessageHelper.GetCreationSuccessMessage(EntityNames.Financing);
                return RedirectToAction(nameof(Details), new { id = financing.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, MessageHelper.GetCreationErrorMessage(EntityNames.Financing, ex));
                await LoadFormDependencies();
                return View(model);
            }
        }

        [RequirePermission("financings.simulate")]
        public IActionResult Simulate()
        {
            ViewBag.CorrectionIndexes = GetCorrectionIndexes();
            ViewBag.FinancingTypes = GetFinancingTypes();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("financings.simulate")]
        public async Task<IActionResult> Simulate(FinancingSimulationRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CorrectionIndexes = GetCorrectionIndexes();
                ViewBag.FinancingTypes = GetFinancingTypes();
                return View(model);
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var simulation = await _financingService.SimulateFinancingAsync(model, token);

                ViewBag.Simulation = simulation;
                ViewBag.CorrectionIndexes = GetCorrectionIndexes();
                ViewBag.FinancingTypes = GetFinancingTypes();

                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Erro ao simular financiamento: " + ex.Message);
                ViewBag.CorrectionIndexes = GetCorrectionIndexes();
                ViewBag.FinancingTypes = GetFinancingTypes();
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("financings.edit")]
        public async Task<IActionResult> Cancel(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do financiamento não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                await _financingService.CancelFinancingAsync(id, token);
                TempData["SuccessMessage"] = MessageHelper.GetCancelSuccessMessage(EntityNames.Financing);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetCancelErrorMessage(EntityNames.Financing, ex);
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("financings.edit")]
        public async Task<IActionResult> Complete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do financiamento não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                await _financingService.CompleteFinancingAsync(id, token);
                TempData["SuccessMessage"] = "Financiamento marcado como concluído com sucesso";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Erro ao marcar financiamento como concluído: " + ex.Message;
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [HttpGet]
        [RequirePermission("financings.view")]
        public async Task<IActionResult> GetInstallments(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do financiamento não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var installments = await _financingService.GetFinancingInstallmentsAsync(id, token);
                return Json(installments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erro ao obter parcelas: " + ex.Message);
            }
        }

        [HttpGet]
        [RequirePermission("financings.view")]
        public async Task<IActionResult> GetInstallmentsByPending(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do financiamento não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var installments = await _financingService.GetFinancingInstallmentsAsync(id, token);
                return Json(installments.Where(e => e.Status == 1));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erro ao obter parcelas: " + ex.Message);
            }
        }

        private List<SelectListItem> GetCorrectionIndexes()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "5", Text = "Fixo (sem correção)" },
                new SelectListItem { Value = "1", Text = "IPCA" },
                new SelectListItem { Value = "3", Text = "SELIC" },
                new SelectListItem { Value = "2", Text = "TR" },
                new SelectListItem { Value = "4", Text = "IGPM" }
            };
        }

        private List<SelectListItem> GetFinancingTypes()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Price (Prestações fixas)" },
                new SelectListItem { Value = "2", Text = "SAC (Amortizações iguais)" }
            };
        }

        private async Task LoadFormDependencies()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var paymentTypes = await _paymentTypeService.GetAllPaymentTypesAsync(token);
                var financingPaymentTypes = paymentTypes.Where(pt => pt.IsFinancingType).ToList();

                ViewBag.PaymentTypes = financingPaymentTypes;
                ViewBag.CorrectionIndexes = GetCorrectionIndexes();
                ViewBag.FinancingTypes = GetFinancingTypes();
            }
            catch
            {
                ViewBag.PaymentTypes = new List<object>();
                ViewBag.CorrectionIndexes = GetCorrectionIndexes();
                ViewBag.FinancingTypes = GetFinancingTypes();
            }
        }
    }
}
