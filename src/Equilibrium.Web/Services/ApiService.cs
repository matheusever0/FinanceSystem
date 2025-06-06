using Equilibrium.Web.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Equilibrium.Web.Services
{
    public class ApiService : IApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ApiService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private static readonly JwtSecurityTokenHandler TokenHandler = new();
        private static readonly Dictionary<string, string> ClaimTypeMapping = new()
            {
                { "nameid", ClaimTypes.NameIdentifier },
                { "unique_name", ClaimTypes.Name },
                { "email", ClaimTypes.Email },
                { "role", ClaimTypes.Role }
            };

        public ApiService(
            IHttpClientFactory httpClientFactory,
            ILogger<ApiService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        private async Task HandleUnauthorizedResponse()
        {
            _logger.LogWarning("Token expirado ou inválido. Redirecionando para login...");
            if (_httpContextAccessor.HttpContext != null)
            {
                await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                _httpContextAccessor.HttpContext.Session.Clear();
                _httpContextAccessor.HttpContext.Items["RequireRelogin"] = true;
            }
        }

        private HttpClient CreateClient(string token)
        {
            var client = _httpClientFactory.CreateClient("FinanceAPI");
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            return client;
        }

        private async Task<string> HandleResponse(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await HandleUnauthorizedResponse();
                var errorMessage = string.IsNullOrEmpty(content) ? "Sessão expirada ou inválida" : content;
                throw new UnauthorizedAccessException(errorMessage);
            }

            if (!response.IsSuccessStatusCode)
            {
                try
                {
                    var errorResponse = JsonSerializer.Deserialize<ApiErrorResponse>(content, _jsonOptions);
                    if (errorResponse?.Errors != null)
                    {
                        var allMessages = errorResponse.Errors
                            .SelectMany(kvp => kvp.Value.Select(msg => $"{kvp.Key}: {msg}"))
                            .ToList();

                        var formattedMessage = string.Join("\n", allMessages);
                        throw new Exception($"Erros de validação:\n{formattedMessage}");
                    }
                }
                catch (JsonException)
                {
                    // ignora, segue com erro genérico
                }

                var fallbackMessage = string.IsNullOrEmpty(content) ? "Erro desconhecido na API" : content;
                throw new Exception(fallbackMessage);
            }

            return content;
        }

        public class ApiErrorResponse
        {
            public string Type { get; set; }
            public string Title { get; set; }
            public int Status { get; set; }
            public Dictionary<string, string[]> Errors { get; set; }
            public string TraceId { get; set; }
        }

        public async Task<bool> VerifyTokenAsync(string token)
        {
            if (string.IsNullOrEmpty(token)) return false;
            try
            {
                var client = CreateClient(token);
                var response = await client.GetAsync("api/auth/verify-token");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<T> GetAsync<T>(string endpoint, string token)
        {
            var client = CreateClient(token);
            var response = await client.GetAsync(endpoint);
            var content = await HandleResponse(response);

            return JsonSerializer.Deserialize<T>(content, _jsonOptions)!;
        }

        public async Task<T> PostAsync<T>(string endpoint, object? data = null, string token = "")
        {
            var client = CreateClient(token);

            HttpContent? content = null;
            if (data != null)
            {
                var json = JsonSerializer.Serialize(data);
                content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            var response = await client.PostAsync(endpoint, content);
            var responseContent = await HandleResponse(response);

            return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions)!;
        }

        public async Task<T> PutAsync<T>(string endpoint, object? data = null, string token = "")
        {
            var client = CreateClient(token);
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PutAsync(endpoint, content);
            var responseContent = await HandleResponse(response);

            return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions)!;
        }

        public async Task DeleteAsync(string endpoint, string token)
        {
            var client = CreateClient(token);
            var response = await client.DeleteAsync(endpoint);
            await HandleResponse(response);
        }

        public async Task<T> DeleteAsync<T>(string endpoint, string token)
        {
            var client = CreateClient(token);
            var response = await client.DeleteAsync(endpoint);
            var responseContent = await HandleResponse(response);

            return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions)!;
        }

        public Task<ClaimsPrincipal> GetClaimsPrincipalFromToken(string token)
        {
            try
            {
                if (!TokenHandler.CanReadToken(token))
                {
                    _logger.LogError("Token JWT inválido");
                    return Task.FromResult<ClaimsPrincipal>(null);
                }

                var jwtToken = TokenHandler.ReadJwtToken(token);
                var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);

                foreach (var claim in jwtToken.Claims)
                {
                    var claimType = ClaimTypeMapping.TryGetValue(claim.Type, out var mappedType) ? mappedType : claim.Type;
                    identity.AddClaim(new Claim(claimType, claim.Value));
                }

                return Task.FromResult(new ClaimsPrincipal(identity));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar token JWT");
                return Task.FromResult<ClaimsPrincipal>(null);
            }
        }
        private string ConvertFilterToQueryString(object filter)
        {
            if (filter == null) return string.Empty;

            var properties = filter.GetType().GetProperties();
            var parameters = new List<string>();

            foreach (var prop in properties)
            {
                var value = prop.GetValue(filter);
                if (value != null)
                {
                    if (value is DateTime dateTime)
                        parameters.Add($"{prop.Name}={Uri.EscapeDataString(dateTime.ToString("yyyy-MM-dd"))}");
                    else
                        parameters.Add($"{prop.Name}={Uri.EscapeDataString(value.ToString())}");
                }
            }

            return parameters.Count > 0 ? $"?{string.Join("&", parameters)}" : string.Empty;
        }

        public async Task<T> GetFilteredAsync<T>(string endpoint, object filter, string token)
        {
            var client = CreateClient(token);

            // Converter objeto de filtro para query string
            var queryString = ConvertFilterToQueryString(filter);
            var response = await client.GetAsync($"{endpoint}{queryString}");
            var content = await HandleResponse(response);

            // Extract pagination headers if present
            if (_httpContextAccessor?.HttpContext != null)
            {
                if (response.Headers.Contains("X-Pagination-Total"))
                {
                    response.Headers.TryGetValues("X-Pagination-Total", out var totalValues);
                    _httpContextAccessor.HttpContext.Items["X-Pagination-Total"] = totalValues?.FirstOrDefault();
                }

                if (response.Headers.Contains("X-Pagination-Pages"))
                {
                    response.Headers.TryGetValues("X-Pagination-Pages", out var pagesValues);
                    _httpContextAccessor.HttpContext.Items["X-Pagination-Pages"] = pagesValues?.FirstOrDefault();
                }

                if (response.Headers.Contains("X-Pagination-Page"))
                {
                    response.Headers.TryGetValues("X-Pagination-Page", out var pageValues);
                    _httpContextAccessor.HttpContext.Items["X-Pagination-Page"] = pageValues?.FirstOrDefault();
                }

                if (response.Headers.Contains("X-Pagination-Size"))
                {
                    response.Headers.TryGetValues("X-Pagination-Size", out var sizeValues);
                    _httpContextAccessor.HttpContext.Items["X-Pagination-Size"] = sizeValues?.FirstOrDefault();
                }
            }

            return JsonSerializer.Deserialize<T>(content, _jsonOptions)!;
        }
    }
}
