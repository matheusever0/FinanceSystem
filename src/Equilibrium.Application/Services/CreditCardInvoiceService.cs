// src/Equilibrium.Application/Services/CreditCardInvoiceService.cs
using AutoMapper;
using Equilibrium.Application.DTOs.CreditCard;
using Equilibrium.Application.DTOs.Payment;
using Equilibrium.Application.Interfaces;
using Equilibrium.Domain.Entities;
using Equilibrium.Domain.Interfaces.Services;
using Equilibrium.Resources;

namespace Equilibrium.Application.Services
{
    public class CreditCardInvoiceService : ICreditCardInvoiceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreditCardInvoiceService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<CreditCardInvoiceDto> GetCurrentInvoiceAsync(Guid creditCardId)
        {
            var creditCard = await _unitOfWork.CreditCards.GetByIdAsync(creditCardId)
                ?? throw new KeyNotFoundException(ResourceFinanceApi.CreditCard_NotFound);

            var now = DateTime.Now;
            var closingDate = CalculateClosingDate(now, creditCard.ClosingDay);
            var previousClosingDate = closingDate.AddMonths(-1);

            return await BuildInvoiceAsync(creditCard, previousClosingDate, closingDate);
        }

        public async Task<CreditCardInvoiceDto> GetInvoiceByPeriodAsync(Guid creditCardId, DateTime startDate, DateTime endDate)
        {
            var creditCard = await _unitOfWork.CreditCards.GetByIdAsync(creditCardId)
                ?? throw new KeyNotFoundException(ResourceFinanceApi.CreditCard_NotFound);

            return await BuildInvoiceAsync(creditCard, startDate, endDate);
        }

        public async Task<IEnumerable<CreditCardInvoiceDto>> GetInvoiceHistoryAsync(Guid creditCardId, int months = 12)
        {
            var creditCard = await _unitOfWork.CreditCards.GetByIdAsync(creditCardId)
                ?? throw new KeyNotFoundException(ResourceFinanceApi.CreditCard_NotFound);

            var invoices = new List<CreditCardInvoiceDto>();
            var currentDate = DateTime.Now;

            for (int i = 0; i < months; i++)
            {
                var referenceMonth = currentDate.AddMonths(-i);
                var closingDate = CalculateClosingDate(referenceMonth, creditCard.ClosingDay);
                var previousClosingDate = closingDate.AddMonths(-1);

                var invoice = await BuildInvoiceAsync(creditCard, previousClosingDate, closingDate);
                invoices.Add(invoice);
            }

            return invoices.OrderByDescending(i => i.ReferenceDate);
        }

        public async Task<CreditCardDto> PayInvoiceAsync(Guid creditCardId, PayInvoiceDto payInvoiceDto)
        {
            var creditCard = await _unitOfWork.CreditCards.GetByIdAsync(creditCardId)
                ?? throw new KeyNotFoundException(ResourceFinanceApi.CreditCard_NotFound);

            var user = await _unitOfWork.Users.GetByIdAsync(creditCard.UserId)
                ?? throw new KeyNotFoundException(ResourceFinanceApi.User_NotFound);

            // Se PayFullAmount for true, calcular o valor total da fatura atual
            decimal paymentAmount = payInvoiceDto.Amount;

            if (payInvoiceDto.PayFullAmount)
            {
                var currentInvoice = await GetCurrentInvoiceAsync(creditCardId);
                paymentAmount = currentInvoice.RemainingAmount;
            }

            if (paymentAmount <= 0)
                throw new InvalidOperationException("Payment amount must be greater than zero");

            // Verificar se o valor não excede o valor usado
            var usedLimit = creditCard.Limit - creditCard.AvailableLimit;
            if (paymentAmount > usedLimit)
                paymentAmount = usedLimit;

            // Criar registro de pagamento da fatura
            var payment = new CreditCardPayment(
                paymentAmount,
                payInvoiceDto.PaymentDate,
                creditCard,
                user,
                payInvoiceDto.Notes
            );

            await _unitOfWork.CreditCardPayments.AddAsync(payment);

            // Liberar o limite no cartão
            creditCard.IncrementAvailableLimit(paymentAmount);
            await _unitOfWork.CreditCards.UpdateAsync(creditCard);

            await _unitOfWork.CompleteAsync();

            return _mapper.Map<CreditCardDto>(creditCard);
        }

