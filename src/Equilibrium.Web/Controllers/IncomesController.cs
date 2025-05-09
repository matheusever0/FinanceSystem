using Equilibrium.Resources.Web;
using Equilibrium.Resources.Web.Enums;
using Equilibrium.Resources.Web.Helpers;
using Equilibrium.Web.Extensions;
using Equilibrium.Web.Filters;
using Equilibrium.Web.Models.Filters;
using Equilibrium.Web.Models.Income;
using Equilibrium.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text;

namespace Equilibrium.Web.Controllers
{
    [Authorize]
    [RequirePermission("incomes.view")]
    public class IncomesController : Controller
    {
        private readonly IIncomeService _incomeService;
        private readonly IIncomeTypeService _incomeTypeService;

        public IncomesController(
            IIncomeService incomeService,
            IIncomeTypeService incomeTypeService)
        {
            _incomeService = incomeService ?? throw new ArgumentNullException(nameof(incomeService));
            _incomeTypeService = incomeTypeService ?? throw new ArgumentNullException(nameof(incomeTypeService));
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var incomes = await _incomeService.GetAllIncomesAsync(token);
                return View(incomes);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Incomes, ex);
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Pending()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var incomes = await _incomeService.GetPendingIncomesAsync(token);
                ViewBag.Title = "Receitas Pendentes";
                return View("Index", incomes);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Incomes, ex);
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Overdue()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var incomes = await _incomeService.GetOverdueIncomesAsync(token);
                ViewBag.Title = "Receitas Vencidas";
                return View("Index", incomes);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Incomes, ex);
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Received()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var incomes = await _incomeService.GetReceivedIncomesAsync(token);
                ViewBag.Title = "Receitas Recebidas";
                return View("Index", incomes);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Incomes, ex);
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> ByMonth(int month, int year)
        {
            var currentDate = DateTime.Now;

            if (month <= 0 || month > 12)
            {
                month = currentDate.Month;
                year = currentDate.Year;
            }

            if (year <= 0)
            {
                year = currentDate.Year;
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var incomes = await _incomeService.GetIncomesByMonthAsync(month, year, token);

                ViewBag.Month = month;
                ViewBag.Year = year;
                ViewBag.Title = $"Receitas de {new DateTime(year, month, 1).ToString("MMMM/yyyy")}";

                return View("Index", incomes);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Incomes, ex);
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> ByType(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do tipo de receita não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var incomes = await _incomeService.GetIncomesByTypeAsync(id, token);
                var incomeType = await _incomeTypeService.GetIncomeTypeByIdAsync(id, token);

                if (incomeType == null)
                {
                    return NotFound("Tipo de receita não encontrado");
                }

                ViewBag.Title = $"Receitas por Tipo: {incomeType.Name}";
                ViewBag.TypeId = id;

                return View("Index", incomes);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Incomes, ex);
                return RedirectToAction("Index", "Home");
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
                var token = HttpContext.GetJwtToken();
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
                var token = HttpContext.GetJwtToken();
                var incomeTypes = await _incomeTypeService.GetAllIncomeTypesAsync(token);
                ViewBag.IncomeTypes = incomeTypes;
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
                var token = HttpContext.GetJwtToken();
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
                var token = HttpContext.GetJwtToken();
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
                var token = HttpContext.GetJwtToken();
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

        [RequirePermission("incomes.delete")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID da receita não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var income = await _incomeService.GetIncomeByIdAsync(id, token);

                return income == null ? NotFound("Receita não encontrada") : View(income);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Income, ex);
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
                return BadRequest("ID da receita não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                await _incomeService.DeleteIncomeAsync(id, token);
                TempData["SuccessMessage"] = MessageHelper.GetDeletionSuccessMessage(EntityNames.Income);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetDeletionErrorMessage(EntityNames.Income, ex);
                return RedirectToAction(nameof(Index));
            }
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
                var token = HttpContext.GetJwtToken();
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
                var token = HttpContext.GetJwtToken();
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

        [RequirePermission("incomes.view")]
        public IActionResult Export()
        {
            // This action renders the Export view we just created
            return View();
        }

        [RequirePermission("incomes.create")]
        public IActionResult DownloadTemplate()
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("Descricao,Valor,DataVencimento,DataRecebimento,TipoReceitaId,Recorrente,Parcelas,Observacoes");

                // Add example rows
                sb.AppendLine("Salário,3500.00,25/06/2025,,id-tipo-receita-1,Sim,1,'Salário mensal'");
                sb.AppendLine("Freelance,1200.00,15/06/2025,15/06/2025,id-tipo-receita-2,Não,1,'Projeto de design'");
                sb.AppendLine("Aluguel,1800.00,10/06/2025,,id-tipo-receita-3,Sim,1,'Aluguel do imóvel'");
                sb.AppendLine("Investimentos,350.00,30/06/2025,,id-tipo-receita-4,Não,1,'Rendimentos'");

                // Converter de UTF-8 para ISO-8859-1 (Latin1) e depois de volta para UTF-8
                string content = sb.ToString();
                byte[] utf8Bytes = Encoding.UTF8.GetBytes(content);
                byte[] latin1Bytes = Encoding.Convert(Encoding.UTF8, Encoding.GetEncoding("ISO-8859-1"), utf8Bytes);
                byte[] resultBytes = Encoding.Convert(Encoding.GetEncoding("ISO-8859-1"), Encoding.UTF8, latin1Bytes);

                // Adicionar BOM UTF-8
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
                var token = HttpContext.GetJwtToken();
                var successCount = 0;
                var errorCount = 0;
                var errors = new List<string>();

                using (var reader = new StreamReader(csvFile.OpenReadStream()))
                {
                    // Skip header row
                    var header = await reader.ReadLineAsync();
                    if (header == null)
                    {
                        TempData["ErrorMessage"] = "O arquivo CSV está vazio ou não possui cabeçalho.";
                        return RedirectToAction(nameof(Export));
                    }

                    // Validate header
                    var expectedColumns = new[] { "Descricao", "Valor", "DataVencimento", "DataRecebimento",
                "TipoReceitaId", "Recorrente", "Parcelas", "Observacoes" };
                    var headerColumns = header.Split(';').Select(h => h.Trim()).ToArray();

                    if (!ValidateHeader(headerColumns, expectedColumns, out var missingColumns))
                    {
                        TempData["ErrorMessage"] = $"O cabeçalho do CSV está inválido. Colunas ausentes: {string.Join(", ", missingColumns)}";
                        return RedirectToAction(nameof(Export));
                    }

                    var lineNumber = 1; // Start with 1 for the header

                    while (!reader.EndOfStream)
                    {
                        lineNumber++;
                        var line = await reader.ReadLineAsync();
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        try
                        {
                            var model = ParseIncomeLine(line, headerColumns);

                            // If only validating, we don't create the income
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

        // Helper methods
        private bool ValidateHeader(string[] actual, string[] expected, out List<string> missingColumns)
        {
            missingColumns = new List<string>();

            foreach (var column in expected)
            {
                if (!actual.Contains(column, StringComparer.OrdinalIgnoreCase))
                {
                    missingColumns.Add(column);
                }
            }

            return missingColumns.Count == 0;
        }

        private CreateIncomeModel ParseIncomeLine(string line, string[] headers)
        {
            var values = ParseCsvLine(line);
            var model = new CreateIncomeModel();

            // Map values to model based on header positions
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

            // Validate required fields
            if (string.IsNullOrEmpty(model.Description))
                throw new Exception("A descrição é obrigatória");

            if (string.IsNullOrEmpty(model.IncomeTypeId))
                throw new Exception("O tipo de receita é obrigatório");

            return model;
        }

        private string[] ParseCsvLine(string line)
        {
            // Simple CSV parser that handles quoted values
            var result = new List<string>();
            var currentValue = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    // If we're in quotes and the next character is also a quote, it's an escaped quote
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        currentValue.Append('"');
                        i++; // Skip next quote
                    }
                    else
                    {
                        // Toggle quote status
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ';' && !inQuotes)
                {
                    // End of field
                    result.Add(currentValue.ToString());
                    currentValue.Clear();
                }
                else
                {
                    currentValue.Append(c);
                }
            }

            // Add the last field
            result.Add(currentValue.ToString());

            return result.ToArray();
        }

        private decimal ParseDecimal(string value)
        {
            // Handle both comma and dot as decimal separators
            value = value.Replace(".", "").Replace(",", ".");
            if (!decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            {
                throw new Exception($"Valor inválido: {value}");
            }
            return result;
        }

        private DateTime ParseDate(string value)
        {
            // Try different date formats
            string[] formats = { "dd/MM/yyyy", "yyyy-MM-dd", "MM/dd/yyyy" };
            if (!DateTime.TryParseExact(value, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
            {
                throw new Exception($"Data inválida: {value}. Use o formato DD/MM/AAAA.");
            }
            return result;
        }

        [RequirePermission("incomes.view")]
        public async Task<IActionResult> ExportIncomeTypes()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var incomeTypes = await _incomeTypeService.GetAllIncomeTypesAsync(token);

                var sb = new StringBuilder();
                sb.AppendLine("Id,Nome,Descricao,Sistema");

                foreach (var type in incomeTypes)
                {
                    // Escape quotes in text fields
                    var name = EscapeCsvField(type.Name);
                    var description = EscapeCsvField(type.Description);

                    sb.AppendLine($"{type.Id},{name},{description},{(type.IsSystem ? "Sim" : "Não")}");
                }

                // Converter de UTF-8 para ISO-8859-1 (Latin1) e depois de volta para UTF-8
                string content = sb.ToString();
                byte[] utf8Bytes = Encoding.UTF8.GetBytes(content);
                byte[] latin1Bytes = Encoding.Convert(Encoding.UTF8, Encoding.GetEncoding("ISO-8859-1"), utf8Bytes);
                byte[] resultBytes = Encoding.Convert(Encoding.GetEncoding("ISO-8859-1"), Encoding.UTF8, latin1Bytes);

                // Adicionar BOM UTF-8
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

        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return string.Empty;

            // Se contém vírgula, aspas ou quebra de linha, coloca entre aspas e dobra as aspas internas
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
            {
                return "\"" + field.Replace("\"", "\"\"") + "\"";
            }

            return field;
        }


        private async Task LoadIncomeTypesForView()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var incomeTypes = await _incomeTypeService.GetAllIncomeTypesAsync(token);
                ViewBag.IncomeTypes = incomeTypes;
            }
            catch
            {
                ViewBag.IncomeTypes = new List<object>();
            }
        }

        [HttpGet("filter")]
        [RequirePermission("incomes.view")]
        public async Task<IActionResult> Filter(IncomeFilter filter = null)
        {
            if (filter == null)
                filter = new IncomeFilter();

            try
            {
                var token = HttpContext.GetJwtToken();
                var result = await _incomeService.GetFilteredAsync(filter, token);

                // Add pagination headers
                Response.Headers.Add("X-Pagination-Total", result.TotalCount.ToString());
                Response.Headers.Add("X-Pagination-Pages", result.TotalPages.ToString());
                Response.Headers.Add("X-Pagination-Page", result.PageNumber.ToString());
                Response.Headers.Add("X-Pagination-Size", result.PageSize.ToString());

                ViewBag.Filter = filter;
                ViewBag.TotalCount = result.TotalCount;
                ViewBag.TotalPages = result.TotalPages;
                ViewBag.CurrentPage = result.PageNumber;
                ViewBag.PageSize = result.PageSize;

                return View("Index", result.Items);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Incomes, ex);
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet("api/filter")]
        [RequirePermission("incomes.view")]
        public async Task<IActionResult> FilterJson([FromQuery] IncomeFilter filter)
        {
            if (filter == null)
                filter = new IncomeFilter();

            try
            {
                var token = HttpContext.GetJwtToken();
                var result = await _incomeService.GetFilteredAsync(filter, token);

                return Json(new
                {
                    items = result.Items,
                    totalCount = result.TotalCount,
                    pageNumber = result.PageNumber,
                    pageSize = result.PageSize,
                    totalPages = result.TotalPages,
                    hasPreviousPage = result.HasPreviousPage,
                    hasNextPage = result.HasNextPage
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
