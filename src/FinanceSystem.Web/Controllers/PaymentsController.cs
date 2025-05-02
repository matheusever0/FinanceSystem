using FinanceSystem.Resources.Web;
using FinanceSystem.Resources.Web.Enums;
using FinanceSystem.Resources.Web.Helpers;
using FinanceSystem.Web.Extensions;
using FinanceSystem.Web.Filters;
using FinanceSystem.Web.Interfaces;
using FinanceSystem.Web.Models.Payment;
using FinanceSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text;

namespace FinanceSystem.Web.Controllers
{
    [Authorize]
    [RequirePermission("payments.view")]
    public class PaymentsController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IPaymentTypeService _paymentTypeService;
        private readonly IPaymentMethodService _paymentMethodService;
        private readonly ICreditCardService _creditCardService;
        private readonly IFinancingService _financingService;

        public PaymentsController(
            IPaymentService paymentService,
            IPaymentTypeService paymentTypeService,
            IPaymentMethodService paymentMethodService,
            ICreditCardService creditCardService,
            IFinancingService financingService)
        {
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _paymentTypeService = paymentTypeService ?? throw new ArgumentNullException(nameof(paymentTypeService));
            _paymentMethodService = paymentMethodService ?? throw new ArgumentNullException(nameof(paymentMethodService));
            _creditCardService = creditCardService ?? throw new ArgumentNullException(nameof(creditCardService));
            _financingService = financingService ?? throw new ArgumentNullException(nameof(financingService));
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var payments = await _paymentService.GetAllPaymentsAsync(token);
                return View(payments);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Payments, ex);
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Pending()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var pendingPayments = await _paymentService.GetPendingPaymentsAsync(token);
                ViewBag.Title = "Pagamentos Pendentes";
                return View("Index", pendingPayments);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Payments, ex);
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Overdue()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var overduePayments = await _paymentService.GetOverduePaymentsAsync(token);
                ViewBag.Title = "Pagamentos Vencidos";
                return View("Index", overduePayments);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Payments, ex);
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
                var monthlyPayments = await _paymentService.GetPaymentsByMonthAsync(month, year, token);

                ViewBag.Month = month;
                ViewBag.Year = year;
                ViewBag.Title = $"Pagamentos de {new DateTime(year, month, 1).ToString("MMMM/yyyy")}";

                return View("Index", monthlyPayments);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Payments, ex);
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> ByType(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do tipo de pagamento não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var payments = await _paymentService.GetPaymentsByTypeAsync(id, token);
                var paymentType = await _paymentTypeService.GetPaymentTypeByIdAsync(id, token);

                if (paymentType == null)
                {
                    return NotFound("Tipo de pagamento não encontrado");
                }

                ViewBag.Title = $"Pagamentos do Tipo: {paymentType.Name}";
                ViewBag.TypeId = id;

                return View("Index", payments);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Payments, ex);
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> ByMethod(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do método de pagamento não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var payments = await _paymentService.GetPaymentsByMethodAsync(id, token);
                var paymentMethod = await _paymentMethodService.GetPaymentMethodByIdAsync(id, token);

                if (paymentMethod == null)
                {
                    return NotFound("Método de pagamento não encontrado");
                }

                ViewBag.Title = $"Pagamentos por Método: {paymentMethod.Name}";
                ViewBag.MethodId = id;

                return View("Index", payments);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Payments, ex);
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do pagamento não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var payment = await _paymentService.GetPaymentByIdAsync(id, token);

                return payment == null ? NotFound("Pagamento não encontrado") : View(payment);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Payment, ex);
                return RedirectToAction(nameof(Index));
            }
        }

        [RequirePermission("payments.create")]
        public async Task<IActionResult> Create()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var paymentTypes = await _paymentTypeService.GetAllPaymentTypesAsync(token);
                var paymentMethods = await _paymentMethodService.GetAllPaymentMethodsAsync(token);
                var creditCards = await _creditCardService.GetAllCreditCardsAsync(token);
                var financings = await _financingService.GetActiveFinancingsAsync(token);

                ViewBag.PaymentTypes = paymentTypes;
                ViewBag.PaymentMethods = paymentMethods;
                ViewBag.CreditCards = creditCards;
                ViewBag.Financings = financings;

                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Erro ao preparar o formulário.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("payments.create")]
        public async Task<IActionResult> Create(CreatePaymentModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadFormDependencies();
                return View(model);
            }

            try
            {
                var token = HttpContext.GetJwtToken();

                // Verificar se o tipo de pagamento é de financiamento
                var paymentType = await _paymentTypeService.GetPaymentTypeByIdAsync(model.PaymentTypeId, token);
                if (paymentType != null && paymentType.IsFinancingType && string.IsNullOrEmpty(model.FinancingId))
                {
                    ModelState.AddModelError("FinancingId", "Um financiamento deve ser selecionado para este tipo de pagamento.");
                    await LoadFormDependencies();
                    return View(model);
                }

                // Verificar se o método de pagamento é cartão de crédito e se foi selecionado um cartão
                var paymentMethod = await _paymentMethodService.GetPaymentMethodByIdAsync(model.PaymentMethodId, token);
                if (paymentMethod != null && paymentMethod.Type == 2 && string.IsNullOrEmpty(model.CreditCardId))
                {
                    ModelState.AddModelError("CreditCardId", ResourceFinanceWeb.Error_CreditCardRequired);
                    await LoadFormDependencies();
                    return View(model);
                }

                var payment = await _paymentService.CreatePaymentAsync(model, token);
                TempData["SuccessMessage"] = MessageHelper.GetCreationSuccessMessage(EntityNames.Payment);
                return RedirectToAction(nameof(Details), new { id = payment.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, MessageHelper.GetCreationErrorMessage(EntityNames.Payment, ex));
                await LoadFormDependencies();
                return View(model);
            }
        }

        [RequirePermission("payments.create")]
        public async Task<IActionResult> CreateWithFinancing()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var paymentTypes = await _paymentTypeService.GetAllPaymentTypesAsync(token);

                // Filtrar tipos de pagamento que são marcados como tipo de financiamento
                var financingPaymentTypes = paymentTypes.Where(pt => pt.IsFinancingType).ToList();

                if (!financingPaymentTypes.Any())
                {
                    TempData["WarningMessage"] = "Não há tipos de pagamento configurados para financiamento.";
                    return RedirectToAction(nameof(Create));
                }

                var paymentMethods = await _paymentMethodService.GetAllPaymentMethodsAsync(token);
                var financings = await _financingService.GetActiveFinancingsAsync(token);

                ViewBag.PaymentTypes = financingPaymentTypes;
                ViewBag.PaymentMethods = paymentMethods;
                ViewBag.Financings = financings;
                ViewBag.IsFinancingPayment = true;

                return View("Create");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Payment, ex);
                return RedirectToAction(nameof(Index));
            }
        }

        [RequirePermission("payments.edit")]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do pagamento não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var payment = await _paymentService.GetPaymentByIdAsync(id, token);

                if (payment == null)
                {
                    return NotFound("Pagamento não encontrado");
                }

                var paymentTypes = await _paymentTypeService.GetAllPaymentTypesAsync(token);
                var paymentMethods = await _paymentMethodService.GetAllPaymentMethodsAsync(token);
                var creditCards = await _creditCardService.GetAllCreditCardsAsync(token);

                ViewBag.PaymentTypes = paymentTypes;
                ViewBag.PaymentMethods = paymentMethods;
                ViewBag.CreditCards = creditCards;
                ViewBag.CreditCardPaymentMethod = paymentMethods.FirstOrDefault(pm => pm.Type == 2)?.Id;

                var model = new UpdatePaymentModel
                {
                    Description = payment.Description,
                    Amount = payment.Amount,
                    DueDate = payment.DueDate,
                    PaymentDate = payment.PaymentDate,
                    Status = payment.Status,
                    IsRecurring = payment.IsRecurring,
                    Notes = payment.Notes,
                    PaymentTypeId = payment.PaymentTypeId,
                    PaymentMethodId = payment.PaymentMethodId
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Payment, ex);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("payments.edit")]
        public async Task<IActionResult> Edit(string id, UpdatePaymentModel model)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do pagamento não fornecido");
            }

            if (!ModelState.IsValid)
            {
                await LoadFormDependencies();
                return View(model);
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                await _paymentService.UpdatePaymentAsync(id, model, token);
                TempData["SuccessMessage"] = MessageHelper.GetUpdateSuccessMessage(EntityNames.Payment);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, MessageHelper.GetUpdateErrorMessage(EntityNames.Payment, ex));
                await LoadFormDependencies();
                return View(model);
            }
        }

        [RequirePermission("payments.delete")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do pagamento não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var payment = await _paymentService.GetPaymentByIdAsync(id, token);

                return payment == null ? NotFound("Pagamento não encontrado") : View(payment);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Payment, ex);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [RequirePermission("payments.delete")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do pagamento não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                await _paymentService.DeletePaymentAsync(id, token);
                TempData["SuccessMessage"] = MessageHelper.GetDeletionSuccessMessage(EntityNames.Payment);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetDeletionErrorMessage(EntityNames.Payment, ex);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("payments.edit")]
        public async Task<IActionResult> MarkAsPaid(string id, DateTime? paymentDate = null)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do pagamento não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                await _paymentService.MarkAsPaidAsync(id, paymentDate ?? DateTime.Now, token);
                TempData["SuccessMessage"] = MessageHelper.GetStatusChangeSuccessMessage(EntityNames.Payment, EntityStatus.Paid);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetStatusChangeErrorMessage(EntityNames.Payment, EntityStatus.Paid, ex);
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("payments.edit")]
        public async Task<IActionResult> MarkAsOverdue(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do pagamento não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                await _paymentService.MarkAsOverdueAsync(id, token);
                TempData["SuccessMessage"] = MessageHelper.GetStatusChangeSuccessMessage(EntityNames.Payment, EntityStatus.Overdue);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetStatusChangeErrorMessage(EntityNames.Payment, EntityStatus.Overdue, ex);
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("payments.edit")]
        public async Task<IActionResult> Cancel(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("ID do pagamento não fornecido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                await _paymentService.CancelPaymentAsync(id, token);
                TempData["SuccessMessage"] = MessageHelper.GetCancelSuccessMessage(EntityNames.Payment);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetCancelErrorMessage(EntityNames.Payment, ex);
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [RequirePermission("payments.create")]
        public IActionResult Export()
        {
            // This action renders the Export view we just created
            return View();
        }

        [RequirePermission("payments.create")]
        public IActionResult DownloadTemplate()
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("Descricao,Valor,DataVencimento,DataPagamento,TipoPagamentoId,MetodoPagamentoId,Recorrente,Parcelas,CartaoCreditoId,FinanciamentoId,ParcelaFinanciamentoId,Observacoes");

                // Add example rows
                sb.AppendLine("Aluguel,1500.00,05/06/2025,,id-tipo-pagamento-1,id-metodo-pagamento-1,Sim,1,,,,'Pagamento mensal de aluguel'");
                sb.AppendLine("Compra Supermercado,253.45,10/06/2025,10/06/2025,id-tipo-pagamento-2,id-metodo-pagamento-2,Não,1,,,,'Compras do mês'");
                sb.AppendLine("Parcela Carro,850.00,15/06/2025,,id-tipo-pagamento-3,id-metodo-pagamento-3,Não,1,id-cartao-1,,,'Parcela 10/48'");
                sb.AppendLine("Prestação Apartamento,2500.00,20/06/2025,,id-tipo-pagamento-4,id-metodo-pagamento-4,Não,1,,id-financiamento-1,id-parcela-financiamento-1,'Prestação 24/360'");

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

                return File(finalBytes, "application/octet-stream", "modelo_pagamentos.csv");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Erro ao gerar o modelo de CSV: " + ex.Message;
                return RedirectToAction(nameof(Export));
            }
        }

        [HttpPost]
        [RequirePermission("payments.create")]
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
                    var expectedColumns = new[] { "Descricao", "Valor", "DataVencimento", "DataPagamento",
                "TipoPagamentoId", "MetodoPagamentoId", "Recorrente", "Parcelas",
                "CartaoCreditoId", "FinanciamentoId", "Observacoes" };
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
                            var model = ParsePaymentLine(line, headerColumns);

                            // If only validating, we don't create the payment
                            if (!validateOnly)
                            {
                                await _paymentService.CreatePaymentAsync(model, token);
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
                        TempData["SuccessMessage"] = $"Validação concluída com sucesso. {successCount} pagamentos estão prontos para importação.";
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
                        TempData["SuccessMessage"] = $"{successCount} pagamentos foram importados com sucesso.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = $"{successCount} pagamentos importados com sucesso, mas {errorCount} apresentaram erros: {string.Join("<br>", errors)}";
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

        private CreatePaymentModel ParsePaymentLine(string line, string[] headers)
        {
            var values = ParseCsvLine(line);
            var model = new CreatePaymentModel();

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
                    case "datapagamento":
                        model.PaymentDate = string.IsNullOrEmpty(value) ? null : ParseDate(value);
                        break;
                    case "tipopagamentoid":
                        model.PaymentTypeId = value;
                        break;
                    case "metodopagamentoid":
                        model.PaymentMethodId = value;
                        break;
                    case "recorrente":
                        model.IsRecurring = string.Equals(value, "Sim", StringComparison.OrdinalIgnoreCase);
                        break;
                    case "parcelas":
                        model.NumberOfInstallments = string.IsNullOrEmpty(value) ? 1 : int.Parse(value);
                        break;
                    case "cartaocreditoid":
                        model.CreditCardId = string.IsNullOrEmpty(value) ? null : value;
                        break;
                    case "financiamentoid":
                        model.FinancingId = string.IsNullOrEmpty(value) ? null : value;
                        break;
                    case "parcelafinanciamentoid":
                        model.FinancingInstallmentId = string.IsNullOrEmpty(value) ? null : value;
                        break;
                    case "observacoes":
                        model.Notes = string.IsNullOrEmpty(value) ? "" : value;
                        break;
                }
            }

            // Validate required fields
            if (string.IsNullOrEmpty(model.Description))
                throw new Exception("A descrição é obrigatória");

            if (string.IsNullOrEmpty(model.PaymentTypeId))
                throw new Exception("O tipo de pagamento é obrigatório");

            if (string.IsNullOrEmpty(model.PaymentMethodId))
                throw new Exception("O método de pagamento é obrigatório");

            // Validar se tem financiamento mas não tem parcela
            if (!string.IsNullOrEmpty(model.FinancingId) && string.IsNullOrEmpty(model.FinancingInstallmentId))
                throw new Exception("Quando um financiamento é especificado, a parcela do financiamento é obrigatória");

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

        [RequirePermission("payments.view")]
        public async Task<IActionResult> ExportPaymentTypes()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var paymentTypes = await _paymentTypeService.GetAllPaymentTypesAsync(token);

                var sb = new StringBuilder();
                sb.AppendLine("Id,Nome,Descricao,TipoFinanciamento");

                foreach (var type in paymentTypes)
                {
                    // Escape quotes in text fields
                    var name = EscapeCsvField(type.Name);
                    var description = EscapeCsvField(type.Description);

                    sb.AppendLine($"{type.Id},{name},{description},{(type.IsFinancingType ? "Sim" : "Não")}");
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

                return File(finalBytes, "application/octet-stream", "tipos_pagamento.csv");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Erro ao exportar tipos de pagamento: " + ex.Message;
                return RedirectToAction(nameof(Export));
            }
        }

        [RequirePermission("payments.view")]
        public async Task<IActionResult> ExportPaymentMethods()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var paymentMethods = await _paymentMethodService.GetAllPaymentMethodsAsync(token);

                var sb = new StringBuilder();
                sb.AppendLine("Id,Nome,Descricao");

                foreach (var method in paymentMethods)
                {

                    // Escape quotes in text fields
                    var name = EscapeCsvField(method.Name);
                    var description = EscapeCsvField(method.Description);

                    sb.AppendLine($"{method.Id},{name},{description}");
                }

                // Converter de UTF-8 para ISO-8859-1 (Latin1) e depois de volta para UTF-8
                // Isso às vezes resolve problemas de codificação
                string content = sb.ToString();
                byte[] utf8Bytes = Encoding.UTF8.GetBytes(content);
                byte[] latin1Bytes = Encoding.Convert(Encoding.UTF8, Encoding.GetEncoding("ISO-8859-1"), utf8Bytes);
                byte[] resultBytes = Encoding.Convert(Encoding.GetEncoding("ISO-8859-1"), Encoding.UTF8, latin1Bytes);

                // Adicionar BOM UTF-8
                byte[] bom = Encoding.UTF8.GetPreamble();
                byte[] finalBytes = new byte[bom.Length + resultBytes.Length];
                Buffer.BlockCopy(bom, 0, finalBytes, 0, bom.Length);
                Buffer.BlockCopy(resultBytes, 0, finalBytes, bom.Length, resultBytes.Length);

                return File(finalBytes, "application/octet-stream", "metodos_pagamento.csv");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Erro ao exportar métodos de pagamento: " + ex.Message;
                return RedirectToAction(nameof(Export));
            }
        }

        [RequirePermission("payments.view")]
        public async Task<IActionResult> ExportCreditCards()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var creditCards = await _creditCardService.GetAllCreditCardsAsync(token);

                var sb = new StringBuilder();
                sb.AppendLine("Id,Nome,Bandeira,UltimosDigitos");

                foreach (var card in creditCards)
                {
                    // Escape quotes in text fields
                    var name = EscapeCsvField(card.Name);
                    var brand = EscapeCsvField(card.CardBrand);

                    sb.AppendLine($"{card.Id},{name},{brand},{card.LastFourDigits}");
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

                return File(finalBytes, "application/octet-stream", "cartoes_credito.csv");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Erro ao exportar cartões de crédito: " + ex.Message;
                return RedirectToAction(nameof(Export));
            }
        }

        [RequirePermission("payments.view")]
        public async Task<IActionResult> ExportFinancings()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var financings = await _financingService.GetActiveFinancingsAsync(token);

                var sb = new StringBuilder();
                sb.AppendLine("Id,Descricao,ValorTotal,DataInicio,Status");

                foreach (var financing in financings)
                {
                    // Escape quotes in text fields
                    var description = EscapeCsvField(financing.Description);
                    var status = EscapeCsvField(financing.StatusDescription);

                    sb.AppendLine($"{financing.Id},{description},{financing.TotalAmount.ToString("F2", CultureInfo.InvariantCulture)},{financing.StartDate:dd/MM/yyyy},{status}");
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

                return File(finalBytes, "application/octet-stream", "financiamentos.csv");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Erro ao exportar financiamentos: " + ex.Message;
                return RedirectToAction(nameof(Export));
            }
        }

        [RequirePermission("payments.view")]
        public async Task<IActionResult> ExportFinancingInstallments()
        {
            try
            {
                var token = HttpContext.GetJwtToken();

                var sb = new StringBuilder();
                sb.AppendLine("Id,FinanciamentoId,DescricaoFinanciamento,NumeroParcela,Valor,DataVencimento,Status");

                // Obter os financiamentos ativos
                var financings = await _financingService.GetActiveFinancingsAsync(token);

                foreach (var financing in financings)
                {
                    // Para cada financiamento, obter suas parcelas
                    var installments = await _financingService.GetFinancingInstallmentsAsync(financing.Id, token);

                    if (installments != null && installments.Any())
                    {
                        // Escape quotes in text fields
                        var financingDescription = EscapeCsvField(financing.Description);

                        foreach (var installment in installments)
                        {
                            var statusDesc = EscapeCsvField(installment.StatusDescription);

                            sb.AppendLine($"{installment.Id},{financing.Id},{financingDescription},{installment.InstallmentNumber},{installment.TotalAmount.ToString("F2", CultureInfo.InvariantCulture)},{installment.DueDate:dd/MM/yyyy},{statusDesc}");
                        }
                    }
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

                return File(finalBytes, "application/octet-stream", "parcelas_financiamentos.csv");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Erro ao exportar parcelas de financiamentos: " + ex.Message;
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

        private async Task LoadFormDependencies(bool includeFinancings = true)
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var paymentTypes = await _paymentTypeService.GetAllPaymentTypesAsync(token);
                var paymentMethods = await _paymentMethodService.GetAllPaymentMethodsAsync(token);
                var creditCards = await _creditCardService.GetAllCreditCardsAsync(token);
                var financings = await _financingService.GetActiveFinancingsAsync(token);

                ViewBag.PaymentTypes = paymentTypes;
                ViewBag.PaymentMethods = paymentMethods;
                ViewBag.CreditCards = creditCards;
                ViewBag.Financings = financings;
                ViewBag.CreditCardPaymentMethod = paymentMethods.FirstOrDefault(pm => pm.Type == 2)?.Id;
            }
            catch
            {
                ViewBag.PaymentTypes = new List<object>();
                ViewBag.PaymentMethods = new List<object>();
                ViewBag.CreditCards = new List<object>();
                ViewBag.Financings = new List<object>();
            }
        }
    }
}