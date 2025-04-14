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

        public ApiService(
            IHttpClientFactory httpClientFactory,
            ILogger<ApiService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
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

        public async Task<bool> VerifyTokenAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
                return false;

            try
            {
                var client = _httpClientFactory.CreateClient("FinanceAPI");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

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
            try
            {
                var client = _httpClientFactory.CreateClient("FinanceAPI");
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                var response = await client.GetAsync(endpoint);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    await HandleUnauthorizedResponse();
                    throw new UnauthorizedAccessException("Sessão expirada ou inválida");
                }

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(content, _jsonOptions)!;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                await HandleUnauthorizedResponse();
                throw new UnauthorizedAccessException("Sessão expirada ou inválida");
            }
        }

        public async Task<T> PostAsync<T>(string endpoint, object? data = null, string token = "")
        {
            try
            {
                var client = _httpClientFactory.CreateClient("FinanceAPI");
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                var json = data != null ? JsonSerializer.Serialize(data) : "";
                _logger.LogInformation($"Enviando requisição para {endpoint}: {json}");

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(endpoint, content);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    await HandleUnauthorizedResponse();
                    throw new UnauthorizedAccessException("Sessão expirada ou inválida");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Resposta do endpoint {endpoint}: {responseContent}");

                response.EnsureSuccessStatusCode();
                return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions)!;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                await HandleUnauthorizedResponse();
                throw new UnauthorizedAccessException("Sessão expirada ou inválida");
            }
        }

        public async Task<T> PutAsync<T>(string endpoint, object? data = null, string token = "")
        {
            try
            {
                var client = _httpClientFactory.CreateClient("FinanceAPI");
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PutAsync(endpoint, content);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    await HandleUnauthorizedResponse();
                    throw new UnauthorizedAccessException("Sessão expirada ou inválida");
                }

                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions)!;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                await HandleUnauthorizedResponse();
                throw new UnauthorizedAccessException("Sessão expirada ou inválida");
            }
        }

        public async Task DeleteAsync(string endpoint, string token)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("FinanceAPI");
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                var response = await client.DeleteAsync(endpoint);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    await HandleUnauthorizedResponse();
                    throw new UnauthorizedAccessException("Sessão expirada ou inválida");
                }

                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                await HandleUnauthorizedResponse();
                throw new UnauthorizedAccessException("Sessão expirada ou inválida");
            }
        }

        public async Task<T> DeleteAsync<T>(string endpoint, string token)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("FinanceAPI");
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                var response = await client.DeleteAsync(endpoint);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    await HandleUnauthorizedResponse();
                    throw new UnauthorizedAccessException("Sessão expirada ou inválida");
                }

                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions)!;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                await HandleUnauthorizedResponse();
                throw new UnauthorizedAccessException("Sessão expirada ou inválida");
            }
        }

        public Task<ClaimsPrincipal> GetClaimsPrincipalFromToken(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                if (!handler.CanReadToken(token))
                {
                    _logger.LogError("Token JWT inválido");
                    return Task.FromResult<ClaimsPrincipal>(null);
                }

                var jwtToken = handler.ReadJwtToken(token);
                var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);

                foreach (var claim in jwtToken.Claims)
                {
                    var claimType = claim.Type switch
                    {
                        "nameid" => ClaimTypes.NameIdentifier,
                        "unique_name" => ClaimTypes.Name,
                        "email" => ClaimTypes.Email,
                        "role" => ClaimTypes.Role,
                        _ => claim.Type
                    };

                    identity.AddClaim(new Claim(claimType, claim.Value));
                }

                var principal = new ClaimsPrincipal(identity);
                return Task.FromResult(principal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar token JWT");
                return Task.FromResult<ClaimsPrincipal>(null);
            }
        }
    }
}