        public async Task<CreditCardInvoiceDetailDto> GetInvoiceDetailsAsync(Guid creditCardId, DateTime referenceDate)
        {
            var creditCard = await _unitOfWork.CreditCards.GetByIdAsync(creditCardId)
                ?? throw new KeyNotFoundException(ResourceFinanceApi.CreditCard_NotFound);

            var closingDate = CalculateClosingDate(referenceDate, creditCard.ClosingDay);
            var previousClosingDate = closingDate.AddMonths(-1);

            // Buscar transações do período
            var payments = await _unitOfWork.Payments.FindAsync(p =>
                p.CreditCardId == creditCardId &&
                p.DueDate >= previousClosingDate &&
                p.DueDate < closingDate);

            // Buscar pagamentos de fatura do período
            var invoicePayments = await _unitOfWork.CreditCardPayments.GetPaymentsByPeriodAsync(
                creditCardId, previousClosingDate, closingDate);

            var invoice = await BuildInvoiceAsync(creditCard, previousClosingDate, closingDate);

            var detailDto = _mapper.Map<CreditCardInvoiceDetailDto>(invoice);
            detailDto.Transactions = _mapper.Map<List<PaymentDto>>(payments);
            detailDto.Payments = _mapper.Map<List<InvoicePaymentDto>>(invoicePayments);

            return detailDto;
        }

        private async Task<CreditCardInvoiceDto> BuildInvoiceAsync(CreditCard creditCard, DateTime startDate, DateTime endDate)
        {
            // Buscar transações do período
            var payments = await _unitOfWork.Payments.FindAsync(p =>
                p.CreditCardId == creditCard.Id &&
                p.DueDate >= startDate &&
                p.DueDate < endDate);

            // Buscar pagamentos de fatura do período
            var totalPaid = await _unitOfWork.CreditCardPayments.GetTotalPaidInPeriodAsync(
                creditCard.Id, startDate, endDate);

            var totalTransactions = payments.Sum(p => p.Amount);
            var dueDate = CalculateDueDate(endDate, creditCard.DueDay);

            return new CreditCardInvoiceDto
            {
                CreditCardId = creditCard.Id,
                CreditCardName = creditCard.Name,
                ReferenceDate = endDate,
                ClosingDate = endDate,
                DueDate = dueDate,
                TotalAmount = totalTransactions,
                PaidAmount = totalPaid,
                RemainingAmount = Math.Max(0, totalTransactions - totalPaid),
                IsPaid = totalPaid >= totalTransactions,
                IsOverdue = dueDate < DateTime.Now && totalPaid < totalTransactions,
                TransactionCount = payments.Count(),
                AvailableLimit = creditCard.AvailableLimit,
                UsedLimit = creditCard.Limit - creditCard.AvailableLimit,
                TotalLimit = creditCard.Limit
            };
        }

        private DateTime CalculateClosingDate(DateTime referenceDate, int closingDay)
        {
            var year = referenceDate.Year;
            var month = referenceDate.Month;

            // Garantir que o dia de fechamento não exceda os dias do mês
            var daysInMonth = DateTime.DaysInMonth(year, month);
            var actualClosingDay = Math.Min(closingDay, daysInMonth);

            var closingDate = new DateTime(year, month, actualClosingDay);

            // Se a data de referência é antes do fechamento, usar o fechamento do mês anterior
            if (referenceDate.Day < actualClosingDay)
            {
                closingDate = closingDate.AddMonths(-1);
            }

            return closingDate;
        }

        private DateTime CalculateDueDate(DateTime closingDate, int dueDay)
        {
            var nextMonth = closingDate.AddMonths(1);
            var daysInMonth = DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month);
            var actualDueDay = Math.Min(dueDay, daysInMonth);

            return new DateTime(nextMonth.Year, nextMonth.Month, actualDueDay);
        }
    }
}