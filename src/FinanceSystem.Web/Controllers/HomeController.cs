using FinanceSystem.Web.Extensions;
using FinanceSystem.Web.Models;
using FinanceSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;

namespace FinanceSystem.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly ICreditCardService _creditCardService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            IPaymentService paymentService,
            ICreditCardService creditCardService,
            ILogger<HomeController> logger)
        {
            _paymentService = paymentService;
            _creditCardService = creditCardService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                var pendingPayments = await _paymentService.GetPendingPaymentsAsync(token);
                var overduePayments = await _paymentService.GetOverduePaymentsAsync(token);
                var creditCards = await _creditCardService.GetAllCreditCardsAsync(token);
                decimal totalBalance = 5000.00m;
                var monthlyData = await GetMonthlyDataAsync(token);

                var labels = monthlyData.Keys.ToList();
                var values = monthlyData.Values.ToList();

                var jsonOptions = new JsonSerializerOptions
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                ViewBag.MonthlyLabels = JsonSerializer.Serialize(labels, jsonOptions);
                ViewBag.MonthlyValues = JsonSerializer.Serialize(values, jsonOptions);
                ViewBag.PaymentsPending = pendingPayments;
                ViewBag.PaymentsOverdue = overduePayments;
                ViewBag.CreditCards = creditCards;
                ViewBag.TotalBalance = totalBalance;
                ViewBag.MonthlyData = monthlyData;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar dashboard");
                TempData["ErrorMessage"] = $"Erro ao carregar dashboard: {ex.Message}";
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        private async Task<Dictionary<string, decimal>> GetMonthlyDataAsync(string token)
        {
            var result = new Dictionary<string, decimal>();
            var currentDate = DateTime.Now;

            for (int i = 5; i >= 0; i--)
            {
                var month = currentDate.AddMonths(-i).Month;
                var year = currentDate.AddMonths(-i).Year;
                var monthName = new DateTime(year, month, 1).ToString("MMM/yy");

                try
                {
                    var payments = await _paymentService.GetPaymentsByMonthAsync(month, year, token);
                    var monthTotal = payments.Sum(p => p.Amount);
                    result.Add(monthName, monthTotal);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao obter dados do mês {Month}/{Year}", month, year);
                    result.Add(monthName, 0);
                }
            }

            return result;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}