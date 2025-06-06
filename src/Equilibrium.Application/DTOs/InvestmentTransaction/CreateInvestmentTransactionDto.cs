﻿using Equilibrium.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Equilibrium.Application.DTOs.InvestmentTransaction
{
    public class CreateInvestmentTransactionDto
    {
        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required]
        public TransactionType Type { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Taxes { get; set; }

        [StringLength(50)]
        public required string Broker { get; set; }

        [StringLength(500)]
        public required string Notes { get; set; }
    }
}
