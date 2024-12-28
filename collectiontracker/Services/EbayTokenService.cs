using Microsoft.AspNetCore.Authentication.BearerToken;
using System.Net.Http.Headers;

namespace collectiontracker.Services
{
    public class EbayTokenService
    {
        private readonly IConfiguration configuration;
        private readonly string tokenEndPoint = "https://api.ebay.com/identity/v1/oauth2/token";

        public EbayTokenService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public async Task<string> GetAccessTokenAsync()
        {
            var clientId = configuration["EbayClientId"];
            var clientSecret = configuration["EbayClientSecret"];

            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, tokenEndPoint);

            var authenicationHeader = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{clientId}:{clientSecret}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authenicationHeader);

            request.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("scope", "https://api.ebay.com/oauth/api_scope")
            });

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var tokenResponse = await response.Content.ReadFromJsonAsync<AccessTokenResponse>();
                return tokenResponse.AccessToken;
            }
            else
            {
                throw new Exception($"Failed to get access token: {response.StatusCode}");
            }
        }
    }
}
