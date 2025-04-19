using System.Text.Json.Serialization;

namespace FinanceSystem.Application.DTOs.Common
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string MessageKey { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }

        [JsonIgnore] 
        public int StatusCode { get; set; } = 200;

        public static ApiResponse<T> SuccessResult(T data)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data,
                StatusCode = 200
            };
        }

        public static ApiResponse<T> ErrorResult(string messageKey, int statusCode = 400)
        {
            return new ApiResponse<T>
            {
                Success = false,
                MessageKey = messageKey,
                Data = default,
                StatusCode = statusCode
            };
        }
    }
}
