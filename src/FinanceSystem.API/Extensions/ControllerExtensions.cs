using FinanceSystem.Application.DTOs.Common;
using Microsoft.AspNetCore.Mvc;

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
            string message,
            int statusCode = StatusCodes.Status400BadRequest)
        {
            var response = ApiResponse<T>.ErrorResult(message, statusCode);
            response.Message = message; 
            return controller.StatusCode(statusCode, response);
        }

        public static ActionResult ApiUnauthorized<T>(this ControllerBase controller,
            string message)
        {
            return controller.ApiError<T>(message, StatusCodes.Status401Unauthorized);
        }

        public static ActionResult ApiNotFound<T>(this ControllerBase controller,
            string message)
        {
            return controller.ApiError<T>(message, StatusCodes.Status404NotFound);
        }

        public static ActionResult ApiBadRequest<T>(this ControllerBase controller,
            string message)
        {
            return controller.ApiError<T>(message, StatusCodes.Status400BadRequest);
        }

        public static ActionResult ApiForbidden<T>(this ControllerBase controller,
            string message)
        {
            return controller.ApiError<T>(message, StatusCodes.Status403Forbidden);
        }
    }
}