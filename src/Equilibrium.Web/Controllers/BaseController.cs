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
            string redirectController = null, string redirectAction = "Index", object routeValues = null)
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
    }
}