using Equilibrium.Resources.Web;
using Equilibrium.Resources.Web.Enums;
using Equilibrium.Resources.Web.Helpers;
using Equilibrium.Web.Filters;
using Equilibrium.Web.Helpers;
using Equilibrium.Web.Interfaces;
using Equilibrium.Web.Models.Filters;
using Equilibrium.Web.Models.Income;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;
using System.Text;

namespace Equilibrium.Web.Controllers
{
    [Authorize]
    [RequirePermission("incomes.view")]
    public class IncomesController : BaseController
    {
        private readonly IIncomeService _incomeService;
        private readonly IIncomeTypeService _incomeTypeService;

        public IncomesController(
            IIncomeService incomeService,
            IIncomeTypeService incomeTypeService)
        {
            _incomeService = incomeService;
            _incomeTypeService = incomeTypeService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var token = GetToken();

                var filter = FilterCacheHelper.GetIncomeFilter(HttpContext.Session);

                await LoadFilterData(token);

                if (filter == null)
                    return View(new List<IncomeModel>());

                var hasActiveFilters = filter.HasFilters();
                var incomes = await _incomeService.GetFilteredIncomesAsync(filter, token);

                ViewBag.CurrentFilter = filter;
                ViewBag.HasActiveFilters = hasActiveFilters;

                return View(incomes);
            }
            catch (Exception ex)
            {
                return HandleException(ex, EntityNames.Income, "loading");
            }
        }

        [HttpPost]
        public IActionResult ApplyFilters(IncomeFilter filter)
        {
            try
            {
                FilterCacheHelper.SaveIncomeFilter(HttpContext.Session, filter);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return HandleException(ex, EntityNames.Income, "filtering");
            }
        }

        public IActionResult QuickFilter(string filterType)
        {
            try
            {
                var filter = filterType.ToLower() switch
                {
                    "thismonth" => new IncomeFilter { Month = DateTime.Now.Month, Year = DateTime.Now.Year, OrderBy = "dueDate", Ascending = true },
                    "thisweek" => CreateThisWeekIncomeFilter(),
                    "pending" => FilterHelper.QuickFilters.PendingIncomes(),
                    "received" => FilterHelper.QuickFilters.ReceivedThisMonth(),
                    "overdue" => FilterHelper.QuickFilters.OverdueIncomes(),
                    _ => new IncomeFilter()
                };

                FilterCacheHelper.SaveIncomeFilter(HttpContext.Session, filter);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return HandleException(ex, EntityNames.Income, "filtering");
            }
        }

        public IActionResult ClearFilters()
        {
            try
            {
                FilterCacheHelper.ClearIncomeFilter(HttpContext.Session);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return HandleException(ex, EntityNames.Income, "clearing filters");
            }
        }

