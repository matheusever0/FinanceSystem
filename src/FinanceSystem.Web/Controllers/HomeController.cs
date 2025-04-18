using FinanceSystem.Web.Extensions;
using FinanceSystem.Web.Models.Generics;
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
        private readonly IIncomeService _incomeService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            IPaymentService paymentService,
            ICreditCardService creditCardService,
            IIncomeService incomeService,
            ILogger<HomeController> logger)
        {
            _paymentService = paymentService;
            _creditCardService = creditCardService;
            _incomeService = incomeService;
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
                var pendingIncomes = await _incomeService.GetPendingIncomesAsync(token);
                var receivedIncomes = await _incomeService.GetReceivedIncomesAsync(token);
                var creditCards = await _creditCardService.GetAllCreditCardsAsync(token);
                var payments = await _paymentService.GetAllPaymentsAsync(token);
                var incomesMonth = await _incomeService.GetIncomesByMonthAsync(DateTime.Now.Month, DateTime.Now.Year, token);
                var paymentsMonth = await _paymentService.GetPaymentsByMonthAsync(DateTime.Now.Month, DateTime.Now.Year, token);

                // Calcular saldo dinâmico
                decimal totalIncome = receivedIncomes.Sum(i => i.Amount);
                decimal totalPayments = payments.Sum(i => i.Amount);
                decimal totalBalance = totalIncome - totalPayments;

                ViewBag.TotalBalance = totalBalance;
                ViewBag.PendingPayments = pendingPayments;
                ViewBag.PaymentsOverdue = overduePayments;
                ViewBag.PendingIncomes = pendingIncomes;
                ViewBag.CreditCards = creditCards;
                ViewBag.IncomesMonth = incomesMonth.Sum(i => i.Amount);
                ViewBag.PaymentsMonth = paymentsMonth.Sum(i => i.Amount);

                // Manter lógica existente de gráficos mensais
                var monthlyData = await GetMonthlyDataAsync(token);
                ViewBag.MonthlyLabels = JsonSerializer.Serialize(monthlyData.Keys.ToList());
                ViewBag.MonthlyValues = JsonSerializer.Serialize(monthlyData.Values.ToList());

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