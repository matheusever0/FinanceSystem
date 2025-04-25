using FinanceSystem.Resources.Web.Enums;

namespace FinanceSystem.Resources.Web.Helpers
{
    public static class MessageHelper
    {
        // Erros de carregamento
        public static string GetLoadingErrorMessage(EntityNames entity, Exception ex)
        {
            string entityName = EntityNameHelper.GetEntityName(entity);
            return string.Format(ResourceFinanceWeb.Error_Loading, entityName, ex.Message);
        }

        // Erros de criação
        public static string GetCreationErrorMessage(EntityNames entity, Exception ex)
        {
            string entityName = EntityNameHelper.GetEntityName(entity);
            return string.Format(ResourceFinanceWeb.Error_Creating, entityName, ex.Message);
        }

        // Erros de atualização
        public static string GetUpdateErrorMessage(EntityNames entity, Exception ex)
        {
            string entityName = EntityNameHelper.GetEntityName(entity);
            return string.Format(ResourceFinanceWeb.Error_Updating, entityName, ex.Message);
        }

        // Erros de exclusão
        public static string GetDeletionErrorMessage(EntityNames entity, Exception ex)
        {
            string entityName = EntityNameHelper.GetEntityName(entity);
            return string.Format(ResourceFinanceWeb.Error_Deleting, entityName, ex.Message);
        }

        // Erros de marcação de status
        public static string GetStatusChangeErrorMessage(EntityNames entity, EntityStatus status, Exception ex)
        {
            string entityName = EntityNameHelper.GetEntityName(entity);
            string statusName = EntityNameHelper.GetStatusName(status);
            return string.Format(ResourceFinanceWeb.Error_MarkStatus, entityName, statusName, ex.Message);
        }

        // Erros de cancelamento
        public static string GetCancelErrorMessage(EntityNames entity, Exception ex)
        {
            string entityName = EntityNameHelper.GetEntityName(entity);
            return string.Format(ResourceFinanceWeb.Error_CancelEntity, entityName, ex.Message);
        }

        // Erros de geração de relatório
        public static string GetReportGenerationErrorMessage(ReportType type, Exception ex)
        {
            string typeName = EntityNameHelper.GetReportTypeName(type);
            return string.Format(ResourceFinanceWeb.Error_Generating, typeName, ex.Message);
        }

        // Entity não encontrada na relação com parcela
        public static string GetParentEntityNotFoundMessage(EntityNames entity)
        {
            string entityName = EntityNameHelper.GetEntityName(entity);
            return string.Format(ResourceFinanceWeb.Error_ParentEntityNotFound, entityName);
        }

        // Não pode editar entity do sistema
        public static string GetCannotEditSystemEntityMessage(EntityNames entity)
        {
            string entityName = EntityNameHelper.GetEntityName(entity);
            return string.Format(ResourceFinanceWeb.Error_CannotEditSystemEntity, entityName);
        }

        // Não pode excluir entity do sistema
        public static string GetCannotDeleteSystemEntityMessage(EntityNames entity)
        {
            string entityName = EntityNameHelper.GetEntityName(entity);
            return string.Format(ResourceFinanceWeb.Error_CannotDeleteSystemEntity, entityName);
        }

        // Mensagens de sucesso
        public static string GetCreationSuccessMessage(EntityNames entity)
        {
            string entityName = EntityNameHelper.GetEntityName(entity);
            return string.Format(ResourceFinanceWeb.Success_Created, entityName);
        }

        public static string GetUpdateSuccessMessage(EntityNames entity)
        {
            string entityName = EntityNameHelper.GetEntityName(entity);
            return string.Format(ResourceFinanceWeb.Success_Updated, entityName);
        }

        public static string GetDeletionSuccessMessage(EntityNames entity)
        {
            string entityName = EntityNameHelper.GetEntityName(entity);
            return string.Format(ResourceFinanceWeb.Success_Deleted, entityName);
        }

        public static string GetStatusChangeSuccessMessage(EntityNames entity, EntityStatus status)
        {
            string entityName = EntityNameHelper.GetEntityName(entity);
            string statusName = EntityNameHelper.GetStatusName(status);
            return string.Format(ResourceFinanceWeb.Success_StatusChanged, entityName, statusName);
        }

        public static string GetCancelSuccessMessage(EntityNames entity)
        {
            string entityName = EntityNameHelper.GetEntityName(entity);
            return string.Format(ResourceFinanceWeb.Success_Canceled, entityName);
        }
    }
}
