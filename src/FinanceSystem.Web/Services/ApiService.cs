using FinanceSystem.Web.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.IdentityModel.Tokens.Jwt;
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

        public ApiService(IHttpClientFactory httpClientFactory, ILogger<ApiService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<T> GetAsync<T>(string endpoint, string token = null)
        {
            var client = _httpClientFactory.CreateClient("FinanceAPI");
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content, _jsonOptions);
        }

        public async Task<T> PostAsync<T>(string endpoint, object data, string token = null)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("FinanceAPI");
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                var json = JsonSerializer.Serialize(data);
                _logger.LogInformation($"Enviando requisição para {endpoint}: {json}");

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(endpoint, content);

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Resposta do endpoint {endpoint}: {responseContent}");

                response.EnsureSuccessStatusCode();

                return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"Erro na requisição para {endpoint}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro geral na requisição para {endpoint}");
                throw;
            }
        }

        public async Task<T> PutAsync<T>(string endpoint, object data, string token = null)
        {
            var client = _httpClientFactory.CreateClient("FinanceAPI");
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync(endpoint, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
        }

        public async Task DeleteAsync(string endpoint, string token = null)
        {
            var client = _httpClientFactory.CreateClient("FinanceAPI");
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await client.DeleteAsync(endpoint);
            response.EnsureSuccessStatusCode();
        }

        public Task<ClaimsPrincipal> GetClaimsPrincipalFromToken(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();

                // Verifica se o token é válido
                if (!handler.CanReadToken(token))
                {
                    _logger.LogError("Token JWT inválido");
                    return Task.FromResult<ClaimsPrincipal>(null);
                }

                var jwtToken = handler.ReadJwtToken(token);

                var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);

                // Adiciona todas as claims do token
                foreach (var claim in jwtToken.Claims)
                {
                    identity.AddClaim(claim);
                }

                if (!identity.Claims.Any())
                {
                    _logger.LogWarning("Token não contém claims");
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