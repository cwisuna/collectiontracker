using Microsoft.AspNetCore.Authentication.BearerToken;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace collectiontracker.Services
{
    public class EbayTokenService
    {
        private readonly IConfiguration configuration;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly string tokenEndPoint = "https://api.ebay.com/identity/v1/oauth2/token";

        public EbayTokenService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            this.configuration = configuration;
            this.httpClientFactory = httpClientFactory;
        }
        public async Task<string> GetAccessTokenAsync()
        {
            var clientId = configuration["EbayClientId"];
            var clientSecret = configuration["EbayClientSecret"];

            var client = httpClientFactory.CreateClient(); // Creates the HttpClient
            var request = new HttpRequestMessage(HttpMethod.Post, tokenEndPoint);

            var authenticationHeader = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{clientId}:{clientSecret}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authenticationHeader);

            request.Content = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("scope", "https://api.ebay.com/oauth/api_scope")
        });

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var tokenResponse = await response.Content.ReadFromJsonAsync<EbayAccessTokenResponse>();

                if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
                {
                    throw new Exception("Failed to parse access token response.");
                }

                return tokenResponse.AccessToken;
            }
            else
            {
                throw new Exception($"Failed to get access token: {response.StatusCode}, {response.ReasonPhrase}");
            }
        }
    }
}
