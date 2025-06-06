using Equilibrium.Domain.Entities;
using Equilibrium.Domain.Enums;
using Equilibrium.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace Equilibrium.Infrastructure.Services
{
    public class RecurrenceService : IRecurrenceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RecurrenceService> _logger;

        public RecurrenceService(
            IUnitOfWork unitOfWork,
            ILogger<RecurrenceService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task ProcessRecurringPaymentsAsync()
        {
            try
            {
                _logger.LogInformation("Processing recurring payments...");

                var recurringPayments = await _unitOfWork.Payments.GetRecurringPaymentsWithDetailsAsync();
                var activeRecurringPayments = recurringPayments
                    .Where(p => p.Status != PaymentStatus.Cancelled)
                    .ToList();

                foreach (var payment in activeRecurringPayments)
                {
                    var nextMonth = payment.DueDate.AddMonths(1);
                    var firstDayOfNextMonth = new DateTime(nextMonth.Year, nextMonth.Month, 1);
                    var lastDayOfNextMonth = firstDayOfNextMonth.AddMonths(1).AddDays(-1);

                    var existingPayments = await _unitOfWork.Payments.GetPaymentsByPeriodAndDetailsAsync(
                        payment.UserId,
                        payment.PaymentTypeId,
                        payment.Description,
                        firstDayOfNextMonth,
                        lastDayOfNextMonth);

                    if (!existingPayments.Any())
                    {
                        var nextDueDate = new DateTime(
                            nextMonth.Year,
                            nextMonth.Month,
                            Math.Min(payment.DueDate.Day, DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month))
                        );

                        var newPayment = new Payment(
                            payment.Description,
                            payment.Amount,
                            nextDueDate,
                            payment.PaymentType,
                            payment.PaymentMethod,
                            payment.User);

                        newPayment.SetRecurring(payment.IsRecurring);
                        newPayment.SetNotes(payment.Notes);

                        await _unitOfWork.Payments.AddAsync(newPayment);
                        _logger.LogInformation("Created recurring payment for {NextDueDate:yyyy-MM-dd} - {Description}", nextDueDate, payment.Description);
                    }
                }

                await _unitOfWork.CompleteAsync();
                _logger.LogInformation("Recurring payments processing completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing recurring payments");
                throw new InvalidOperationException("Error processing recurring payments", ex);
            }
        }

        public async Task ProcessRecurringIncomesAsync()
        {
            try
            {
                _logger.LogInformation("Processing recurring incomes...");

                var recurringIncomes = await _unitOfWork.Incomes.GetRecurringIncomesWithDetailsAsync();
                var activeRecurringIncomes = recurringIncomes
                    .Where(i => i.Status != IncomeStatus.Cancelled)
                    .ToList();

                foreach (var income in activeRecurringIncomes)
                {
                    var nextMonth = income.DueDate.AddMonths(1);
                    var firstDayOfNextMonth = new DateTime(nextMonth.Year, nextMonth.Month, 1);
                    var lastDayOfNextMonth = firstDayOfNextMonth.AddMonths(1).AddDays(-1);

                    var existingIncomes = await _unitOfWork.Incomes.GetIncomesByPeriodAndDetailsAsync(
                        income.UserId,
                        income.IncomeTypeId,
                        income.Description,
                        firstDayOfNextMonth,
                        lastDayOfNextMonth);

                    if (!existingIncomes.Any())
                    {
                        var nextDueDate = new DateTime(
                            nextMonth.Year,
                            nextMonth.Month,
                            Math.Min(income.DueDate.Day, DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month))
                        );

                        var newIncome = new Income(
                            income.Description,
                            income.Amount,
                            nextDueDate,
                            income.IncomeType,
                            income.User,
                            income.IsRecurring,
                            income.Notes
                        );

                        await _unitOfWork.Incomes.AddAsync(newIncome);
                        _logger.LogInformation("Created recurring income for {NextDueDate:yyyy-MM-dd} - {Description}", nextDueDate, income.Description);
                    }
                }

                await _unitOfWork.CompleteAsync();
                _logger.LogInformation("Recurring incomes processing completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing recurring incomes");
                throw new InvalidOperationException("Error processing recurring incomes", ex);
            }
        }
    }
}