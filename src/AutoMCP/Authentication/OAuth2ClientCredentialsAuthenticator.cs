using System.Net.Http.Headers;
using System.Text.Json;
using AutoMCP.Abstractions;
using AutoMCP.Helpers;

namespace AutoMCP.Authentication
{
    /// <summary>
    /// Provides OAuth 2.0 client credentials flow authentication
    /// </summary>
    public class OAuth2ClientCredentialsAuthenticator : IAuthenticator
    {
        private readonly string _tokenUrl;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _scope;
        private readonly HttpClient _httpClient;
        private readonly OAuthCache _tokenCache;

        public OAuth2ClientCredentialsAuthenticator(
            string tokenUrl,
            string clientId,
            string clientSecret,
            string? scope = null)
        {
            _tokenUrl = tokenUrl ?? throw new ArgumentNullException(nameof(tokenUrl));
            _clientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
            _clientSecret = clientSecret ?? throw new ArgumentNullException(nameof(clientSecret));
            _scope = scope ?? "api";
            _httpClient = new HttpClient();
            _tokenCache = new OAuthCache();
        }

        public async Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            var token = await GetAccessTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<Dictionary<string, string>> GetAuthHeadersAsync()
        {
            var token = await GetAccessTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                return new Dictionary<string, string>
                {
                    ["Authorization"] = $"Bearer {token}"
                };
            }

            return new Dictionary<string, string>();
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            var token = await GetAccessTokenAsync();
            return !string.IsNullOrEmpty(token);
        }

        private async Task<string?> GetAccessTokenAsync()
        {
            // Check cache first
            var cachedToken = _tokenCache.GetToken();
            if (cachedToken != null)
            {
                return cachedToken;
            }

            try
            {
                // Build request for token
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("client_id", _clientId),
                    new KeyValuePair<string, string>("client_secret", _clientSecret),
                    new KeyValuePair<string, string>("scope", _scope)
                });

                // Request new token
                var response = await _httpClient.PostAsync(_tokenUrl, content);
                response.EnsureSuccessStatusCode();

                var tokenJson = await response.Content.ReadAsStringAsync();
                var tokenData = JsonSerializer.Deserialize<JsonElement>(tokenJson);

                var accessToken = tokenData.GetProperty("access_token").GetString();
                var expiresIn = tokenData.TryGetProperty("expires_in", out var expiresInElement)
                    ? expiresInElement.GetInt32()
                    : 3600;

                if (accessToken != null)
                {
                    _tokenCache.SetToken(accessToken, expiresIn);
                    return accessToken;
                }
            }
            catch (Exception)
            {
                // Log error or handle as needed
            }

            return null;
        }
    }
}