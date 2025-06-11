using Equilibrium.Resources.Web.Enums;
using Equilibrium.Resources.Web.Helpers;
using Equilibrium.Web.Extensions;
using Equilibrium.Web.Filters;
using Equilibrium.Web.Interfaces;
using Equilibrium.Web.Models.IncomeType;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Equilibrium.Web.Controllers
{
    [Authorize]
    [RequirePermission("incomes.view")]
    public class IncomeTypesController : BaseController
    {
        private readonly IIncomeTypeService _incomeTypeService;
        private readonly IIncomeService _incomeService;

        public IncomeTypesController(IIncomeTypeService incomeTypeService, IIncomeService incomeService)
        {
            _incomeTypeService = incomeTypeService;
            _incomeService = incomeService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var token = GetToken();
                var incomeTypes = await _incomeTypeService.GetAllIncomeTypesAsync(token);
                return View(incomeTypes);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.IncomeType, ex);
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> System()
        {
            try
            {
                var token = GetToken();
                var incomeTypes = await _incomeTypeService.GetSystemIncomeTypesAsync(token);
                ViewBag.IsSystemView = true;
                ViewBag.Title = "Tipos de Receita do Sistema";
                return View("Index", incomeTypes);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.IncomeType, ex);
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> User()
        {
            try
            {
                var token = GetToken();
                var incomeTypes = await _incomeTypeService.GetUserIncomeTypesAsync(token);
                ViewBag.IsUserView = true;
                ViewBag.Title = "Meus Tipos de Receita";
                return View("Index", incomeTypes);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.IncomeType, ex);
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do tipo de receita não fornecido");
            }

            try
            {
                var token = GetToken();
                var incomeType = await _incomeTypeService.GetIncomeTypeByIdAsync(id, token);

                return incomeType == null ? NotFound("Tipo de receita não encontrado") : View(incomeType);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.IncomeType, ex);
                return RedirectToAction(nameof(Index));
            }
        }

        [RequirePermission("incomes.create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("incomes.create")]
        public async Task<IActionResult> Create(CreateIncomeTypeModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var token = GetToken();
                var incomeType = await _incomeTypeService.CreateIncomeTypeAsync(model, token);
                TempData["SuccessMessage"] = MessageHelper.GetCreationSuccessMessage(EntityNames.IncomeType);
                return RedirectToAction(nameof(Details), new { id = incomeType.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, MessageHelper.GetCreationErrorMessage(EntityNames.IncomeType, ex));
                return View(model);
            }
        }

        [RequirePermission("incomes.edit")]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do tipo de receita não fornecido");
            }

            try
            {
                var token = GetToken();
                var incomeType = await _incomeTypeService.GetIncomeTypeByIdAsync(id, token);

                if (incomeType == null)
                {
                    return NotFound("Tipo de receita não encontrado");
                }

                if (incomeType.IsSystem)
                {
                    TempData["ErrorMessage"] = MessageHelper.GetCannotEditSystemEntityMessage(EntityNames.IncomeType);
                    return RedirectToAction(nameof(Details), new { id });
                }

                var model = new UpdateIncomeTypeModel
                {
                    Name = incomeType.Name,
                    Description = incomeType.Description
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.IncomeType, ex);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("incomes.edit")]
        public async Task<IActionResult> Edit(string id, UpdateIncomeTypeModel model)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do tipo de receita não fornecido");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var token = GetToken();
                var incomeType = await _incomeTypeService.GetIncomeTypeByIdAsync(id, token);

                if (incomeType == null)
                {
                    return NotFound("Tipo de receita não encontrado");
                }

                if (incomeType.IsSystem)
                {
                    TempData["ErrorMessage"] = MessageHelper.GetCannotEditSystemEntityMessage(EntityNames.IncomeType);
                    return RedirectToAction(nameof(Details), new { id });
                }

                await _incomeTypeService.UpdateIncomeTypeAsync(id, model, token);
                TempData["SuccessMessage"] = MessageHelper.GetUpdateSuccessMessage(EntityNames.IncomeType);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, MessageHelper.GetUpdateErrorMessage(EntityNames.IncomeType, ex));
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("incometypes.delete")]
        public async Task<IActionResult> Delete(string id)
        {
            return await HandleGenericDelete(
                id,
                _incomeTypeService,
                async (service, itemId, token) => await service.DeleteIncomeTypeAsync(itemId, token),
                async (service, itemId, token) => await service.GetIncomeTypeByIdAsync(itemId, token),
                "tipo de receita",
                null,
                async (item) => {
                    if (item is IncomeTypeModel incomeType)
                    {
                        var incomesCount = await _incomeService.GetIncomesByTypeAsync(incomeType.Id, GetToken());
                        if (incomesCount.Any())
                        {
                            return (false, $"Este tipo possui {incomesCount.Count()} receitas associadas. Não é possível excluir.");
                        }
                    }
                    return (true, null);
                }
            );
        }
    }
}
