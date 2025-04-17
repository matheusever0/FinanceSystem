﻿using FinanceSystem.Domain.Enums;

namespace FinanceSystem.Application.DTOs
{
    public class IncomeDto
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public IncomeStatus Status { get; set; }
        public string StatusDescription => Status.ToString();
        public bool IsRecurring { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Guid UserId { get; set; }
        public Guid IncomeTypeId { get; set; }
        public string IncomeTypeName { get; set; }

        public List<IncomeInstallmentDto> Installments { get; set; } = new List<IncomeInstallmentDto>();
    }
}