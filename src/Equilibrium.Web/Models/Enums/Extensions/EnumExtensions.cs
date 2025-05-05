namespace Equilibrium.Web.Models.Enums.Extensions
{
    public static class EnumExtensions
    {
        public static string GetStatusDescription(this StatusType status)
        {
            return status switch
            {
                StatusType.Pending => "Pendente",
                StatusType.Completed => "Concluído",
                StatusType.Canceled => "Cancelado",
                StatusType.Overdue => "Vencido",
                StatusType.Inactive => "Inativo",
                _ => "Desconhecido"
            };
        }

        public static string GetStatusBadgeClass(this StatusType status)
        {
            return status switch
            {
                StatusType.Pending => "bg-warning",
                StatusType.Completed => "bg-success",
                StatusType.Canceled => "bg-secondary",
                StatusType.Overdue => "bg-danger",
                StatusType.Inactive => "bg-secondary",
                _ => "bg-primary"
            };
        }

        public static string GetPaymentMethodDescription(this PaymentMethodType type)
        {
            return type switch
            {
                PaymentMethodType.Cash => "Dinheiro",
                PaymentMethodType.CreditCard => "Cartão de Crédito",
                PaymentMethodType.DebitCard => "Cartão de Débito",
                PaymentMethodType.BankTransfer => "Transferência Bancária",
                PaymentMethodType.DigitalWallet => "Carteira Digital",
                PaymentMethodType.Check => "Cheque",
                PaymentMethodType.Other => "Outro",
                _ => "Desconhecido"
            };
        }

        public static string GetFinancingTypeDescription(this FinancingType type)
        {
            return type switch
            {
                FinancingType.Price => "PRICE",
                FinancingType.Sac => "SAC",
                _ => "Desconhecido"
            };
        }

        public static string GetCorrectionIndexDescription(this CorrectionIndexType type)
        {
            return type switch
            {
                CorrectionIndexType.None => "Sem correção",
                CorrectionIndexType.Ipca => "IPCA",
                CorrectionIndexType.Igpm => "IGPM",
                CorrectionIndexType.Tr => "TR",
                CorrectionIndexType.Cdi => "CDI",
                _ => "Desconhecido"
            };
        }

        public static string GetInvestmentTypeDescription(this InvestmentType type)
        {
            return type switch
            {
                InvestmentType.Stock => "Ações",
                InvestmentType.RealEstateFund => "Fundos Imobiliários",
                InvestmentType.Etf => "ETFs",
                InvestmentType.ForeignStock => "Ações Estrangeiras",
                InvestmentType.FixedIncome => "Renda Fixa",
                _ => "Não Categorizado"
            };
        }

        public static string GetTransactionTypeDescription(this TransactionType type)
        {
            return type switch
            {
                TransactionType.Purchase => "Compra",
                TransactionType.Sale => "Venda",
                TransactionType.Dividend => "Dividendo",
                TransactionType.Split => "Split",
                TransactionType.Bonus => "Bonificação",
                TransactionType.InterestOnCapital => "JCP",
                TransactionType.Income => "Rendimento",
                _ => "Outros"
            };
        }

        public static string GetProgressBarClass(decimal percentage)
        {
            if (percentage >= 0.9m) return "bg-success";
            if (percentage >= 0.6m) return "bg-info";
            if (percentage >= 0.3m) return "bg-warning";
            return "bg-danger";
        }
    }
}