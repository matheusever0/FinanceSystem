using FinanceSystem.Web.Extensions;
using FinanceSystem.Web.Filters;
using FinanceSystem.Web.Models.Generics;
using FinanceSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceSystem.Web.Controllers
{
    [Authorize]
    [RequirePermission("payments.view")]
    public class ReportsController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IPaymentTypeService _paymentTypeService;
        private readonly IPaymentMethodService _paymentMethodService;
        private readonly ICreditCardService _creditCardService;

        private const string ERROR_GENERATING_MONTHLY_REPORT = "Erro ao gerar relatório mensal: {0}";
        private const string ERROR_GENERATING_ANNUAL_REPORT = "Erro ao gerar relatório anual: {0}";
        private const string ERROR_GENERATING_CREDIT_CARDS_REPORT = "Erro ao gerar relatório de cartões de crédito: {0}";
        private const string ERROR_GENERATING_PRINT_REPORT = "Erro ao gerar relatório para impressão: {0}";

        private const int STATUS_PAID = 2;
        private const int STATUS_PENDING = 1;
        private const int STATUS_OVERDUE = 3;

        public ReportsController(
            IPaymentService paymentService,
            IPaymentTypeService paymentTypeService,
            IPaymentMethodService paymentMethodService,
            ICreditCardService creditCardService)
        {
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _paymentTypeService = paymentTypeService ?? throw new ArgumentNullException(nameof(paymentTypeService));
            _paymentMethodService = paymentMethodService ?? throw new ArgumentNullException(nameof(paymentMethodService));
            _creditCardService = creditCardService ?? throw new ArgumentNullException(nameof(creditCardService));
        }

        public async Task<IActionResult> Monthly(int? month, int? year)
        {
            try
            {
                month ??= DateTime.Now.Month;
                year ??= DateTime.Now.Year;

                var token = HttpContext.GetJwtToken();

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

                ViewBag.PaymentTypes = paymentTypes;
                ViewBag.PaymentsByType = paymentsByType;
                ViewBag.Month = month;
                ViewBag.Year = year;
                ViewBag.TotalAmount = totalAmount;
                ViewBag.PaidAmount = paidAmount;
                ViewBag.PendingAmount = pendingAmount;
                ViewBag.OverdueAmount = overdueAmount;

                return View(payments);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_GENERATING_MONTHLY_REPORT, ex.Message);
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Annual(int? year)
        {
            try
            {
                year ??= DateTime.Now.Year;
                var token = HttpContext.GetJwtToken();
                var monthlyData = await GetMonthlyDataForYear(year.Value, token);

                var paymentTypes = await _paymentTypeService.GetAllPaymentTypesAsync(token);
                var paymentMethods = await _paymentMethodService.GetAllPaymentMethodsAsync(token);

                ViewBag.MonthlyData = monthlyData;
                ViewBag.Year = year;
                ViewBag.PaymentTypes = paymentTypes;
                ViewBag.PaymentMethods = paymentMethods;

                var totalAnnual = monthlyData.Values.Sum();
                ViewBag.TotalAnnual = totalAnnual;
                ViewBag.AverageMonthly = totalAnnual / Math.Max(1, monthlyData.Count(m => m.Value > 0));

                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_GENERATING_ANNUAL_REPORT, ex.Message);
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> CreditCards()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                var creditCards = await _creditCardService.GetAllCreditCardsAsync(token);
                var cardData = await GetCreditCardData(creditCards, token);

                ViewBag.CardData = cardData;
                return View(creditCards);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_GENERATING_CREDIT_CARDS_REPORT, ex.Message);
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
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
                var payments = await _paymentService.GetPaymentsByMonthAsync(month, year, token);

                var totalAmount = payments.Sum(p => p.Amount);
                var paidAmount = payments.Where(p => p.Status == STATUS_PAID).Sum(p => p.Amount);
                var pendingAmount = payments.Where(p => p.Status == STATUS_PENDING).Sum(p => p.Amount);
                var overdueAmount = payments.Where(p => p.Status == STATUS_OVERDUE).Sum(p => p.Amount);

                ViewBag.Month = month;
                ViewBag.Year = year;
                ViewBag.TotalAmount = totalAmount;
                ViewBag.PaidAmount = paidAmount;
                ViewBag.PendingAmount = pendingAmount;
                ViewBag.OverdueAmount = overdueAmount;
                ViewBag.PrintMode = true;

                return View("PrintMonthly", payments);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_GENERATING_PRINT_REPORT, ex.Message);
                return RedirectToAction("Monthly", new { month, year });
            }
        }

        private async Task<Dictionary<string, decimal>> GetMonthlyDataForYear(int year, string token)
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

        private async Task<List<object>> GetCreditCardData(IEnumerable<object> creditCards, string token)
        {
            var cardData = new List<object>();

            foreach (var card in creditCards)
            {
                try
                {
                    var paymentMethodId = GetPropertyValue(card, "PaymentMethodId")?.ToString();

                    if (string.IsNullOrEmpty(paymentMethodId))
                    {
                        continue;
                    }

                    var payments = await _paymentService.GetPaymentsByMethodAsync(paymentMethodId, token);
                    var totalAmount = payments.Sum(p => p.Amount);
                    var paidAmount = payments.Where(p => p.Status == STATUS_PAID).Sum(p => p.Amount);
                    var pendingAmount = payments.Where(p => p.Status == STATUS_PENDING).Sum(p => p.Amount);

                    var limit = Convert.ToDecimal(GetPropertyValue(card, "Limit"));
                    var availableLimit = Convert.ToDecimal(GetPropertyValue(card, "AvailableLimit"));
                    decimal usagePercentage = 0;

                    if (limit > 0)
                    {
                        usagePercentage = (limit - availableLimit) / limit * 100;
                    }

                    cardData.Add(new
                    {
                        Card = card,
                        Payments = payments,
                        TotalAmount = totalAmount,
                        PaidAmount = paidAmount,
                        PendingAmount = pendingAmount,
                        UsagePercentage = usagePercentage
                    });
                }
                catch
                {
                    // Se falhar ao processar um cartão específico, continua com os outros
                }
            }

            return cardData;
        }

        private object GetPropertyValue(object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName)?.GetValue(obj, null);
        }
    }
}