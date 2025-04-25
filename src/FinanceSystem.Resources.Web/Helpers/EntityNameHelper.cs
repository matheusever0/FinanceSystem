using FinanceSystem.Resources.Web.Enums;

namespace FinanceSystem.Resources.Web.Helpers
{
    public static class EntityNameHelper
    {
        private static readonly Dictionary<EntityNames, string> _entityNames = new()
        {
            { EntityNames.Payment, "pagamento" },
            { EntityNames.Payments, "pagamentos" },
            { EntityNames.PaymentInstallment, "parcela" },
            { EntityNames.Income, "receita" },
            { EntityNames.Incomes, "receitas" },
            { EntityNames.IncomeInstallment, "parcela" },
            { EntityNames.PaymentType, "tipo de pagamento" },
            { EntityNames.IncomeType, "tipo de receita" },
            { EntityNames.PaymentMethod, "método de pagamento" },
            { EntityNames.CreditCard, "cartão de crédito" },
            { EntityNames.User, "usuário" },
            { EntityNames.Role, "perfil" },
            { EntityNames.Permission, "permissão" },
            { EntityNames.Investment, "investimento" },
            { EntityNames.Transaction, "transação" },
            { EntityNames.Report, "relatório" }
        };

        private static readonly Dictionary<EntityStatus, string> _entityStatus = new()
        {
            { EntityStatus.Paid, "pago" },
            { EntityStatus.Overdue, "vencido" },
            { EntityStatus.Received, "recebido" },
            { EntityStatus.Pending, "pendente" }
        };

        private static readonly Dictionary<ReportType, string> _reportTypes = new()
        {
            { ReportType.Monthly, "relatório mensal" },
            { ReportType.Annual, "relatório anual" },
            { ReportType.CreditCards, "relatório de cartões de crédito" },
            { ReportType.Print, "relatório para impressão" }
        };

        public static string GetEntityName(EntityNames entity)
        {
            return _entityNames.TryGetValue(entity, out var name) ? name : entity.ToString().ToLower();
        }

        public static string GetStatusName(EntityStatus status)
        {
            return _entityStatus.TryGetValue(status, out var name) ? name : status.ToString().ToLower();
        }

        public static string GetReportTypeName(ReportType type)
        {
            return _reportTypes.TryGetValue(type, out var name) ? name : type.ToString().ToLower();
        }
    }
}
