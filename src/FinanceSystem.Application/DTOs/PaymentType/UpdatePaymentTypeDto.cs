﻿using System.ComponentModel.DataAnnotations;

namespace FinanceSystem.Application.DTOs.PaymentType
{
    public class UpdatePaymentTypeDto
    {
        [StringLength(50, MinimumLength = 3)]
        public string Name { get; set; }

        [StringLength(200)]
        public string Description { get; set; }
    }
}
