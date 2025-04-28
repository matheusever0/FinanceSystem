using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceSystem.Application.DTOs.Financing
{
    public class FinancingForecastInstallmentDto
    {
        public int Number { get; set; }
        public DateTime DueDate { get; set; }
        public decimal OriginalAmount { get; set; }
        public decimal ProjectedAmount { get; set; }
        public decimal Difference { get; set; }
        public decimal DifferencePercentage { get; set; }
    }
}
