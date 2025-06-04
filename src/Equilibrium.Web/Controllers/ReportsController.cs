using Equilibrium.Resources.Web.Enums;
using Equilibrium.Resources.Web.Helpers;
using Equilibrium.Web.Extensions;
using Equilibrium.Web.Filters;
using Equilibrium.Web.Interfaces;
using Equilibrium.Web.Models.Generics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Equilibrium.Web.Controllers
{
    [Authorize]
    [RequirePermission("reports.view")]
    public class ReportsController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IPaymentTypeService _paymentTypeService;
        private readonly IPaymentMethodService _paymentMethodService;
        private readonly IIncomeService _incomeService;
        private readonly IIncomeTypeService _incomeTypeService;

        // Status codes para pagamentos
        private const int STATUS_PAID = 2;
        private const int STATUS_PENDING = 1;
        private const int STATUS_OVERDUE = 3;

        // Status codes para receitas
        private const int INCOME_STATUS_RECEIVED = 2;
        private const int INCOME_STATUS_PENDING = 1;
        private const int INCOME_STATUS_OVERDUE = 3;

        public ReportsController(
            IPaymentService paymentService,
            IIncomeService incomeService,
            IPaymentTypeService paymentTypeService,
            ICreditCardService creditCardService,
            IPaymentMethodService paymentMethodService,
            IIncomeTypeService incomeTypeService)
        {
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _incomeService = incomeService ?? throw new ArgumentNullException(nameof(incomeService));
            _paymentTypeService = paymentTypeService ?? throw new ArgumentNullException(nameof(paymentTypeService));
            _paymentMethodService = paymentMethodService ?? throw new ArgumentNullException(nameof(paymentMethodService));
            _incomeTypeService = incomeTypeService ?? throw new ArgumentNullException(nameof(incomeTypeService));
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Monthly(int? month, int? year)
        {
            try
            {
                month ??= DateTime.Now.Month;
                year ??= DateTime.Now.Year;

                var token = HttpContext.GetJwtToken();

                // Carregar pagamentos
                var payments = await _paymentService.GetPaymentsByMonthAsync(month.Value, year.Value, token);
                var paymentTypes = await _paymentTypeService.GetAllPaymentTypesAsync(token);

                var paymentsByType = payments
                    .GroupBy(p => p.PaymentTypeId)
                    .Select(g => new PaymentByTypeDto
                    {
                        TypeId = g.Key,
                        TypeName = paymentTypes.FirstOrDefault(t => t.Id == g.Key)?.Name ?? "Desconhecido",
                        TotalAmount = g.Sum(p => p.Amount)
                    })
                    .OrderByDescending(g => g.TotalAmount)
                    .ToList();

                var totalAmount = payments.Sum(p => p.Amount);
                var paidAmount = payments.Where(p => p.Status == STATUS_PAID).Sum(p => p.Amount);
                var pendingAmount = payments.Where(p => p.Status == STATUS_PENDING).Sum(p => p.Amount);
                var overdueAmount = payments.Where(p => p.Status == STATUS_OVERDUE).Sum(p => p.Amount);

                // Carregar receitas
                var incomes = await _incomeService.GetIncomesByMonthAsync(month.Value, year.Value, token);
                var incomeTypes = await _incomeTypeService.GetAllIncomeTypesAsync(token);

                var incomesByType = incomes
                    .GroupBy(i => i.IncomeTypeId)
                    .Select(g => new PaymentByTypeDto
                    {
                        TypeId = g.Key,
                        TypeName = incomeTypes.FirstOrDefault(t => t.Id == g.Key)?.Name ?? "Desconhecido",
                        TotalAmount = g.Sum(i => i.Amount)
                    })
                    .OrderByDescending(g => g.TotalAmount)
                    .ToList();

                var totalIncomeAmount = incomes.Sum(i => i.Amount);
                var receivedIncomeAmount = incomes.Where(i => i.Status == INCOME_STATUS_RECEIVED).Sum(i => i.Amount);
                var pendingIncomeAmount = incomes.Where(i => i.Status == INCOME_STATUS_PENDING).Sum(i => i.Amount);
                var overdueIncomeAmount = incomes.Where(i => i.Status == INCOME_STATUS_OVERDUE).Sum(i => i.Amount);

                // Passar dados para a view
                ViewBag.PaymentTypes = paymentTypes;
                ViewBag.PaymentsByType = paymentsByType;
                ViewBag.Month = month;
                ViewBag.Year = year;
                ViewBag.TotalAmount = totalAmount;
                ViewBag.PaidAmount = paidAmount;
                ViewBag.PendingAmount = pendingAmount;
                ViewBag.OverdueAmount = overdueAmount;

                // Dados de receitas para a view
                ViewBag.Incomes = incomes;
                ViewBag.IncomeTypes = incomeTypes;
                ViewBag.IncomesByType = incomesByType;
                ViewBag.TotalIncomeAmount = totalIncomeAmount;
                ViewBag.ReceivedIncomeAmount = receivedIncomeAmount;
                ViewBag.PendingIncomeAmount = pendingIncomeAmount;
                ViewBag.OverdueIncomeAmount = overdueIncomeAmount;

                return View(payments);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetReportGenerationErrorMessage(ReportType.Monthly, ex);
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Annual(int? year)
        {
            try
            {
                year ??= DateTime.Now.Year;
                var token = HttpContext.GetJwtToken();

                // Get payment data
                var monthlyPaymentsData = await GetMonthlyPaymentsDataForYear(year.Value, token);

                // Get income data (new)
                var monthlyIncomesData = await GetMonthlyIncomesDataForYear(year.Value, token);

                var paymentTypes = await _paymentTypeService.GetAllPaymentTypesAsync(token);
                var paymentMethods = await _paymentMethodService.GetAllPaymentMethodsAsync(token);
                var incomeTypes = await _incomeTypeService.GetAllIncomeTypesAsync(token);

                // Payment totals
                var totalAnnualPayments = monthlyPaymentsData.Values.Sum();
                var averageMonthlyPayments = totalAnnualPayments / Math.Max(1, monthlyPaymentsData.Count(m => m.Value > 0));

                // Income totals (new)
                var totalAnnualIncomes = monthlyIncomesData.Values.Sum();
                var averageMonthlyIncomes = totalAnnualIncomes / Math.Max(1, monthlyIncomesData.Count(m => m.Value > 0));

                // Balance (new)
                var monthlyBalanceData = GetMonthlyBalanceData(monthlyIncomesData, monthlyPaymentsData);
                var totalAnnualBalance = totalAnnualIncomes - totalAnnualPayments;

                ViewBag.MonthlyPaymentsData = monthlyPaymentsData;
                ViewBag.MonthlyIncomesData = monthlyIncomesData;
                ViewBag.MonthlyBalanceData = monthlyBalanceData;
                ViewBag.Year = year;
                ViewBag.PaymentTypes = paymentTypes;
                ViewBag.PaymentMethods = paymentMethods;
                ViewBag.IncomeTypes = incomeTypes;

                ViewBag.TotalAnnualPayments = totalAnnualPayments;
                ViewBag.AverageMonthlyPayments = averageMonthlyPayments;
                ViewBag.TotalAnnualIncomes = totalAnnualIncomes;
                ViewBag.AverageMonthlyIncomes = averageMonthlyIncomes;
                ViewBag.TotalAnnualBalance = totalAnnualBalance;

                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetReportGenerationErrorMessage(ReportType.Annual, ex);
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> PrintMonthly(int month, int year)
        {
            if (month <= 0 || month > 12)
            {
                return BadRequest("Mês inválido");
            }

            if (year <= 0)
            {
                return BadRequest("Ano inválido");
            }

            try
            {
                var token = HttpContext.GetJwtToken();

                // Carregar pagamentos
                var payments = await _paymentService.GetPaymentsByMonthAsync(month, year, token);
                var totalAmount = payments.Sum(p => p.Amount);
                var paidAmount = payments.Where(p => p.Status == STATUS_PAID).Sum(p => p.Amount);
                var pendingAmount = payments.Where(p => p.Status == STATUS_PENDING).Sum(p => p.Amount);
                var overdueAmount = payments.Where(p => p.Status == STATUS_OVERDUE).Sum(p => p.Amount);

                // Carregar receitas
                var incomes = await _incomeService.GetIncomesByMonthAsync(month, year, token);
                var totalIncomeAmount = incomes.Sum(i => i.Amount);
                var receivedIncomeAmount = incomes.Where(i => i.Status == INCOME_STATUS_RECEIVED).Sum(i => i.Amount);
                var pendingIncomeAmount = incomes.Where(i => i.Status == INCOME_STATUS_PENDING).Sum(i => i.Amount);
                var overdueIncomeAmount = incomes.Where(i => i.Status == INCOME_STATUS_OVERDUE).Sum(i => i.Amount);

                ViewBag.Month = month;
                ViewBag.Year = year;
                ViewBag.TotalAmount = totalAmount;
                ViewBag.PaidAmount = paidAmount;
                ViewBag.PendingAmount = pendingAmount;
                ViewBag.OverdueAmount = overdueAmount;

                ViewBag.Incomes = incomes;
                ViewBag.TotalIncomeAmount = totalIncomeAmount;
                ViewBag.ReceivedIncomeAmount = receivedIncomeAmount;
                ViewBag.PendingIncomeAmount = pendingIncomeAmount;
                ViewBag.OverdueIncomeAmount = overdueIncomeAmount;

                ViewBag.Balance = totalIncomeAmount - totalAmount;

                return View("PrintMonthly", payments);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetReportGenerationErrorMessage(ReportType.Print, ex);
                return RedirectToAction("Index", "Home");
            }
        }

        private async Task<Dictionary<string, decimal>> GetMonthlyIncomesDataForYear(int year, string token)
        {
            var monthlyData = new Dictionary<string, decimal>();

            for (int month = 1; month <= 12; month++)
            {
                var monthName = new DateTime(year, month, 1).ToString("MMM");
                try
                {
                    var incomes = await _incomeService.GetIncomesByMonthAsync(month, year, token);
                    monthlyData.Add(monthName, incomes.Sum(p => p.Amount));
                }
                catch
                {
                    monthlyData.Add(monthName, 0);
                }
            }

            return monthlyData;
        }

        private Dictionary<string, decimal> GetMonthlyBalanceData(
            Dictionary<string, decimal> incomeData,
            Dictionary<string, decimal> paymentData)
        {
            var balanceData = new Dictionary<string, decimal>();

            foreach (var month in incomeData.Keys)
            {
                var income = incomeData.GetValueOrDefault(month, 0);
                var payment = paymentData.GetValueOrDefault(month, 0);
                balanceData[month] = income - payment;
            }

            return balanceData;
        }

        private async Task<Dictionary<string, decimal>> GetMonthlyPaymentsDataForYear(int year, string token)
        {
            var monthlyData = new Dictionary<string, decimal>();

            for (int month = 1; month <= 12; month++)
            {
                var monthName = new DateTime(year, month, 1).ToString("MMM");
                try
                {
                    var payments = await _paymentService.GetPaymentsByMonthAsync(month, year, token);
                    monthlyData.Add(monthName, payments.Sum(p => p.Amount));
                }
                catch
                {
                    monthlyData.Add(monthName, 0);
                }
            }

            return monthlyData;
        }
    }
}