using FinanceSystem.Resources.Web.Enums;

namespace FinanceSystem.Resources.Web.Helpers
{
    public static class MessageHelper
    {
        private static string CapitalizeFirst(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return message;

            message = message.Trim();

            if (message.Length == 1)
                return message.ToUpper();

            return char.ToUpper(message[0]) + message.Substring(1);
        }

        private static GeneroGramatical DetectarGenero(string nomeEntidade)
        {
            if (string.IsNullOrWhiteSpace(nomeEntidade))
                return GeneroGramatical.Masculino;

            nomeEntidade = nomeEntidade.Trim().ToLowerInvariant();

            if (nomeEntidade.EndsWith("a"))
                return GeneroGramatical.Feminino;

            return GeneroGramatical.Masculino;
        }

        private static string GetMensagemComGenero(string entidade, string verboBase)
        {
            var genero = DetectarGenero(entidade);
            var verbo = genero == GeneroGramatical.Feminino ? $"{verboBase}a" : $"{verboBase}o";
            return CapitalizeFirst($"{entidade} {verbo} com sucesso");
        }


        public static string GetLoadingErrorMessage(EntityNames entity, Exception ex)
        {
            string entityName = EntityNameHelper.GetEntityName(entity);
            return CapitalizeFirst(string.Format(ResourceFinanceWeb.Error_Loading, entityName, ex.Message));
        }

        public static string GetCreationErrorMessage(EntityNames entity, Exception ex)
        {
            string entityName = EntityNameHelper.GetEntityName(entity);
            return CapitalizeFirst(string.Format(ResourceFinanceWeb.Error_Creating, entityName, ex.Message));
        }

        public static string GetUpdateErrorMessage(EntityNames entity, Exception ex)
        {
            string entityName = EntityNameHelper.GetEntityName(entity);
            return CapitalizeFirst(string.Format(ResourceFinanceWeb.Error_Updating, entityName, ex.Message));
        }

        public static string GetDeletionErrorMessage(EntityNames entity, Exception ex)
        {
            string entityName = EntityNameHelper.GetEntityName(entity);
            return CapitalizeFirst(string.Format(ResourceFinanceWeb.Error_Deleting, entityName, ex.Message));
        }

        public static string GetStatusChangeErrorMessage(EntityNames entity, EntityStatus status, Exception ex)
        {
            string entityName = EntityNameHelper.GetEntityName(entity);
            string statusName = EntityNameHelper.GetStatusName(status);
            return CapitalizeFirst(string.Format(ResourceFinanceWeb.Error_MarkStatus, entityName, statusName, ex.Message));
        }

        public static string GetCancelErrorMessage(EntityNames entity, Exception ex)
        {
            string entityName = EntityNameHelper.GetEntityName(entity);
            return CapitalizeFirst(string.Format(ResourceFinanceWeb.Error_CancelEntity, entityName, ex.Message));
        }

        public static string GetReportGenerationErrorMessage(ReportType type, Exception ex)
        {
            string typeName = EntityNameHelper.GetReportTypeName(type);
            return CapitalizeFirst(string.Format(ResourceFinanceWeb.Error_Generating, typeName, ex.Message));
        }

        public static string GetParentEntityNotFoundMessage(EntityNames entity)
        {
            string entityName = EntityNameHelper.GetEntityName(entity);
            return CapitalizeFirst(string.Format(ResourceFinanceWeb.Error_ParentEntityNotFound, entityName));
        }

        public static string GetCannotEditSystemEntityMessage(EntityNames entity)
        {
            string entityName = EntityNameHelper.GetEntityName(entity);
            return CapitalizeFirst(string.Format(ResourceFinanceWeb.Error_CannotEditSystemEntity, entityName));
        }

        public static string GetCannotDeleteSystemEntityMessage(EntityNames entity)
        {
            string entityName = EntityNameHelper.GetEntityName(entity);
            return CapitalizeFirst(string.Format(ResourceFinanceWeb.Error_CannotDeleteSystemEntity, entityName));
        }

        public static string GetCreationSuccessMessage(EntityNames entity)
        {
            string entityName = EntityNameHelper.GetEntityName(entity);
            return GetMensagemComGenero(entityName, "criad");
        }

        public static string GetUpdateSuccessMessage(EntityNames entity)
        {
            string entityName = EntityNameHelper.GetEntityName(entity);
            return GetMensagemComGenero(entityName, "atualizad");
        }

        public static string GetDeletionSuccessMessage(EntityNames entity)
        {
            string entityName = EntityNameHelper.GetEntityName(entity);
            return GetMensagemComGenero(entityName, "excluíd");
        }

        public static string GetCancelSuccessMessage(EntityNames entity)
        {
            string entityName = EntityNameHelper.GetEntityName(entity);
            return GetMensagemComGenero(entityName, "cancelad");
        }

        public static string GetStatusChangeSuccessMessage(EntityNames entity, EntityStatus status)
        {
            string entityName = EntityNameHelper.GetEntityName(entity);
            string statusName = EntityNameHelper.GetStatusName(status);
            return CapitalizeFirst(string.Format(ResourceFinanceWeb.Success_StatusChanged, entityName, statusName));
        }
    }
}
