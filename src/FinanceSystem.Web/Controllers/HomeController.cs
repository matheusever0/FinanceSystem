using FinanceSystem.Web.Extensions;
using FinanceSystem.Web.Models.Generics;
using FinanceSystem.Web.Models.Payment;
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
        private const int STATUS_PAGO = 2;
        private const int MONTHS_TO_SHOW = 6;

        private const string ERROR_LOADING_DASHBOARD = "Erro ao carregar dashboard: {0}";

        private readonly IPaymentService _paymentService;
        private readonly ICreditCardService _creditCardService;
        private readonly IIncomeService _incomeService;

        public HomeController(
            IPaymentService paymentService,
            ICreditCardService creditCardService,
            IIncomeService incomeService)
        {
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _creditCardService = creditCardService ?? throw new ArgumentNullException(nameof(creditCardService));
            _incomeService = incomeService ?? throw new ArgumentNullException(nameof(incomeService));
        }

        public async Task<IActionResult> Index()
        {
            if (!HttpContext.IsUserAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var token = HttpContext.GetJwtToken();
                await LoadDashboardData(token);
                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = string.Format(ERROR_LOADING_DASHBOARD, ex.Message);
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        private async Task LoadDashboardData(string token)
        {
            var currentDate = DateTime.Now;
            var currentMonth = currentDate.Month;
            var currentYear = currentDate.Year;

            var pendingPayments = await _paymentService.GetPendingPaymentsAsync(token);
            var overduePayments = await _paymentService.GetOverduePaymentsAsync(token);
            var pendingIncomes = await _incomeService.GetPendingIncomesAsync(token);
            var receivedIncomes = await _incomeService.GetReceivedIncomesAsync(token);
            var creditCards = await _creditCardService.GetAllCreditCardsAsync(token);
            var payments = await _paymentService.GetAllPaymentsAsync(token);
            var incomesMonth = await _incomeService.GetIncomesByMonthAsync(currentMonth, currentYear, token);
            var paymentsMonth = await _paymentService.GetPaymentsByMonthAsync(currentMonth, currentYear, token);
            var overdueIncomes = await _incomeService.GetOverdueIncomesAsync(token);

            decimal totalIncome = receivedIncomes.Where(e => e.Status == STATUS_PAGO).Sum(i => i.Amount);
            decimal totalPayments = payments.Where(e => e.Status == STATUS_PAGO).Sum(i => i.Amount);
            decimal totalBalance = totalIncome - totalPayments;

            ViewBag.TotalBalance = totalBalance;
            ViewBag.PendingPayments = pendingPayments;
            ViewBag.PaymentsOverdue = overduePayments;
            ViewBag.OverdueIncomes = overdueIncomes;
            ViewBag.PendingIncomes = pendingIncomes;
            ViewBag.CreditCards = creditCards;
            ViewBag.IncomesMonth = incomesMonth.Where(e => e.Status == STATUS_PAGO).Sum(i => i.Amount);
            ViewBag.PaymentsMonth = paymentsMonth.Where(e => e.Status == STATUS_PAGO).Sum(i => i.Amount);

            var monthlyData = await GetMonthlyComparisonDataAsync(token);
            ViewBag.MonthlyLabels = JsonSerializer.Serialize(monthlyData.Select(m => m.Month));
            ViewBag.MonthlyIncomeValues = JsonSerializer.Serialize(monthlyData.Select(m => m.IncomeAmount));
            ViewBag.MonthlyPaymentValues = JsonSerializer.Serialize(monthlyData.Select(m => m.PaymentAmount));
        }

        private async Task<List<MonthlyComparisonData>> GetMonthlyComparisonDataAsync(string token)
        {
            var result = new List<MonthlyComparisonData>();
            var currentDate = DateTime.Now;

            for (int i = MONTHS_TO_SHOW - 1; i >= 0; i--)
            {
                var targetDate = currentDate.AddMonths(-i);
                var month = targetDate.Month;
                var year = targetDate.Year;
                var monthName = targetDate.ToString("MMM/yy");

                try
                {
                    var payments = await _paymentService.GetPaymentsByMonthAsync(month, year, token);
                    var incomes = await _incomeService.GetIncomesByMonthAsync(month, year, token);

                    var monthPaymentTotal = payments.Where(e => e.Status == STATUS_PAGO).Sum(p => p.Amount);
                    var monthIncomeTotal = incomes.Where(e => e.Status == STATUS_PAGO).Sum(i => i.Amount);

                    result.Add(new MonthlyComparisonData
                    {
                        Month = monthName,
                        PaymentAmount = monthPaymentTotal,
                        IncomeAmount = monthIncomeTotal
                    });
                }
                catch
                {
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