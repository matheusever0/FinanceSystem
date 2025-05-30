using Equilibrium.Resources.Web.Enums;
using Equilibrium.Web.Interfaces;
using Equilibrium.Web.Models.Filters;
using Equilibrium.Web.Models.Generics;
using Equilibrium.Web.Models.Payment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;

namespace Equilibrium.Web.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        private const int STATUS_PAGO = 2;
        private const int STATUS_RECEBIDO = 2;
        private const int STATUS_PENDENTE = 1;
        private const int STATUS_VENCIDO = 3;
        private const int MONTHS_TO_SHOW = 6;

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

            // Dados básicos
            var creditCards = await _creditCardService.GetAllCreditCardsAsync(token);

            // Usar novos filtros para pagamentos
            var thisMonthPaymentsFilter = new PaymentFilter
            {
                Month = currentMonth,
                Year = currentYear
            };
            var thisMonthPayments = await _paymentService.GetFilteredPaymentsAsync(thisMonthPaymentsFilter, token);

            // Usar novos filtros para receitas
            var thisMonthIncomesFilter = new IncomeFilter
            {
                Month = currentMonth,
                Year = currentYear
            };
            var thisMonthIncomes = await _incomeService.GetFilteredIncomesAsync(thisMonthIncomesFilter, token);

            // Filtros específicos para pendências e vencimentos
            var pendingPayments = await _paymentService.GetPendingPaymentsAsync(token);
            var overduePayments = await _paymentService.GetOverduePaymentsAsync(token);
            var pendingIncomes = await _incomeService.GetPendingIncomesAsync(token);
            var overdueIncomes = await _incomeService.GetIncomesByMonthAsync(currentMonth, currentYear, token);

            

            // Totais financeiros
            var totalIncomeReceived = await _incomeService.GetReceivedIncomesTotalAsync(token);
            var totalPaymentsPaid = await _paymentService.GetTotalPaymentsByPeriodAsync(currentMonth, currentYear, token);
            var totalBalance = totalIncomeReceived - totalPaymentsPaid;

            // Próximos vencimentos (próximos 7 dias)
            var upcomingPayments = await GetUpcomingPayments(token, 7);
            var upcomingIncomes = await _incomeService.GetUpcomingIncomesAsync(7, token);

            // Financiamentos ativos
            var activeFinancings = await _financingService.GetActiveFinancingsAsync(token);

            // Popular ViewBag
            ViewBag.TotalBalance = totalBalance;
            ViewBag.TotalIncomeReceived = totalIncomeReceived;
            ViewBag.TotalPaymentsPaid = totalPaymentsPaid;

            ViewBag.PendingPayments = pendingPayments;
            ViewBag.OverduePayments = overduePayments;
            ViewBag.PendingIncomes = pendingIncomes;
            ViewBag.OverdueIncomes = overdueIncomes.Where(e => e.DueDate < DateTime.Now);

            ViewBag.UpcomingPayments = upcomingPayments;
            ViewBag.UpcomingIncomes = upcomingIncomes;

            ViewBag.CreditCards = creditCards;
            ViewBag.ActiveFinancings = activeFinancings;

            ViewBag.ThisMonthPayments = thisMonthPayments.Where(p => p.Status == STATUS_PAGO).Sum(p => p.Amount);
            ViewBag.ThisMonthIncomes = thisMonthIncomes.Where(i => i.Status == STATUS_RECEBIDO).Sum(i => i.Amount);

            // Dados para gráficos
            await LoadChartData(token);
        }

        private async Task LoadChartData(string token)
        {
            var monthlyData = await GetMonthlyComparisonDataAsync(token);
            ViewBag.MonthlyLabels = JsonSerializer.Serialize(monthlyData.Select(m => m.Month));
            ViewBag.MonthlyIncomeValues = JsonSerializer.Serialize(monthlyData.Select(m => m.IncomeAmount));
            ViewBag.MonthlyPaymentValues = JsonSerializer.Serialize(monthlyData.Select(m => m.PaymentAmount));

            // Dados por categoria
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            var paymentsByType = await _paymentService.GetPaymentsByTypeAsync(currentMonth, currentYear, token);
            var incomesByType = await _incomeService.GetIncomesByTypeAsync(currentMonth, currentYear, token);

            ViewBag.PaymentsByTypeLabels = JsonSerializer.Serialize(paymentsByType.Keys);
            ViewBag.PaymentsByTypeValues = JsonSerializer.Serialize(paymentsByType.Values);
            ViewBag.IncomesByTypeLabels = JsonSerializer.Serialize(incomesByType.Keys);
            ViewBag.IncomesByTypeValues = JsonSerializer.Serialize(incomesByType.Values);
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
                    var paymentFilter = new PaymentFilter
                    {
                        Month = month,
                        Year = year,
                        Status = "Paid"
                    };
                    var payments = await _paymentService.GetFilteredPaymentsAsync(paymentFilter, token);

                    var incomeFilter = new IncomeFilter
                    {
                        Month = month,
                        Year = year,
                        Status = "Received"
                    };
                    var incomes = await _incomeService.GetFilteredIncomesAsync(incomeFilter, token);

                    var monthPaymentTotal = payments.Sum(p => p.Amount);
                    var monthIncomeTotal = incomes.Sum(i => i.Amount);

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

        private async Task<List<PaymentModel>> GetUpcomingPayments(string token, int days)
        {
            var endDate = DateTime.Today.AddDays(days);
            var filter = new PaymentFilter
            {
                StartDate = DateTime.Today,
                EndDate = endDate,
                Status = "Pending",
                OrderBy = "dueDate",
                Ascending = true
            };

            var payments = await _paymentService.GetFilteredPaymentsAsync(filter, token);
            return payments.ToList();
        }

        // Endpoints AJAX para carregamento dinâmico
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
                var upcomingIncomes = await _incomeService.GetUpcomingIncomesAsync(days, token);

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

        // Método para análise rápida de tendências
        [HttpGet]
        public async Task<IActionResult> GetTrendAnalysis()
        {
            try
            {
                var token = GetToken();
                var currentMonth = DateTime.Now;
                var previousMonth = currentMonth.AddMonths(-1);

                // Comparar receitas
                var currentIncomeComparison = await _incomeService.ComparePeriodsAsync(
                    new DateTime(currentMonth.Year, currentMonth.Month, 1),
                    new DateTime(currentMonth.Year, currentMonth.Month, DateTime.DaysInMonth(currentMonth.Year, currentMonth.Month)),
                    token);

                // Comparar pagamentos 
                var currentPaymentFilter = new PaymentFilter
                {
                    Month = currentMonth.Month,
                    Year = currentMonth.Year,
                    Status = "Paid"
                };
                var previousPaymentFilter = new PaymentFilter
                {
                    Month = previousMonth.Month,
                    Year = previousMonth.Year,
                    Status = "Paid"
                };

                var currentPayments = await _paymentService.GetFilteredPaymentsAsync(currentPaymentFilter, token);
                var previousPayments = await _paymentService.GetFilteredPaymentsAsync(previousPaymentFilter, token);

                var currentPaymentTotal = currentPayments.Sum(p => p.Amount);
                var previousPaymentTotal = previousPayments.Sum(p => p.Amount);

                var paymentChange = previousPaymentTotal > 0
                    ? ((currentPaymentTotal - previousPaymentTotal) / previousPaymentTotal) * 100
                    : 0;

                var analysisData = new
                {
                    income = new
                    {
                        current = currentIncomeComparison.currentPeriod,
                        previous = currentIncomeComparison.previousPeriod,
                        change = currentIncomeComparison.percentageChange,
                        trend = currentIncomeComparison.percentageChange > 0 ? "up" :
                                currentIncomeComparison.percentageChange < 0 ? "down" : "stable"
                    },
                    payment = new
                    {
                        current = currentPaymentTotal,
                        previous = previousPaymentTotal,
                        change = paymentChange,
                        trend = paymentChange > 0 ? "up" : paymentChange < 0 ? "down" : "stable"
                    },
                    balance = new
                    {
                        current = currentIncomeComparison.currentPeriod - currentPaymentTotal,
                        previous = currentIncomeComparison.previousPeriod - previousPaymentTotal
                    }
                };

                return Json(analysisData);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // Métodos auxiliares privados (mantidos para compatibilidade)
        private async Task LoadFinancialSummaryData(string token)
        {
            var currentDate = DateTime.Now;
            var currentMonth = currentDate.Month;
            var currentYear = currentDate.Year;

            var creditCards = await _creditCardService.GetAllCreditCardsAsync(token);

            var pendingIncomes = await _incomeService.GetPendingIncomesAsync(token);
            var pendingPayments = await _paymentService.GetPendingPaymentsAsync(token);
            var overduePayments = await _paymentService.GetOverduePaymentsAsync(token);
            var overdueIncomes = await _incomeService.GetOverdueIncomesAsync(token);

            var totalIncome = await _incomeService.GetReceivedIncomesTotalAsync(token);
            var totalPayments = await _paymentService.GetTotalPaymentsByPeriodAsync(currentMonth, currentYear, token);
            var totalBalance = totalIncome - totalPayments;

            ViewBag.TotalBalance = totalBalance;
            ViewBag.PendingPayments = pendingPayments;
            ViewBag.OverduePayments = overduePayments;
            ViewBag.OverdueIncomes = overdueIncomes;
            ViewBag.PendingIncomes = pendingIncomes;
            ViewBag.CreditCards = creditCards;
            ViewBag.ThisMonthIncomes = await _incomeService.GetTotalIncomesByPeriodAsync(currentMonth, currentYear, token);
            ViewBag.ThisMonthPayments = totalPayments;
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