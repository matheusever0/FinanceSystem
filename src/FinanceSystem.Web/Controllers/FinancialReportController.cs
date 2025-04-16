using FinanceSystem.Web.Extensions;
using FinanceSystem.Web.Filters;
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
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(
            IPaymentService paymentService,
            IPaymentTypeService paymentTypeService,
            IPaymentMethodService paymentMethodService,
            ICreditCardService creditCardService,
            ILogger<ReportsController> logger)
        {
            _paymentService = paymentService;
            _paymentTypeService = paymentTypeService;
            _paymentMethodService = paymentMethodService;
            _creditCardService = creditCardService;
            _logger = logger;
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
                    .Select(g => new {
                        TypeId = g.Key,
                        TypeName = paymentTypes.FirstOrDefault(t => t.Id == g.Key)?.Name ?? "Desconhecido",
                        TotalAmount = g.Sum(p => p.Amount)
                    })
                    .OrderByDescending(g => g.TotalAmount)
                    .ToList();

                                var totalAmount = payments.Sum(p => p.Amount);
                var paidAmount = payments.Where(p => p.Status == 2).Sum(p => p.Amount);
                var pendingAmount = payments.Where(p => p.Status == 1).Sum(p => p.Amount);
                var overdueAmount = payments.Where(p => p.Status == 3).Sum(p => p.Amount);

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
                _logger.LogError(ex, "Erro ao gerar relatório mensal");
                TempData["ErrorMessage"] = $"Erro ao gerar relatório: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Annual(int? year)
        {
            try
            {
                                year ??= DateTime.Now.Year;

                var token = HttpContext.GetJwtToken();

                                var monthlyData = new Dictionary<string, decimal>();
                for (int month = 1; month <= 12; month++)
                {
                    var monthName = new DateTime(year.Value, month, 1).ToString("MMM");
                    try
                    {
                        var payments = await _paymentService.GetPaymentsByMonthAsync(month, year.Value, token);
                        monthlyData.Add(monthName, payments.Sum(p => p.Amount));
                    }
                    catch (Exception)
                    {
                                                monthlyData.Add(monthName, 0);
                    }
                }

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
                _logger.LogError(ex, "Erro ao gerar relatório anual");
                TempData["ErrorMessage"] = $"Erro ao gerar relatório: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> CreditCards()
        {
            try
            {
                var token = HttpContext.GetJwtToken();

                                var creditCards = await _creditCardService.GetAllCreditCardsAsync(token);

                                var cardData = new List<object>();
                foreach (var card in creditCards)
                {
                    try
                    {
                        var payments = await _paymentService.GetPaymentsByMethodAsync(card.PaymentMethodId, token);
                        var totalAmount = payments.Sum(p => p.Amount);
                        var paidAmount = payments.Where(p => p.Status == 2).Sum(p => p.Amount);
                        var pendingAmount = payments.Where(p => p.Status == 1).Sum(p => p.Amount);

                        cardData.Add(new
                        {
                            Card = card,
                            Payments = payments,
                            TotalAmount = totalAmount,
                            PaidAmount = paidAmount,
                            PendingAmount = pendingAmount,
                            UsagePercentage = (card.Limit - card.AvailableLimit) / card.Limit * 100
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro ao obter dados do cartão {CardId}", card.Id);
                    }
                }

                ViewBag.CardData = cardData;

                return View(creditCards);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar relatório de cartões de crédito");
                TempData["ErrorMessage"] = $"Erro ao gerar relatório: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public async Task<IActionResult> PrintMonthly(int month, int year)
        {
            try
            {
                var token = HttpContext.GetJwtToken();

                                var payments = await _paymentService.GetPaymentsByMonthAsync(month, year, token);

                                var totalAmount = payments.Sum(p => p.Amount);
                var paidAmount = payments.Where(p => p.Status == 2).Sum(p => p.Amount);
                var pendingAmount = payments.Where(p => p.Status == 1).Sum(p => p.Amount);
                var overdueAmount = payments.Where(p => p.Status == 3).Sum(p => p.Amount);

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
                _logger.LogError(ex, "Erro ao gerar relatório mensal para impressão");
                TempData["ErrorMessage"] = $"Erro ao gerar relatório: {ex.Message}";
                return RedirectToAction("Monthly", new { month, year });
            }
        }
    }
}