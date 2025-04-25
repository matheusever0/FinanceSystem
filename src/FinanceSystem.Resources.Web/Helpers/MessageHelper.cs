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

            return char.ToUpper(message[0]) + message.Substring(1).ToLower();
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
            return CapitalizeFirst(string.Format(ResourceFinanceWeb.Success_Created, entityName));
        }

        public static string GetUpdateSuccessMessage(EntityNames entity)
        {
            string entityName = EntityNameHelper.GetEntityName(entity);
            return CapitalizeFirst(string.Format(ResourceFinanceWeb.Success_Updated, entityName));
        }

        public static string GetDeletionSuccessMessage(EntityNames entity)
        {
            string entityName = EntityNameHelper.GetEntityName(entity);
            return CapitalizeFirst(string.Format(ResourceFinanceWeb.Success_Deleted, entityName));
        }

        public static string GetStatusChangeSuccessMessage(EntityNames entity, EntityStatus status)
        {
            string entityName = EntityNameHelper.GetEntityName(entity);
            string statusName = EntityNameHelper.GetStatusName(status);
            return CapitalizeFirst(string.Format(ResourceFinanceWeb.Success_StatusChanged, entityName, statusName));
        }

        public static string GetCancelSuccessMessage(EntityNames entity)
        {
            string entityName = EntityNameHelper.GetEntityName(entity);
            return CapitalizeFirst(string.Format(ResourceFinanceWeb.Success_Canceled, entityName));
        }
    }
}
