using Equilibrium.Web.Extensions;
using Equilibrium.Web.Interfaces;
using Equilibrium.Web.Models.Generics;
using Equilibrium.Web.Models.Payment;
using Equilibrium.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;
using Equilibrium.Resources.Web.Enums;
using Equilibrium.Resources.Web.Helpers;

namespace Equilibrium.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private const int STATUS_PAGO = 2;
        private const int MONTHS_TO_SHOW = 6;

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
                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = MessageHelper.GetLoadingErrorMessage(EntityNames.Dashboard, ex);
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
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

        [HttpGet]
        public async Task<IActionResult> GetFinancialSummary()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                await LoadFinancialSummaryData(token);
                return PartialView("_Partials/_FinancialSummary");
            }
            catch (Exception ex)
            {
                return StatusCode(500, MessageHelper.GetLoadingErrorMessage(EntityNames.Dashboard, ex));
            }
        }

        [HttpGet]
        public IActionResult GetQuickActions()
        {
            try
            {
                return PartialView("_Partials/_QuickActions");
            }
            catch (Exception ex)
            {
                return StatusCode(500, MessageHelper.GetLoadingErrorMessage(EntityNames.Dashboard, ex));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMonthlyComparison()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                await LoadMonthlyComparisonData(token);
                return PartialView("_Partials/_MonthlyComparison");
            }
            catch (Exception ex)
            {
                return StatusCode(500, MessageHelper.GetLoadingErrorMessage(EntityNames.Dashboard, ex));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPendings()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                await LoadPendingsData(token);
                return PartialView("_Partials/_Pendings");
            }
            catch (Exception ex)
            {
                return StatusCode(500, MessageHelper.GetLoadingErrorMessage(EntityNames.Dashboard, ex));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetInvestmentSummary()
        {
            try
            {
                var token = HttpContext.GetJwtToken();
                await LoadInvestmentSummaryData(token);
                return PartialView("_Partials/_InvestmentSummary");
            }
            catch (Exception ex)
            {
                return StatusCode(500, MessageHelper.GetLoadingErrorMessage(EntityNames.Dashboard, ex));
            }
        }

        private async Task LoadFinancialSummaryData(string token)
        {
            var currentDate = DateTime.Now;
            var currentMonth = currentDate.Month;
            var currentYear = currentDate.Year;

            var creditCards = await _creditCardService.GetAllCreditCardsAsync(token);
            var incomesMonth = await _incomeService.GetIncomesByMonthAsync(currentMonth, currentYear, token);
            var paymentsMonth = await _paymentService.GetPaymentsByMonthAsync(currentMonth, currentYear, token);
            var pendingIncomes = await _incomeService.GetPendingIncomesAsync(token);
            var pendingPayments = await _paymentService.GetPendingPaymentsAsync(token);
            var overduePayments = await _paymentService.GetOverduePaymentsAsync(token);
            var overdueIncomes = await _incomeService.GetOverdueIncomesAsync(token);
            var receivedIncomes = await _incomeService.GetReceivedIncomesAsync(token);
            var payments = await _paymentService.GetAllPaymentsAsync(token);

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
        }

        private async Task LoadMonthlyComparisonData(string token)
        {
            var monthlyData = await GetMonthlyComparisonDataAsync(token);
            ViewBag.MonthlyLabels = JsonSerializer.Serialize(monthlyData.Select(m => m.Month));
            ViewBag.MonthlyIncomeValues = JsonSerializer.Serialize(monthlyData.Select(m => m.IncomeAmount));
            ViewBag.MonthlyPaymentValues = JsonSerializer.Serialize(monthlyData.Select(m => m.PaymentAmount));
        }

        private async Task LoadPendingsData(string token)
        {
            var pendingPayments = await _paymentService.GetPendingPaymentsAsync(token);
            var overduePayments = await _paymentService.GetOverduePaymentsAsync(token);
            var pendingIncomes = await _incomeService.GetPendingIncomesAsync(token);
            var overdueIncomes = await _incomeService.GetOverdueIncomesAsync(token);

            ViewBag.PendingPayments = pendingPayments;
            ViewBag.PaymentsOverdue = overduePayments;
            ViewBag.PendingIncomes = pendingIncomes;
            ViewBag.OverdueIncomes = overdueIncomes;
        }

        private async Task LoadInvestmentSummaryData(string token)
        {
            var investmentService = HttpContext.RequestServices.GetService<IInvestmentService>();
            var investments = (await investmentService!.GetAllInvestmentsAsync(token)).ToArray();

            ViewBag.TotalInvested = investments.Sum(i => i.TotalInvested);
            ViewBag.CurrentInvestmentsValue = investments.Sum(i => i.CurrentTotal);
            ViewBag.InvestmentsGainLoss = investments.Sum(i => i.GainLossValue);
            ViewBag.TopPerformingInvestments = investments
                .OrderByDescending(i => i.GainLossPercentage)
                .Take(3)
                .ToList();
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