using Equilibrium.Web.Models.Generics;
using Equilibrium.Web.Models.Income;
using Equilibrium.Web.Models.Payment;
using System.Text.Json;

namespace Equilibrium.Web.Models.ViewModels
{
    public class MonthlyReportViewModel
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public string MonthName => new DateTime(Year, Month, 1).ToString("MMMM");

        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal PendingAmount { get; set; }
        public decimal OverdueAmount { get; set; }

        public decimal TotalIncomeAmount { get; set; }
        public decimal ReceivedIncomeAmount { get; set; }
        public decimal PendingIncomeAmount { get; set; }
        public decimal OverdueIncomeAmount { get; set; }

        public List<PaymentModel> Payments { get; set; } = new List<PaymentModel>();
        public List<IncomeModel> Incomes { get; set; } = new List<IncomeModel>();
        public List<PaymentByTypeDto> PaymentsByType { get; set; } = new List<PaymentByTypeDto>();
        public List<PaymentByTypeDto> IncomesByType { get; set; } = new List<PaymentByTypeDto>();

        public decimal Balance => TotalIncomeAmount - TotalAmount;

        // Navegação entre meses
        public int PreviousMonth => Month == 1 ? 12 : Month - 1;
        public int PreviousYear => Month == 1 ? Year - 1 : Year;
        public int NextMonth => Month == 12 ? 1 : Month + 1;
        public int NextYear => Month == 12 ? Year + 1 : Year;

        private JsonSerializerOptions JsonOptions => new JsonSerializerOptions
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public string TypeLabels => JsonSerializer.Serialize(PaymentsByType.Select(p => p.TypeName).ToList(), JsonOptions);
        public string TypeValues => JsonSerializer.Serialize(PaymentsByType.Select(p => p.TotalAmount).ToList(), JsonOptions);

        public string IncomeTypeLabels => JsonSerializer.Serialize(IncomesByType.Select(p => p.TypeName).ToList(), JsonOptions);
        public string IncomeTypeValues => JsonSerializer.Serialize(IncomesByType.Select(p => p.TotalAmount).ToList(), JsonOptions);

        public string StatusLabels => JsonSerializer.Serialize(new[] { "Pago", "Pendente", "Vencido", "Cancelado" }, JsonOptions);
        public string StatusValues => JsonSerializer.Serialize(new[] { PaidAmount, PendingAmount, OverdueAmount, 0.0m }, JsonOptions);

        public string IncomeStatusLabels => JsonSerializer.Serialize(new[] { "Recebido", "Pendente", "Vencido", "Cancelado" }, JsonOptions);
        public string IncomeStatusValues => JsonSerializer.Serialize(new[] { ReceivedIncomeAmount, PendingIncomeAmount, OverdueIncomeAmount, 0.0m }, JsonOptions);

        public string MonthlyComparisonLabels => JsonSerializer.Serialize(new[] { "Receitas", "Despesas" }, JsonOptions);
        public string MonthlyComparisonIncomeValues => JsonSerializer.Serialize(new decimal[] { TotalIncomeAmount }, JsonOptions);
        public string MonthlyComparisonExpenseValues => JsonSerializer.Serialize(new decimal[] { TotalAmount }, JsonOptions);
    }
}