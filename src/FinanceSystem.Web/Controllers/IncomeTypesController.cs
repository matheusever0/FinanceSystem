using FinanceSystem.Web.Extensions;
using FinanceSystem.Web.Filters;
using FinanceSystem.Web.Models.IncomeType;
using FinanceSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceSystem.Web.Controllers
{
    [Authorize]
    [RequirePermission("incomes.view")]
    public class IncomeTypesController : Controller
    {
        private readonly IIncomeTypeService _incomeTypeService;

        private const string ERROR_LOADING_INCOME_TYPES = "Erro ao carregar tipos de receita: {0}";
        private const string ERROR_LOADING_SYSTEM_INCOME_TYPES = "Erro ao carregar tipos de receita do sistema: {0}";
        private const string ERROR_LOADING_USER_INCOME_TYPES = "Erro ao carregar tipos de receita do usuário: {0}";
        private const string ERROR_LOADING_INCOME_TYPE_DETAILS = "Erro ao carregar detalhes do tipo de receita: {0}";
        private const string ERROR_CREATING_INCOME_TYPE = "Erro ao criar tipo de receita: {0}";
        private const string ERROR_LOADING_INCOME_TYPE_EDIT = "Erro ao carregar tipo de receita para edição: {0}";
        private const string ERROR_UPDATING_INCOME_TYPE = "Erro ao atualizar tipo de receita: {0}";
        private const string ERROR_LOADING_INCOME_TYPE_DELETE = "Erro ao carregar tipo de receita para exclusão: {0}";
        private const string ERROR_DELETING_INCOME_TYPE = "Erro ao excluir tipo de receita: {0}";
        private const string ERROR_CANNOT_EDIT_SYSTEM_TYPE = "Não é possível editar tipos de receita do sistema";
        private const string ERROR_CANNOT_DELETE_SYSTEM_TYPE = "Não é possível excluir tipos de receita do sistema";

        private const string SUCCESS_CREATE_INCOME_TYPE = "Tipo de receita criado com sucesso!";
        private const string SUCCESS_UPDATE_INCOME_TYPE = "Tipo de receita atualizado com sucesso!";
        private const string SUCCESS_DELETE_INCOME_TYPE = "Tipo de receita excluído com sucesso!";

        public IncomeTypesController(IIncomeTypeService incomeTypeService)
        {
            _incomeTypeService = incomeTypeService ?? throw new ArgumentNullException(nameof(incomeTypeService));
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var incomeTypes = await _incomeTypeService.GetAllIncomeTypesAsync(token);
                return View(incomeTypes);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_INCOME_TYPES, ex.Message);
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> System()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var incomeTypes = await _incomeTypeService.GetSystemIncomeTypesAsync(token);
                ViewBag.IsSystemView = true;
                ViewBag.Title = "Tipos de Receita do Sistema";
                return View("Index", incomeTypes);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_SYSTEM_INCOME_TYPES, ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> User()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var incomeTypes = await _incomeTypeService.GetUserIncomeTypesAsync(token);
                ViewBag.IsUserView = true;
                ViewBag.Title = "Meus Tipos de Receita";
                return View("Index", incomeTypes);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_USER_INCOME_TYPES, ex.Message);
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
                var token = HttpContext.GetJwtToken();
                var incomeType = await _incomeTypeService.GetIncomeTypeByIdAsync(id, token);

                return incomeType == null ? NotFound("Tipo de receita não encontrado") : View(incomeType);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_INCOME_TYPE_DETAILS, ex.Message);
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
                var token = HttpContext.GetJwtToken();
                var incomeType = await _incomeTypeService.CreateIncomeTypeAsync(model, token);
                TempData["SuccessMessage"] = SUCCESS_CREATE_INCOME_TYPE;
                return RedirectToAction(nameof(Details), new { id = incomeType.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, string.Format(ERROR_CREATING_INCOME_TYPE, ex.Message));
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
                var token = HttpContext.GetJwtToken();
                var incomeType = await _incomeTypeService.GetIncomeTypeByIdAsync(id, token);

                if (incomeType == null)
                {
                    return NotFound("Tipo de receita não encontrado");
                }

                if (incomeType.IsSystem)
                {
                    TempData["ErrorMessage"] = ERROR_CANNOT_EDIT_SYSTEM_TYPE;
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
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_INCOME_TYPE_EDIT, ex.Message);
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
                var token = HttpContext.GetJwtToken();
                var incomeType = await _incomeTypeService.GetIncomeTypeByIdAsync(id, token);

                if (incomeType == null)
                {
                    return NotFound("Tipo de receita não encontrado");
                }

                if (incomeType.IsSystem)
                {
                    TempData["ErrorMessage"] = ERROR_CANNOT_EDIT_SYSTEM_TYPE;
                    return RedirectToAction(nameof(Details), new { id });
                }

                await _incomeTypeService.UpdateIncomeTypeAsync(id, model, token);
                TempData["SuccessMessage"] = SUCCESS_UPDATE_INCOME_TYPE;
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, string.Format(ERROR_UPDATING_INCOME_TYPE, ex.Message));
                return View(model);
            }
        }

        [RequirePermission("incomes.delete")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do tipo de receita não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var incomeType = await _incomeTypeService.GetIncomeTypeByIdAsync(id, token);

                if (incomeType == null)
                {
                    return NotFound("Tipo de receita não encontrado");
                }

                if (incomeType.IsSystem)
                {
                    TempData["ErrorMessage"] = ERROR_CANNOT_DELETE_SYSTEM_TYPE;
                    return RedirectToAction(nameof(Details), new { id });
                }

                return View(incomeType);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_INCOME_TYPE_DELETE, ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [RequirePermission("incomes.delete")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do tipo de receita não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var incomeType = await _incomeTypeService.GetIncomeTypeByIdAsync(id, token);

                if (incomeType == null)
                {
                    return NotFound("Tipo de receita não encontrado");
                }

                if (incomeType.IsSystem)
                {
                    TempData["ErrorMessage"] = ERROR_CANNOT_DELETE_SYSTEM_TYPE;
                    return RedirectToAction(nameof(Details), new { id });
                }

                await _incomeTypeService.DeleteIncomeTypeAsync(id, token);
                TempData["SuccessMessage"] = SUCCESS_DELETE_INCOME_TYPE;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_DELETING_INCOME_TYPE, ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }
    }
}