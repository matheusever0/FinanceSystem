using FinanceSystem.Application.DTOs.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace FinanceSystem.API.Extensions
{
    public static class ControllerExtensions
    {
        public static ActionResult ApiOk<T>(this ControllerBase controller, T data)
        {
            var response = ApiResponse<T>.SuccessResult(data);
            return controller.Ok(response);
        }

        public static ActionResult ApiError<T>(this ControllerBase controller,
            string messageKey,
            IStringLocalizer localizer,
            int statusCode = StatusCodes.Status400BadRequest)
        {
            var response = ApiResponse<T>.ErrorResult(messageKey, statusCode);
            response.Message = localizer[messageKey].Value;

            return controller.StatusCode(statusCode, response);
        }

        public static ActionResult ApiUnauthorized<T>(this ControllerBase controller,
            string messageKey,
            IStringLocalizer localizer)
        {
            return controller.ApiError<T>(messageKey, localizer, StatusCodes.Status401Unauthorized);
        }

        public static ActionResult ApiNotFound<T>(this ControllerBase controller,
            string messageKey,
            IStringLocalizer localizer)
        {
            return controller.ApiError<T>(messageKey, localizer, StatusCodes.Status404NotFound);
        }

        public static ActionResult ApiBadRequest<T>(this ControllerBase controller,
            string messageKey,
            IStringLocalizer localizer)
        {
            return controller.ApiError<T>(messageKey, localizer, StatusCodes.Status400BadRequest);
        }

        public static ActionResult ApiForbidden<T>(this ControllerBase controller,
            string messageKey,
            IStringLocalizer localizer)
        {
            return controller.ApiError<T>(messageKey, localizer, StatusCodes.Status403Forbidden);
        }
    }
}