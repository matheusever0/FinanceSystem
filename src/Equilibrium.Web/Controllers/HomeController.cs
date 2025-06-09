using Equilibrium.Resources.Web.Enums;
using Equilibrium.Web.Interfaces;
using Equilibrium.Web.Models.Filters;
using Equilibrium.Web.Models.Generics;
using Equilibrium.Web.Models.Payment;
using Equilibrium.Web.Models.Income;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;
using Equilibrium.Web.Models.Financing;
using Equilibrium.Web.Models.CreditCard;

namespace Equilibrium.Web.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        private const int MONTHS_TO_SHOW = 3;

        private readonly IPaymentService _paymentService;
        private readonly ICreditCardService _creditCardService;
        private readonly IIncomeService _incomeService;
        private readonly IFinancingService _financingService;

        public HomeController(
            IPaymentService paymentService,
            ICreditCardService creditCardService,
            IIncomeService incomeService,
            IFinancingService financingService)
        {
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _creditCardService = creditCardService ?? throw new ArgumentNullException(nameof(creditCardService));
            _incomeService = incomeService ?? throw new ArgumentNullException(nameof(incomeService));
            _financingService = financingService ?? throw new ArgumentNullException(nameof(financingService));
        }

        public async Task<IActionResult> Index()
        {
            if (!IsUserAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var token = GetToken();
                await LoadDashboardData(token);
                return View();
            }
            catch (Exception ex)
            {
                return HandleException(ex, EntityNames.Dashboard, "loading");
            }
        }

        private async Task LoadDashboardData(string token)
        {
            var currentDate = DateTime.Now;
            var currentMonth = currentDate.Month;
            var currentYear = currentDate.Year;

            try
            {
                var creditCards = await _creditCardService.GetAllCreditCardsAsync(token);
                var thisMonthReceivedIncomesFilter = new IncomeFilter { Month = currentMonth, Year = currentYear, Status = "Received" };
                var thisMonthReceivedIncomes = await _incomeService.GetFilteredIncomesAsync(thisMonthReceivedIncomesFilter, token);

                var thisMonthPaidPaymentsFilter = new PaymentFilter { Month = currentMonth, Year = currentYear, Status = "Paid" };
                var thisMonthPaidPayments = await _paymentService.GetFilteredPaymentsAsync(thisMonthPaidPaymentsFilter, token);

                var pendingPayments = await _paymentService.GetFilteredPaymentsAsync(new PaymentFilter { Status = "Pending", Month = currentMonth, Year = currentYear }, token);
                var overduePayments = await _paymentService.GetFilteredPaymentsAsync(new PaymentFilter { Status = "Overdue", Month = currentMonth, Year = currentYear }, token);
                var pendingIncomes = await _incomeService.GetFilteredIncomesAsync(new IncomeFilter { Status = "Pending", Month = currentMonth, Year = currentYear }, token);
                var overdueIncomes = await _incomeService.GetFilteredIncomesAsync(new IncomeFilter { Status = "Pending", EndDate = DateTime.Today.AddDays(-1) }, token);

                var totalIncomeReceived = thisMonthReceivedIncomes.Sum(i => i.Amount);
                var totalPaymentsPaid = thisMonthPaidPayments.Sum(p => p.Amount);

                var allReceivedIncomesFilter = new IncomeFilter { Status = "Received", EndDate = DateTime.Today };
                var allPaidPaymentsFilter = new PaymentFilter { Status = "Paid", EndDate = DateTime.Today };

                var allReceivedIncomes = await _incomeService.GetFilteredIncomesAsync(allReceivedIncomesFilter, token);
                var allPaidPayments = await _paymentService.GetFilteredPaymentsAsync(allPaidPaymentsFilter, token);

                var totalAllIncomeReceived = allReceivedIncomes.Sum(i => i.Amount);
                var totalAllPaymentsPaid = allPaidPayments.Sum(p => p.Amount);
                var currentBalance = totalAllIncomeReceived - totalAllPaymentsPaid;

                var upcomingPayments = await GetUpcomingPayments(token, 7);
                var upcomingIncomes = await GetUpcomingIncomes(token, 7);

                var activeFinancings = await _financingService.GetActiveFinancingsAsync(token);

                ViewBag.TotalBalance = currentBalance;
                ViewBag.TotalIncomeReceived = totalIncomeReceived;
                ViewBag.TotalPaymentsPaid = totalPaymentsPaid;

                ViewBag.PendingPayments = pendingPayments;
                ViewBag.OverduePayments = overduePayments;
                ViewBag.PendingIncomes = pendingIncomes;
                ViewBag.OverdueIncomes = overdueIncomes;

                ViewBag.UpcomingPayments = upcomingPayments;
                ViewBag.UpcomingIncomes = upcomingIncomes;

                ViewBag.CreditCards = creditCards;
                ViewBag.ActiveFinancings = activeFinancings;

                ViewBag.ThisMonthPayments = totalPaymentsPaid;
                ViewBag.ThisMonthIncomes = totalIncomeReceived;

                ViewBag.TotalPendingPayments = pendingPayments.Sum(p => p.Amount);
                ViewBag.TotalOverduePayments = overduePayments.Sum(p => p.Amount);
                ViewBag.TotalPendingIncomes = pendingIncomes.Sum(i => i.Amount);
                ViewBag.TotalOverdueIncomes = overdueIncomes.Sum(i => i.Amount);

                await LoadChartData(token);
            }
            catch (Exception)
            {
                ViewBag.TotalBalance = 0m;
                ViewBag.TotalIncomeReceived = 0m;
                ViewBag.TotalPaymentsPaid = 0m;
                ViewBag.TotalPendingPayments = 0m;
                ViewBag.TotalOverduePayments = 0m;
                ViewBag.TotalPendingIncomes = 0m;
                ViewBag.TotalOverdueIncomes = 0m;
                ViewBag.PendingPayments = new List<PaymentModel>();
                ViewBag.OverduePayments = new List<PaymentModel>();
                ViewBag.PendingIncomes = new List<IncomeModel>();
                ViewBag.OverdueIncomes = new List<IncomeModel>();
                ViewBag.UpcomingPayments = new List<PaymentModel>();
                ViewBag.UpcomingIncomes = new List<IncomeModel>();
                ViewBag.CreditCards = new List<CreditCardModel>();
                ViewBag.ActiveFinancings = new List<FinancingModel>();
                ViewBag.ThisMonthPayments = 0m;
                ViewBag.ThisMonthIncomes = 0m;
                throw;
            }
        }

        private async Task LoadChartData(string token)
        {
            try
            {
                var monthlyData = await GetMonthlyComparisonDataAsync(token);
                ViewBag.MonthlyLabels = JsonSerializer.Serialize(monthlyData.Select(m => m.Month));
                ViewBag.MonthlyIncomeValues = JsonSerializer.Serialize(monthlyData.Select(m => m.IncomeAmount));
                ViewBag.MonthlyPaymentValues = JsonSerializer.Serialize(monthlyData.Select(m => m.PaymentAmount));

                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;

                var paymentsByType = await GetPaymentsByTypeCurrentMonth(token, currentMonth, currentYear);
                var incomesByType = await GetIncomesByTypeCurrentMonth(token, currentMonth, currentYear);

                ViewBag.PaymentsByTypeLabels = JsonSerializer.Serialize(paymentsByType.Keys);
                ViewBag.PaymentsByTypeValues = JsonSerializer.Serialize(paymentsByType.Values);
                ViewBag.IncomesByTypeLabels = JsonSerializer.Serialize(incomesByType.Keys);
                ViewBag.IncomesByTypeValues = JsonSerializer.Serialize(incomesByType.Values);
            }
            catch
            {
                ViewBag.MonthlyLabels = JsonSerializer.Serialize(Array.Empty<string>());
                ViewBag.MonthlyIncomeValues = JsonSerializer.Serialize(Array.Empty<decimal>());
                ViewBag.MonthlyPaymentValues = JsonSerializer.Serialize(Array.Empty<decimal>());
                ViewBag.PaymentsByTypeLabels = JsonSerializer.Serialize(Array.Empty<string>());
                ViewBag.PaymentsByTypeValues = JsonSerializer.Serialize(Array.Empty<decimal>());
                ViewBag.IncomesByTypeLabels = JsonSerializer.Serialize(Array.Empty<string>());
                ViewBag.IncomesByTypeValues = JsonSerializer.Serialize(Array.Empty<decimal>());
            }
        }

        private async Task<Dictionary<string, decimal>> GetPaymentsByTypeCurrentMonth(string token, int month, int year)
        {
            try
            {
                var filter = new PaymentFilter { Month = month, Year = year, Status = "Paid" };

                var payments = await _paymentService.GetFilteredPaymentsAsync(filter, token);

                return payments
                    .GroupBy(p => p.PaymentTypeName)
                    .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount));
            }
            catch
            {
                return [];
            }
        }

        private async Task<Dictionary<string, decimal>> GetIncomesByTypeCurrentMonth(string token, int month, int year)
        {
            try
            {
                var filter = new IncomeFilter { Month = month, Year = year, Status = "Received" };

                var incomes = await _incomeService.GetFilteredIncomesAsync(filter, token);

                return incomes
                    .GroupBy(i => i.IncomeTypeName)
                    .ToDictionary(g => g.Key, g => g.Sum(i => i.Amount));
            }
            catch
            {
                return [];
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
                    var paymentFilter = new PaymentFilter { Month = month, Year = year, Status = "Paid" };
                    var payments = await _paymentService.GetFilteredPaymentsAsync(paymentFilter, token);

                    var incomeFilter = new IncomeFilter { Month = month, Year = year, Status = "Received" };
                    var incomes = await _incomeService.GetFilteredIncomesAsync(incomeFilter, token);

                    var monthPaymentTotal = payments.Sum(p => p.Amount);
                    var monthIncomeTotal = incomes.Sum(i => i.Amount);

                    result.Add(new MonthlyComparisonData { Month = monthName, PaymentAmount = monthPaymentTotal, IncomeAmount = monthIncomeTotal });
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

        private async Task<List<PaymentModel>> GetUpcomingPayments(string token, int days)
        {
            try
            {
                var endDate = DateTime.Today.AddDays(days);
                var filter = new PaymentFilter { StartDate = DateTime.Today, EndDate = endDate, Status = "Pending", OrderBy = "dueDate", Ascending = true };

                var payments = await _paymentService.GetFilteredPaymentsAsync(filter, token);
                return [.. payments];
            }
            catch
            {
                return [];
            }
        }

        private async Task<List<IncomeModel>> GetUpcomingIncomes(string token, int days)
        {
            try
            {
                var endDate = DateTime.Today.AddDays(days);
                var filter = new IncomeFilter { StartDate = DateTime.Today, EndDate = endDate, Status = "Pending", OrderBy = "dueDate", Ascending = true };

                var incomes = await _incomeService.GetFilteredIncomesAsync(filter, token);
                return [.. incomes];
            }
            catch
            {
                return [];
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetFinancialSummary()
        {
            try
            {
                var token = GetToken();
                await LoadFinancialSummaryData(token);
                return PartialView("_Partials/_FinancialSummary");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao carregar resumo financeiro: {ex.Message}");
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
                return StatusCode(500, $"Erro ao carregar ações rápidas: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMonthlyComparison()
        {
            try
            {
                var token = GetToken();
                await LoadMonthlyComparisonData(token);
                return PartialView("_Partials/_MonthlyComparison");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao carregar comparação mensal: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPendings()
        {
            try
            {
                var token = GetToken();
                await LoadPendingsData(token);
                return PartialView("_Partials/_Pendings");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao carregar pendências: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUpcoming(int days = 7)
        {
            try
            {
                var token = GetToken();
                var upcomingPayments = await GetUpcomingPayments(token, days);
                var upcomingIncomes = await GetUpcomingIncomes(token, days);

                ViewBag.UpcomingPayments = upcomingPayments;
                ViewBag.UpcomingIncomes = upcomingIncomes;
                ViewBag.Days = days;

                return PartialView("_Partials/_Upcoming");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao carregar próximos vencimentos: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCreditCardsStatus()
        {
            try
            {
                var token = GetToken();
                var creditCards = await _creditCardService.GetAllCreditCardsAsync(token);

                return PartialView("_Partials/_CreditCardsStatus", creditCards);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao carregar status dos cartões: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetFinancingsStatus()
        {
            try
            {
                var token = GetToken();
                var financings = await _financingService.GetActiveFinancingsAsync(token);

                return PartialView("_Partials/_FinancingsStatus", financings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao carregar status dos financiamentos: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTrendAnalysis()
        {
            try
            {
                var token = GetToken();
                var currentMonth = DateTime.Now;
                var previousMonth = currentMonth.AddMonths(-1);

                var currentIncomeFilter = new IncomeFilter { Month = currentMonth.Month, Year = currentMonth.Year, Status = "Received" };
                var previousIncomeFilter = new IncomeFilter { Month = previousMonth.Month, Year = previousMonth.Year, Status = "Received" };

                var currentIncomes = await _incomeService.GetFilteredIncomesAsync(currentIncomeFilter, token);
                var previousIncomes = await _incomeService.GetFilteredIncomesAsync(previousIncomeFilter, token);

                var currentIncomeTotal = currentIncomes.Sum(i => i.Amount);
                var previousIncomeTotal = previousIncomes.Sum(i => i.Amount);

                var incomeChange = previousIncomeTotal > 0 ? ((currentIncomeTotal - previousIncomeTotal) / previousIncomeTotal) * 100 : 0;

                var currentPaymentFilter = new PaymentFilter { Month = currentMonth.Month, Year = currentMonth.Year, Status = "Paid" };
                var previousPaymentFilter = new PaymentFilter { Month = previousMonth.Month, Year = previousMonth.Year, Status = "Paid" };

                var currentPayments = await _paymentService.GetFilteredPaymentsAsync(currentPaymentFilter, token);
                var previousPayments = await _paymentService.GetFilteredPaymentsAsync(previousPaymentFilter, token);

                var currentPaymentTotal = currentPayments.Sum(p => p.Amount);
                var previousPaymentTotal = previousPayments.Sum(p => p.Amount);

                var paymentChange = previousPaymentTotal > 0
                    ? ((currentPaymentTotal - previousPaymentTotal) / previousPaymentTotal) * 100
                    : 0;

                var analysisData = new
                {
                    income = new { current = currentIncomeTotal, previous = previousIncomeTotal, change = incomeChange, trend = incomeChange > 0 ? "up" : incomeChange < 0 ? "down" : "stable" },
                    payment = new { current = currentPaymentTotal, previous = previousPaymentTotal, change = paymentChange, trend = paymentChange > 0 ? "up" : paymentChange < 0 ? "down" : "stable" },
                    balance = new { current = currentIncomeTotal - currentPaymentTotal, previous = previousIncomeTotal - previousPaymentTotal }
                };

                return Json(analysisData);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        private async Task LoadFinancialSummaryData(string token)
        {
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            var creditCards = await _creditCardService.GetAllCreditCardsAsync(token);
            var pendingPayments = await _paymentService.GetFilteredPaymentsAsync(new PaymentFilter { Status = "Pending", Month = currentMonth, Year = currentYear }, token);
            var overduePayments = await _paymentService.GetFilteredPaymentsAsync(new PaymentFilter { Status = "Overdue", Month = currentMonth, Year = currentYear }, token);
            var pendingIncomes = await _incomeService.GetFilteredIncomesAsync(new IncomeFilter { Status = "Pending", Month = currentMonth, Year = currentYear }, token);
            var overdueIncomes = await _incomeService.GetFilteredIncomesAsync(new IncomeFilter { Status = "Pending", EndDate = DateTime.Today.AddDays(-1) }, token);
            var thisMonthReceivedIncomes = await _incomeService.GetFilteredIncomesAsync(new IncomeFilter { Month = currentMonth, Year = currentYear, Status = "Received" }, token);
            var thisMonthPaidPayments = await _paymentService.GetFilteredPaymentsAsync(new PaymentFilter { Month = currentMonth, Year = currentYear, Status = "Paid" }, token);
            var allReceivedIncomes = await _incomeService.GetFilteredIncomesAsync(new IncomeFilter { Status = "Received", EndDate = DateTime.Today }, token);
            var allPaidPayments = await _paymentService.GetFilteredPaymentsAsync(new PaymentFilter { Status = "Paid", EndDate = DateTime.Today }, token);

            ViewBag.TotalBalance = allReceivedIncomes.Sum(i => i.Amount) - allPaidPayments.Sum(p => p.Amount);
            ViewBag.ThisMonthIncomes = thisMonthReceivedIncomes.Sum(i => i.Amount);
            ViewBag.ThisMonthPayments = thisMonthPaidPayments.Sum(p => p.Amount);
            ViewBag.TotalCreditCards = creditCards.Count();
            ViewBag.TotalPendingPayments = pendingPayments.Sum(p => p.Amount);
            ViewBag.TotalOverduePayments = overduePayments.Sum(p => p.Amount);
            ViewBag.TotalPendingIncomes = pendingIncomes.Sum(i => i.Amount);
            ViewBag.TotalOverdueIncomes = overdueIncomes.Sum(i => i.Amount);
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
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            var pendingPayments = await _paymentService.GetFilteredPaymentsAsync(new PaymentFilter { Status = "Pending", Month = currentMonth, Year = currentYear }, token);
            var overduePayments = await _paymentService.GetFilteredPaymentsAsync(new PaymentFilter { Status = "Overdue", Month = currentMonth, Year = currentYear }, token);
            var pendingIncomes = await _incomeService.GetFilteredIncomesAsync(new IncomeFilter { Status = "Pending", Month = currentMonth, Year = currentYear }, token);
            var overdueIncomes = await _incomeService.GetFilteredIncomesAsync(new IncomeFilter { Status = "Pending", EndDate = DateTime.Today.AddDays(-1) }, token);

            ViewBag.PendingPayments = pendingPayments;
            ViewBag.OverduePayments = overduePayments;
            ViewBag.PendingIncomes = pendingIncomes;
            ViewBag.OverdueIncomes = overdueIncomes;
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