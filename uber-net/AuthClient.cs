using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using uber_net.Models;
using uber_net.Utilities;

namespace uber_net
{
    public class AuthClient
    {
        private const string _baseAddress = "https://login.uber.com";
        private const string _apiVersion = "v2";

        private static readonly string OAuthTokenUrl = $"{_baseAddress}/oauth/{_apiVersion}/token";

        private readonly HttpClient _httpClient;

        public AuthClient() : this(new HttpClient() { BaseAddress = new Uri(_baseAddress) })
        {
        }

        public AuthClient(HttpClient httpClient)
        {
            if (httpClient == null) throw new ArgumentNullException(nameof(httpClient));

            _httpClient = httpClient;
        }
        /// <summary>
        /// The current API version targetted.
        /// </summary>
        public string ApiVersion => _apiVersion;

        /// <summary>
        /// The access token retrieved from the response. Can be empty.
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// The refresh token retrieved from the response. Can be empty.
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// Attempts to authenticate using OAuth and hydrate <see cref="AccessToken"/> and <see cref="RefreshToken"/>.
        /// </summary>
        /// <param name="clientId">The client id to authenticate against.</param>
        /// <param name="clientSecret">The client secrete to authenticate with.</param>
        /// <param name="redirectUri">The redirect uri if/when authenticate succeeds.</param>
        /// <param name="authorizationCode">The authorization code.</param>
        /// <returns>The empty task.</returns>
        public async Task AuthenticateOAuthAsync(string clientId, string clientSecret, string redirectUri, string authorizationCode)
        {
            if (string.IsNullOrEmpty(clientId)) throw new ArgumentNullException($"{nameof(clientId)}");
            if (string.IsNullOrEmpty(clientSecret)) throw new ArgumentNullException($"{nameof(clientSecret)}");
            if (string.IsNullOrEmpty(redirectUri)) throw new ArgumentNullException($"{nameof(redirectUri)}");
            if (string.IsNullOrEmpty(authorizationCode)) throw new ArgumentNullException($"{nameof(authorizationCode)}");
            if (!UrlUtilities.CheckUri(redirectUri)) throw new ArgumentException($"Invalid {nameof(redirectUri)}");

            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("client_secret", clientSecret),
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("redirect_uri", redirectUri),
                    new KeyValuePair<string, string>("code", authorizationCode),
                    new KeyValuePair<string, string>("scope", "") // todo: implement scopes
                });

            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(OAuthTokenUrl),
                Content = content
            };

            var responseMessage = await _httpClient.SendAsync(request).ConfigureAwait(false);
            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (responseMessage.IsSuccessStatusCode)
            {
                var authToken = JsonConvert.DeserializeObject<AuthToken>(response);

                AccessToken = authToken.access_token;
                RefreshToken = authToken.refresh_token;
            }
            else
            {
                //TODO: Richer error handling
                throw new Exception($"Error authenticating: \n {response}");
            }
        }
    }
}
