using FinanceSystem.Web.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace FinanceSystem.Web.Services
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
            _logger.LogInformation("Resposta completa: {Content}", content);

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
            var json = data != null ? JsonSerializer.Serialize(data) : "";
            _logger.LogInformation("Enviando requisição para {Endpoint}: {Json}", endpoint, json);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(endpoint, content);
            var responseContent = await HandleResponse(response);
            _logger.LogInformation("Resposta do endpoint {Endpoint}: {ResponseContent}", endpoint, responseContent);

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

    }
}