        private static IncomeFilter CreateThisWeekIncomeFilter()
        {
            var now = DateTime.Now;
            var startOfWeek = now.AddDays(-(int)now.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(6);

            return new IncomeFilter
            {
                StartDate = startOfWeek.Date,
                EndDate = endOfWeek.Date,
                OrderBy = "dueDate",
                Ascending = true
            };
        }

        private async Task LoadFilterData(string token)
        {
            try
            {
                var incomeTypes = await _incomeTypeService.GetAllIncomeTypesAsync(token);

                var statusOptions = new List<SelectListItem>
            {
                new () { Value = "Pending", Text = "Pendente" },
                new () { Value = "Received", Text = "Recebido" },
                new () { Value = "Cancelled", Text = "Cancelado" }
            };

                var orderByOptions = new List<SelectListItem>
            {
                new () { Value = "dueDate", Text = "Data de Vencimento" },
                new () { Value = "description", Text = "Descrição" },
                new () { Value = "amount", Text = "Valor" },
                new () { Value = "receivedDate", Text = "Data de Recebimento" },
                new () { Value = "status", Text = "Status" },
                new () { Value = "createdAt", Text = "Data de Criação" }
            };

                ViewBag.IncomeTypes = incomeTypes;
                ViewBag.StatusOptions = statusOptions;
                ViewBag.OrderByOptions = orderByOptions;
            }
            catch (Exception)
            {
                ViewBag.IncomeTypes = new List<object>();
                ViewBag.StatusOptions = new List<SelectListItem>();
                ViewBag.OrderByOptions = new List<SelectListItem>();
            }
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID da receita não fornecido");
            }

            try
            {
                var token = GetToken();
                var income = await _incomeService.GetIncomeByIdAsync(id, token);

                return income == null ? NotFound("Receita não encontrada") : View(income);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Income, ex);
                return RedirectToAction(nameof(Index));
            }
        }

        [RequirePermission("incomes.create")]
        public async Task<IActionResult> Create()
        {
            try
            {
                var token = GetToken();
                var incomeTypes = await _incomeTypeService.GetAllIncomeTypesAsync(token);
                ViewBag.IncomeTypes = incomeTypes;
                return View();
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = ResourceFinanceWeb.Error_PreparingForm;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("incomes.create")]
        public async Task<IActionResult> Create(CreateIncomeModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadIncomeTypesForView();
                return View(model);
            }

            try
            {
                var token = GetToken();
                var income = await _incomeService.CreateIncomeAsync(model, token);
                TempData["SuccessMessage"] = MessageHelper.GetCreationSuccessMessage(EntityNames.Income);
                return RedirectToAction(nameof(Details), new { id = income.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, MessageHelper.GetCreationErrorMessage(EntityNames.Income, ex));
                await LoadIncomeTypesForView();
                return View(model);
            }
        }

        [RequirePermission("incomes.edit")]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID da receita não fornecido");
            }

            try
            {
                var token = GetToken();
                var income = await _incomeService.GetIncomeByIdAsync(id, token);

                if (income == null)
                {
                    return NotFound("Receita não encontrada");
                }

                var incomeTypes = await _incomeTypeService.GetAllIncomeTypesAsync(token);
                ViewBag.IncomeTypes = incomeTypes;

                var model = new UpdateIncomeModel
                {
                    Id = income.Id,
                    Description = income.Description,
                    Amount = income.Amount,
                    DueDate = income.DueDate,
                    ReceivedDate = income.ReceivedDate,
                    Status = income.Status,
                    IsRecurring = income.IsRecurring,
                    Notes = income.Notes,
                    IncomeTypeId = income.IncomeTypeId
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Income, ex);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("incomes.edit")]
        public async Task<IActionResult> Edit(string id, UpdateIncomeModel model)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID da receita não fornecido");
            }

            if (!ModelState.IsValid)
            {
                await LoadIncomeTypesForView();
                return View(model);
            }

            try
            {
                var token = GetToken();
                await _incomeService.UpdateIncomeAsync(id, model, token);
                TempData["SuccessMessage"] = MessageHelper.GetUpdateSuccessMessage(EntityNames.Income);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, MessageHelper.GetUpdateErrorMessage(EntityNames.Income, ex));
                await LoadIncomeTypesForView();
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("incomes.delete")]
        public async Task<IActionResult> Delete(string id)
        {
            return await HandleGenericDelete(
                id,
                _incomeService,
                async (service, itemId, token) => await service.DeleteIncomeAsync(itemId, token),
                async (service, itemId, token) => await service.GetIncomeByIdAsync(itemId, token),
                "receita",
                null,
                async (item) => {
                    if (item is IncomeModel income && income.Status == 2)
                    {
                        return (false, "Não é possível excluir uma receita que já foi paga.");
                    }
                    return (true, null);
                }
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("incomes.edit")]
        public async Task<IActionResult> Receive(string id, DateTime receivedDate)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "ID da receita não fornecido.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var token = GetToken();

                var income = await _incomeService.GetIncomeByIdAsync(id, token);
                if (income == null)
                {
                    TempData["ErrorMessage"] = "Receita não encontrada.";
                    return RedirectToAction(nameof(Index));
                }

                if (income.Status != 1) 
                {
                    TempData["ErrorMessage"] = "Esta receita não pode ser marcada como recebida.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                // Validar data de recebimento
                if (receivedDate > DateTime.Today)
                {
                    TempData["ErrorMessage"] = "A data de recebimento não pode ser futura.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                await _incomeService.MarkAsReceivedAsync(id, receivedDate, token);
                TempData["SuccessMessage"] = $"Receita '{income.Description}' marcada como recebida.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao marcar receita como recebida: {ex.Message}";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("incomes.edit")]
        public async Task<IActionResult> MarkAsReceived(string id, DateTime? receivedDate = null)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID da receita não fornecido");
            }

            try
            {
                var token = GetToken();
                await _incomeService.MarkAsReceivedAsync(id, receivedDate ?? DateTime.Now, token);
                TempData["SuccessMessage"] = MessageHelper.GetStatusChangeSuccessMessage(EntityNames.Income, EntityStatus.Received);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetStatusChangeErrorMessage(EntityNames.Income, EntityStatus.Received, ex);
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("incomes.edit")]
        public async Task<IActionResult> Cancel(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID da receita não fornecido");
            }

            try
            {
                var token = GetToken();
                await _incomeService.CancelIncomeAsync(id, token);
                TempData["SuccessMessage"] = MessageHelper.GetCancelSuccessMessage(EntityNames.Income);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetCancelErrorMessage(EntityNames.Income, ex);
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [HttpGet]
        public IActionResult FilterByMonth(int month, int year)
        {
            var filter = new IncomeFilter
            {
                Month = month,
                Year = year
            };

            return ApplyFilters(filter);
        }

        [RequirePermission("incomes.view")]
        public IActionResult Export()
        {
            return View();
        }

        [RequirePermission("incomes.create")]
        public IActionResult DownloadTemplate()
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("Descricao,Valor,DataVencimento,DataRecebimento,TipoReceitaId,Recorrente,Parcelas,Observacoes");

                sb.AppendLine("Sal?rio,3500.00,25/06/2025,,id-tipo-receita-1,Sim,1,'Sal?rio mensal'");
                sb.AppendLine("Freelance,1200.00,15/06/2025,15/06/2025,id-tipo-receita-2,N?o,1,'Projeto de design'");
                sb.AppendLine("Aluguel,1800.00,10/06/2025,,id-tipo-receita-3,Sim,1,'Aluguel do im?vel'");
                sb.AppendLine("Investimentos,350.00,30/06/2025,,id-tipo-receita-4,N?o,1,'Rendimentos'");

                string content = sb.ToString();
                byte[] utf8Bytes = Encoding.UTF8.GetBytes(content);
                byte[] latin1Bytes = Encoding.Convert(Encoding.UTF8, Encoding.GetEncoding("ISO-8859-1"), utf8Bytes);
                byte[] resultBytes = Encoding.Convert(Encoding.GetEncoding("ISO-8859-1"), Encoding.UTF8, latin1Bytes);

                byte[] bom = Encoding.UTF8.GetPreamble();
                byte[] finalBytes = new byte[bom.Length + resultBytes.Length];
                Buffer.BlockCopy(bom, 0, finalBytes, 0, bom.Length);
                Buffer.BlockCopy(resultBytes, 0, finalBytes, bom.Length, resultBytes.Length);

                return File(finalBytes, "application/octet-stream", "modelo_receitas.csv");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Erro ao gerar o modelo de CSV: " + ex.Message;
                return RedirectToAction(nameof(Export));
            }
        }

        [HttpPost]
        [RequirePermission("incomes.create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportFromCSV(IFormFile csvFile, bool validateOnly = false)
        {
            if (csvFile == null || csvFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Nenhum arquivo foi selecionado.";
                return RedirectToAction(nameof(Export));
            }

            if (!csvFile.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "O arquivo deve ser um CSV válido.";
                return RedirectToAction(nameof(Export));
            }

            try
            {
                var token = GetToken();
                var successCount = 0;
                var errorCount = 0;
                var errors = new List<string>();

                using (var reader = new StreamReader(csvFile.OpenReadStream()))
                {
                    var header = await reader.ReadLineAsync();
                    if (header == null)
                    {
                        TempData["ErrorMessage"] = "O arquivo CSV está vazio ou não possui cabeçalho.";
                        return RedirectToAction(nameof(Export));
                    }

                    var expectedColumns = new[] { "Descricao", "Valor", "DataVencimento", "DataRecebimento",
                "TipoReceitaId", "Recorrente", "Parcelas", "Observacoes" };
                    var headerColumns = header.Split(';').Select(h => h.Trim()).ToArray();

                    if (!ValidateHeader(headerColumns, expectedColumns, out var missingColumns))
                    {
                        TempData["ErrorMessage"] = $"O cabeçalho do CSV está invalido. Colunas ausentes: {string.Join(", ", missingColumns)}";
                        return RedirectToAction(nameof(Export));
                    }

                    var lineNumber = 1; 

                    while (!reader.EndOfStream)
                    {
                        lineNumber++;
                        var line = await reader.ReadLineAsync();
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        try
                        {
                            var model = ParseIncomeLine(line, headerColumns);

                            if (!validateOnly)
                            {
                                await _incomeService.CreateIncomeAsync(model, token);
                            }

                            successCount++;
                        }
                        catch (Exception ex)
                        {
                            errorCount++;
                            errors.Add($"Erro na linha {lineNumber}: {ex.Message}");

                            // If more than 10 errors, stop processing
                            if (errors.Count > 10)
                            {
                                errors.Add("Muitos erros encontrados. Processamento interrompido.");
                                break;
                            }
                        }
                    }
                }

                if (validateOnly)
                {
                    if (errorCount == 0)
                    {
                        TempData["SuccessMessage"] = $"Validação concluída com sucesso. {successCount} receitas estão prontas para importação.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = $"Validação concluída com {errorCount} erros. {string.Join("<br>", errors)}";
                    }
                }
                else
                {
                    if (errorCount == 0)
                    {
                        TempData["SuccessMessage"] = $"{successCount} receitas foram importadas com sucesso.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = $"{successCount} receitas importadas com sucesso, mas {errorCount} apresentaram erros: {string.Join("<br>", errors)}";
                    }
                }

                return RedirectToAction(nameof(Export));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao processar o arquivo CSV: {ex.Message}";
                return RedirectToAction(nameof(Export));
            }
        }

        private static bool ValidateHeader(string[] actual, string[] expected, out List<string> missingColumns)
        {
            missingColumns = [];

            foreach (var column in expected)
            {
                if (!actual.Contains(column, StringComparer.OrdinalIgnoreCase))
                {
                    missingColumns.Add(column);
                }
            }

            return missingColumns.Count == 0;
        }

        private static CreateIncomeModel ParseIncomeLine(string line, string[] headers)
        {
            var values = ParseCsvLine(line);
            var model = new CreateIncomeModel();

            for (int i = 0; i < headers.Length && i < values.Length; i++)
            {
                var header = headers[i].Trim();
                var value = values[i].Trim();

                switch (header.ToLowerInvariant())
                {
                    case "descricao":
                        model.Description = value;
                        break;
                    case "valor":
                        model.Amount = ParseDecimal(value);
                        break;
                    case "datavencimento":
                        model.DueDate = ParseDate(value);
                        break;
                    case "datarecebimento":
                        model.ReceivedDate = string.IsNullOrEmpty(value) ? null : ParseDate(value);
                        break;
                    case "tiporeceitaid":
                        model.IncomeTypeId = value;
                        break;
                    case "recorrente":
                        model.IsRecurring = string.Equals(value, "Sim", StringComparison.OrdinalIgnoreCase);
                        break;
                    case "parcelas":
                        model.NumberOfInstallments = string.IsNullOrEmpty(value) ? 1 : int.Parse(value);
                        break;
                    case "observacoes":
                        model.Notes = string.IsNullOrEmpty(value) ? "" : value;
                        break;
                }
            }

            if (string.IsNullOrEmpty(model.Description))
                throw new ArgumentException("A descrição é obrigatória");

            if (string.IsNullOrEmpty(model.IncomeTypeId))
                throw new ArgumentException("O tipo de receita é obrigatório");

            return model;
        }

        private static string[] ParseCsvLine(string line)
        {
            var result = new List<string>();
            var currentValue = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        currentValue.Append('"');
                        i++; 
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ';' && !inQuotes)
                {
                    result.Add(currentValue.ToString());
                    currentValue.Clear();
                }
                else
                {
                    currentValue.Append(c);
                }
            }

            result.Add(currentValue.ToString());

            return [.. result];
        }

        private static decimal ParseDecimal(string value)
        {
            value = value.Replace(".", "").Replace(",", ".");
            if (!decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            {
                throw new ArgumentException($"Valor inválido: {value}");
            }
            return result;
        }

        private static DateTime ParseDate(string value)
        {
            string[] formats = ["dd/MM/yyyy", "yyyy-MM-dd", "MM/dd/yyyy"];
            if (!DateTime.TryParseExact(value, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
            {
                throw new ArgumentException($"Data inválida: {value}. Use o formato DD/MM/AAAA.");
            }
            return result;
        }

        [RequirePermission("incomes.view")]
        public async Task<IActionResult> ExportIncomeTypes()
        {
            try
            {
                var token = GetToken();
                var incomeTypes = await _incomeTypeService.GetAllIncomeTypesAsync(token);

                var sb = new StringBuilder();
                sb.AppendLine("Id,Nome,Descricao,Sistema");

                foreach (var type in incomeTypes)
                {
                    var name = EscapeCsvField(type.Name);
                    var description = EscapeCsvField(type.Description);

                    sb.AppendLine($"{type.Id},{name},{description},{(type.IsSystem ? "Sim" : "N?o")}");
                }

                string content = sb.ToString();
                byte[] utf8Bytes = Encoding.UTF8.GetBytes(content);
                byte[] latin1Bytes = Encoding.Convert(Encoding.UTF8, Encoding.GetEncoding("ISO-8859-1"), utf8Bytes);
                byte[] resultBytes = Encoding.Convert(Encoding.GetEncoding("ISO-8859-1"), Encoding.UTF8, latin1Bytes);

                byte[] bom = Encoding.UTF8.GetPreamble();
                byte[] finalBytes = new byte[bom.Length + resultBytes.Length];
                Buffer.BlockCopy(bom, 0, finalBytes, 0, bom.Length);
                Buffer.BlockCopy(resultBytes, 0, finalBytes, bom.Length, resultBytes.Length);

                return File(finalBytes, "application/octet-stream", "tipos_receita.csv");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Erro ao exportar tipos de receita: " + ex.Message;
                return RedirectToAction(nameof(Export));
            }
        }

        private static string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return string.Empty;

            if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
            {
                return "\"" + field.Replace("\"", "\"\"") + "\"";
            }

            return field;
        }


        private async Task LoadIncomeTypesForView()
        {
            try
            {
                var token = GetToken();
                var incomeTypes = await _incomeTypeService.GetAllIncomeTypesAsync(token);
                ViewBag.IncomeTypes = incomeTypes;
            }
            catch
            {
                ViewBag.IncomeTypes = new List<object>();
            }
        }
    }
}