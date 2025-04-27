using FinanceSystem.Resources.Web.Enums;
using FinanceSystem.Resources.Web.Helpers;
using FinanceSystem.Web.Extensions;
using FinanceSystem.Web.Filters;
using FinanceSystem.Web.Interfaces;
using FinanceSystem.Web.Models.Generics;
using FinanceSystem.Web.Models.Payment;
using FinanceSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace FinanceSystem.Web.Controllers
{
    [Authorize]
    [RequirePermission("reports.view")]
    public class ReportsController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IIncomeService _incomeService;
        private readonly IPaymentTypeService _paymentTypeService;
        private readonly IIncomeTypeService _incomeTypeService;
        private readonly ICreditCardService _creditCardService;
        private readonly IInvestmentService _investmentService;

        private const int STATUS_PAGO = 2;
        private const int STATUS_RECEBIDO = 2;

        public ReportsController(
            IPaymentService paymentService,
            IIncomeService incomeService,
            IPaymentTypeService paymentTypeService,
            IIncomeTypeService incomeTypeService,
            ICreditCardService creditCardService,
            IInvestmentService investmentService)
        {
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _incomeService = incomeService ?? throw new ArgumentNullException(nameof(incomeService));
            _paymentTypeService = paymentTypeService ?? throw new ArgumentNullException(nameof(paymentTypeService));
            _incomeTypeService = incomeTypeService ?? throw new ArgumentNullException(nameof(incomeTypeService));
            _creditCardService = creditCardService ?? throw new ArgumentNullException(nameof(creditCardService));
            _investmentService = investmentService ?? throw new ArgumentNullException(nameof(investmentService));
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Monthly(int month, int year)
        {
            var currentDate = DateTime.Now;
            if (month <= 0 || month > 12)
            {
                month = currentDate.Month;
            }

            if (year <= 0)
            {
                year = currentDate.Year;
            }

            try
            {
                var token = HttpContext.GetJwtToken();

                var payments = await _paymentService.GetPaymentsByMonthAsync(month, year, token);
                var incomes = await _incomeService.GetIncomesByMonthAsync(month, year, token);
                var paymentTypes = await _paymentTypeService.GetAllPaymentTypesAsync(token);
                var incomeTypes = await _incomeTypeService.GetAllIncomeTypesAsync(token);

                ViewBag.Month = month;
                ViewBag.Year = year;
                ViewBag.MonthName = new DateTime(year, month, 1).ToString("MMMM");

                ViewBag.Payments = payments;
                ViewBag.Incomes = incomes;

                decimal totalPayments = payments.Where(p => p.Status == STATUS_PAGO).Sum(p => p.Amount);
                decimal totalIncomes = incomes.Where(i => i.Status == STATUS_RECEBIDO).Sum(i => i.Amount);
                decimal balance = totalIncomes - totalPayments;

                ViewBag.TotalPayments = totalPayments;
                ViewBag.TotalIncomes = totalIncomes;
                ViewBag.Balance = balance;

                // Preparar dados para gráfico de pagamentos por tipo
                var paymentsByType = payments
                    .Where(p => p.Status == STATUS_PAGO)
                    .GroupBy(p => p.PaymentTypeId)
                    .Select(g => new PaymentByTypeDto
                    {
                        TypeId = g.Key,
                        TypeName = paymentTypes.FirstOrDefault(pt => pt.Id == g.Key)?.Name ?? "Desconhecido",
                        TotalAmount = g.Sum(p => p.Amount)
                    })
                    .OrderByDescending(p => p.TotalAmount)
                    .ToList();

                ViewBag.PaymentsByType = paymentsByType;
                ViewBag.PaymentsByTypeLabels = JsonSerializer.Serialize(paymentsByType.Select(p => p.TypeName));
                ViewBag.PaymentsByTypeValues = JsonSerializer.Serialize(paymentsByType.Select(p => p.TotalAmount));

                // Preparar dados para gráfico de receitas por tipo
                var incomesByType = incomes
                    .Where(i => i.Status == STATUS_RECEBIDO)
                    .GroupBy(i => i.IncomeTypeId)
                    .Select(g => new
                    {
                        TypeId = g.Key,
                        TypeName = incomeTypes.FirstOrDefault(it => it.Id == g.Key)?.Name ?? "Desconhecido",
                        TotalAmount = g.Sum(i => i.Amount)
                    })
                    .OrderByDescending(i => i.TotalAmount)
                    .ToList();

                ViewBag.IncomesByTypeLabels = JsonSerializer.Serialize(incomesByType.Select(i => i.TypeName));
                ViewBag.IncomesByTypeValues = JsonSerializer.Serialize(incomesByType.Select(i => i.TotalAmount));

                // Dados para gráfico de balanço
                var months = Enumerable.Range(0, 6)
                    .Select(i => currentDate.AddMonths(-i))
                    .OrderBy(d => d.Year)
                    .ThenBy(d => d.Month)
                    .ToList();

                var monthlyData = await GetMonthlyComparisonDataAsync(token, months);
                ViewBag.MonthlyLabels = JsonSerializer.Serialize(monthlyData.Select(m => m.Month));
                ViewBag.MonthlyIncomeValues = JsonSerializer.Serialize(monthlyData.Select(m => m.IncomeAmount));
                ViewBag.MonthlyPaymentValues = JsonSerializer.Serialize(monthlyData.Select(m => m.PaymentAmount));
                ViewBag.MonthlyBalanceValues = JsonSerializer.Serialize(
                    monthlyData.Select(m => m.IncomeAmount - m.PaymentAmount));

                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetReportGenerationErrorMessage(ReportType.Monthly, ex);
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Annual(int year)
        {
            if (year <= 0)
            {
                year = DateTime.Now.Year;
            }

            try
            {
                var token = HttpContext.GetJwtToken();

                ViewBag.Year = year;

                var monthlyData = new List<MonthlyComparisonData>();
                decimal totalPayments = 0;
                decimal totalIncomes = 0;

                // Obter dados mensais
                for (int month = 1; month <= 12; month++)
                {
                    var payments = await _paymentService.GetPaymentsByMonthAsync(month, year, token);
                    var incomes = await _incomeService.GetIncomesByMonthAsync(month, year, token);

                    var monthPayments = payments.Where(p => p.Status == STATUS_PAGO).Sum(p => p.Amount);
                    var monthIncomes = incomes.Where(i => i.Status == STATUS_RECEBIDO).Sum(i => i.Amount);

                    totalPayments += monthPayments;
                    totalIncomes += monthIncomes;

                    monthlyData.Add(new MonthlyComparisonData
                    {
                        Month = new DateTime(year, month, 1).ToString("MMM"),
                        PaymentAmount = monthPayments,
                        IncomeAmount = monthIncomes
                    });
                }

                ViewBag.MonthlyData = monthlyData;
                ViewBag.TotalPayments = totalPayments;
                ViewBag.TotalIncomes = totalIncomes;
                ViewBag.Balance = totalIncomes - totalPayments;

                ViewBag.MonthlyLabels = JsonSerializer.Serialize(monthlyData.Select(m => m.Month));
                ViewBag.MonthlyIncomeValues = JsonSerializer.Serialize(monthlyData.Select(m => m.IncomeAmount));
                ViewBag.MonthlyPaymentValues = JsonSerializer.Serialize(monthlyData.Select(m => m.PaymentAmount));
                ViewBag.MonthlyBalanceValues = JsonSerializer.Serialize(
                    monthlyData.Select(m => m.IncomeAmount - m.PaymentAmount));

                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetReportGenerationErrorMessage(ReportType.Annual, ex);
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> CreditCards()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var creditCards = await _creditCardService.GetAllCreditCardsAsync(token);

                ViewBag.CreditCards = creditCards;

                var labels = creditCards.Select(c => c.Name).ToList();
                var usedLimits = creditCards.Select(c => c.UsedLimit).ToList();
                var availableLimits = creditCards.Select(c => c.AvailableLimit).ToList();

                ViewBag.CreditCardLabels = JsonSerializer.Serialize(labels);
                ViewBag.CreditCardUsedLimits = JsonSerializer.Serialize(usedLimits);
                ViewBag.CreditCardAvailableLimits = JsonSerializer.Serialize(availableLimits);

                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetReportGenerationErrorMessage(ReportType.CreditCards, ex);
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Investments()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var investments = await _investmentService.GetAllInvestmentsAsync(token);

                ViewBag.Investments = investments;
                ViewBag.TotalInvested = investments.Sum(i => i.TotalInvested);
                ViewBag.CurrentValue = investments.Sum(i => i.CurrentTotal);
                ViewBag.GainLoss = investments.Sum(i => i.GainLossValue);

                var investmentLabels = investments.Select(i => i.Name).ToList();
                var investmentValues = investments.Select(i => i.CurrentTotal).ToList();

                ViewBag.InvestmentLabels = JsonSerializer.Serialize(investmentLabels);
                ViewBag.InvestmentValues = JsonSerializer.Serialize(investmentValues);

                // Agrupar por tipo
                var investmentsByType = investments
                    .GroupBy(i => i.Type)
                    .Select(g => new
                    {
                        Type = g.Key,
                        TypeName = GetInvestmentTypeName(g.Key),
                        TotalValue = g.Sum(i => i.CurrentTotal)
                    })
                    .OrderByDescending(i => i.TotalValue)
                    .ToList();

                ViewBag.InvestmentTypeLabels = JsonSerializer.Serialize(investmentsByType.Select(i => i.TypeName));
                ViewBag.InvestmentTypeValues = JsonSerializer.Serialize(investmentsByType.Select(i => i.TotalValue));

                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Erro ao gerar relatório de investimentos: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Print(int month, int year)
        {
            var currentDate = DateTime.Now;
            if (month <= 0 || month > 12)
            {
                month = currentDate.Month;
            }

            if (year <= 0)
            {
                year = currentDate.Year;
            }

            try
            {
                var token = HttpContext.GetJwtToken();

                var payments = await _paymentService.GetPaymentsByMonthAsync(month, year, token);
                var incomes = await _incomeService.GetIncomesByMonthAsync(month, year, token);
                var paymentTypes = await _paymentTypeService.GetAllPaymentTypesAsync(token);
                var incomeTypes = await _incomeTypeService.GetAllIncomeTypesAsync(token);
                var creditCards = await _creditCardService.GetAllCreditCardsAsync(token);

                ViewBag.Month = month;
                ViewBag.Year = year;
                ViewBag.MonthName = new DateTime(year, month, 1).ToString("MMMM");
                ViewBag.DateGenerated = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

                ViewBag.Payments = payments;
                ViewBag.Incomes = incomes;
                ViewBag.CreditCards = creditCards;

                // Totais
                ViewBag.TotalPayments = payments.Where(p => p.Status == STATUS_PAGO).Sum(p => p.Amount);
                ViewBag.TotalIncomes = incomes.Where(i => i.Status == STATUS_RECEBIDO).Sum(i => i.Amount);
                ViewBag.TotalPendingPayments = payments.Where(p => p.Status == 1).Sum(p => p.Amount);
                ViewBag.TotalPendingIncomes = incomes.Where(i => i.Status == 1).Sum(i => i.Amount);
                ViewBag.Balance = ViewBag.TotalIncomes - ViewBag.TotalPayments;

                // Pagamentos por tipo
                var paymentsByType = payments
                    .Where(p => p.Status == STATUS_PAGO)
                    .GroupBy(p => p.PaymentTypeId)
                    .Select(g => new
                    {
                        TypeId = g.Key,
                        TypeName = paymentTypes.FirstOrDefault(pt => pt.Id == g.Key)?.Name ?? "Desconhecido",
                        TotalAmount = g.Sum(p => p.Amount)
                    })
                    .OrderByDescending(p => p.TotalAmount)
                    .ToList();

                ViewBag.PaymentsByType = paymentsByType;

                // Receitas por tipo
                var incomesByType = incomes
                    .Where(i => i.Status == STATUS_RECEBIDO)
                    .GroupBy(i => i.IncomeTypeId)
                    .Select(g => new
                    {
                        TypeId = g.Key,
                        TypeName = incomeTypes.FirstOrDefault(it => it.Id == g.Key)?.Name ?? "Desconhecido",
                        TotalAmount = g.Sum(i => i.Amount)
                    })
                    .OrderByDescending(i => i.TotalAmount)
                    .ToList();

                ViewBag.IncomesByType = incomesByType;

                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetReportGenerationErrorMessage(ReportType.Print, ex);
                return RedirectToAction("Index", "Home");
            }
        }

        private async Task<List<MonthlyComparisonData>> GetMonthlyComparisonDataAsync(string token, List<DateTime> months)
        {
            var result = new List<MonthlyComparisonData>();

            foreach (var date in months)
            {
                var month = date.Month;
                var year = date.Year;
                var monthName = date.ToString("MMM/yy");

                try
                {
                    var payments = await _paymentService.GetPaymentsByMonthAsync(month, year, token);
                    var incomes = await _incomeService.GetIncomesByMonthAsync(month, year, token);

                    var monthPaymentTotal = payments.Where(p => p.Status == STATUS_PAGO).Sum(p => p.Amount);
                    var monthIncomeTotal = incomes.Where(i => i.Status == STATUS_RECEBIDO).Sum(i => i.Amount);

                    result.Add(new MonthlyComparisonData
                    {
                        Month = monthName,
                        PaymentAmount = monthPaymentTotal,
                        IncomeAmount = monthIncomeTotal
                    });
                }
                catch
                {
                    // Em caso de erro, adiciona valores zero
                    result.Add(new MonthlyComparisonData
                    {
                        Month = monthName,
                        PaymentAmount = 0,
                        IncomeAmount = 0
                    });
                }
            }

            return result;
        }

        private string GetInvestmentTypeName(int type)
        {
            return type switch
            {
                1 => "Ações",
                2 => "Fundos Imobiliários",
                3 => "ETFs",
                4 => "Ações Estrangeiras",
                5 => "Renda Fixa",
                _ => "Outro"
            };
        }
    }
}