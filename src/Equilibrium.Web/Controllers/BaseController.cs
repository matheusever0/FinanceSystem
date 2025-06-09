using Equilibrium.Resources.Web.Enums;
using Equilibrium.Resources.Web.Helpers;
using Equilibrium.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Equilibrium.Web.Controllers
{
    [Authorize]
    public abstract class BaseController : Controller
    {
        protected string GetToken()
        {
            return HttpContext.GetJwtToken();
        }

        protected string GetCurrentUserId()
        {
            return HttpContext.GetCurrentUserId() ?? string.Empty;
        }

        protected string GetCurrentUserName()
        {
            return HttpContext.GetUserName();
        }

        protected bool IsUserAuthenticated()
        {
            return HttpContext.IsUserAuthenticated();
        }

        protected void SetSuccessMessage(EntityNames entity, string action)
        {
            var message = action.ToLower() switch
            {
                "create" or "created" => MessageHelper.GetCreationSuccessMessage(entity),
                "update" or "updated" => MessageHelper.GetUpdateSuccessMessage(entity),
                "delete" or "deleted" => MessageHelper.GetDeletionSuccessMessage(entity),
                "cancel" or "cancelled" => MessageHelper.GetCancelSuccessMessage(entity),
                _ => $"{EntityNameHelper.GetEntityName(entity)} processado com sucesso"
            };
            TempData["SuccessMessage"] = message;
        }

        protected void SetStatusChangeSuccessMessage(EntityNames entity, EntityStatus status)
        {
            TempData["SuccessMessage"] = MessageHelper.GetStatusChangeSuccessMessage(entity, status);
        }

        protected void SetErrorMessage(EntityNames entity, string action, Exception ex)
        {
            var message = action.ToLower() switch
            {
                "load" or "loading" => MessageHelper.GetLoadingErrorMessage(entity, ex),
                "create" or "creating" => MessageHelper.GetCreationErrorMessage(entity, ex),
                "update" or "updating" => MessageHelper.GetUpdateErrorMessage(entity, ex),
                "delete" or "deleting" => MessageHelper.GetDeletionErrorMessage(entity, ex),
                "cancel" or "cancelling" => MessageHelper.GetCancelErrorMessage(entity, ex),
                _ => $"Erro ao processar {EntityNameHelper.GetEntityName(entity)}: {ex.Message}"
            };
            TempData["ErrorMessage"] = message;
        }

        protected void SetStatusChangeErrorMessage(EntityNames entity, EntityStatus status, Exception ex)
        {
            TempData["ErrorMessage"] = MessageHelper.GetStatusChangeErrorMessage(entity, status, ex);
        }

        protected void SetCustomErrorMessage(string message)
        {
            TempData["ErrorMessage"] = message;
        }

        protected void SetCustomSuccessMessage(string message)
        {
            TempData["SuccessMessage"] = message;
        }
        protected IActionResult RedirectToIndexWithSuccess(string controllerName, EntityNames entity, string action)
        {
            SetSuccessMessage(entity, action);
            return RedirectToAction("Index", controllerName);
        }

        protected IActionResult RedirectToIndexWithError(string controllerName, EntityNames entity, string action, Exception ex)
        {
            SetErrorMessage(entity, action, ex);
            return RedirectToAction("Index", controllerName);
        }

        protected IActionResult RedirectToDetailsWithSuccess(string controllerName, string id, EntityNames entity, string action)
        {
            SetSuccessMessage(entity, action);
            return RedirectToAction("Details", controllerName, new { id });
        }

        protected IActionResult RedirectToDetailsWithError(string controllerName, string id, EntityNames entity, string action, Exception ex)
        {
            SetErrorMessage(entity, action, ex);
            return RedirectToAction("Details", controllerName, new { id });
        }

        protected IActionResult HandleException(Exception ex, EntityNames entity, string action,
            string? redirectController = null, string redirectAction = "Index", object? routeValues = null)
        {
            SetErrorMessage(entity, action, ex);

            redirectController ??= ControllerContext.ActionDescriptor.ControllerName;

            if (routeValues != null)
            {
                return RedirectToAction(redirectAction, redirectController, routeValues);
            }

            return RedirectToAction(redirectAction, redirectController);
        }
        protected async Task<bool> HasPermissionAsync(string permissionSystemName)
        {
            try
            {
                return await Helpers.PermissionHelper.HasPermissionAsync(HttpContext, permissionSystemName);
            }
            catch
            {
                return false;
            }
        }

        protected bool CanEditOnlyOwnData()
        {
            return !User.IsInRole("Admin");
        }

        protected bool IsResourceOwner(string resourceUserId)
        {
            if (User.IsInRole("Admin"))
                return true;

            return GetCurrentUserId() == resourceUserId;
        }
        protected async Task<IActionResult> HandleGenericDelete<TService>(
        string id,
        TService service,
        Func<TService, string, string, Task> deleteMethod,
        Func<TService, string, string, Task<object>> getMethod,
        string entityName,
        string? redirectUrl = null,
        Func<object, Task<(bool IsValid, string ErrorMessage)>>? customValidation = null)
        where TService : class
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = $"ID do {entityName} não fornecido.";
                return RedirectToAction("Index");
            }

            try
            {
                var token = GetToken();

                var item = await getMethod(service, id, token);
                if (item == null)
                {
                    TempData["ErrorMessage"] = $"{char.ToUpper(entityName[0])}{entityName[1..]} não encontrado.";
                    return RedirectToAction("Index");
                }

                var (IsValid, ErrorMessage) = customValidation != null
                    ? await customValidation(item)
                    : await ValidateBeforeDelete(item, entityName);

                if (!IsValid)
                {
                    TempData["ErrorMessage"] = ErrorMessage;
                    return RedirectToAction("Details", new { id });
                }

                await deleteMethod(service, id, token);

                var description = GetEntityDescription(item);
                TempData["SuccessMessage"] = $"{char.ToUpper(entityName[0])}{entityName[1..]} '{description}' excluído com sucesso.";

                if (!string.IsNullOrEmpty(redirectUrl))
                {
                    return Redirect(redirectUrl);
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao excluir {entityName}: {ex.Message}";
                return RedirectToAction("Details", new { id });
            }
        }

        protected virtual async Task<(bool IsValid, string ErrorMessage)> ValidateBeforeDelete(object item, string entityName) => await Task.FromResult((true, (string?)null));

        protected virtual string GetEntityDescription(object item)
        {
            var type = item.GetType();

            var nameProperty = type.GetProperty("Name");
            if (nameProperty != null)
            {
                return nameProperty.GetValue(item)?.ToString() ?? "Item";
            }

            var descriptionProperty = type.GetProperty("Description");
            if (descriptionProperty != null)
            {
                return descriptionProperty.GetValue(item)?.ToString() ?? "Item";
            }

            var idProperty = type.GetProperty("Id");
            if (idProperty != null)
            {
                return $"Item {idProperty.GetValue(item)}";
            }

            return "Item";
        }
    }
}