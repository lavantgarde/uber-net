using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using uber_net.Models;
using uber_net.Utilities;

namespace uber_net
{
    public class UberClient : ResponseHeader
    {
        private const string _url = @"https://api.uber.com";
        private const string _apiVersion = "v1.2";

        private readonly string _token;
        private readonly HttpClient _httpClient;
        private TokenTypes _tokenType;

        /// <summary>
        /// Initializes a new instance of the <see cref="UberClient"/> class with <see cref="TokenTypes.Server"/>.
        /// </summary>
        /// <param name="token">The token.</param>
        public UberClient(string token) : this(TokenTypes.Server, token, new HttpClient())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UberClient"/> class.
        /// </summary>
        /// <param name="tokenType">The token type to use.</param>
        /// <param name="token">The token.</param>
        /// <param name="httpClient">The http client to use.</param>
        public UberClient(TokenTypes tokenType, string token, HttpClient httpClient)
        {
            _tokenType = tokenType;
            _token = token;
            _httpClient = httpClient;

            if (_tokenType == TokenTypes.Server)
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", _token);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            }

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// Gets information about Uber products at a given location. 
        /// See <see cref="https://developer.uber.com/docs/riders/references/api/v1.2/products-get"/> for more info.
        /// </summary>
        /// <param name="latitude">The latitude</param>
        /// <param name="longitude">The longitude</param>
        /// <returns>The <see cref="Products"/> information.</returns>
        public async Task<Products> GetProductsAsync(float latitude, float longitude)
        {
            var urlSuffix = string.Format("products?latitude={0}&longitude={1}", latitude.ToString("R"), longitude.ToString("R"));
            var url = UrlUtilities.FormatUrl(_url, _apiVersion, urlSuffix);

            return await HttpGetAsync<Products>(url);
        }

        /// <summary>
        /// Gets an estimated price range for each product offered at a given location.
        /// See <see cref="https://developer.uber.com/docs/riders/references/api/v1.2/estimates-price-get"/> for more info.
        /// </summary>
        /// <param name="startLatitude">Latitude component of start location.</param>
        /// <param name="startLongitude">Longitude component of start location.</param>
        /// <param name="endLatitude">Latitude component of end location.</param>
        /// <param name="endLongitude">Longitude component of end location.</param>
        /// <param name="seat_count">(Optional) The number of seats required for uberPOOL. Default and maximum value is 2.</param>
        /// <returns></returns>
        public async Task<Prices> GetPriceEstimateAsync(float startLatitude, float startLongitude, float endLatitude, float endLongitude, int seat_count = 2)
        {
            if (seat_count > 2 || seat_count < 0) throw new ArgumentOutOfRangeException(nameof(seat_count), "Must be a positive number, no larger than 2.");

            var urlSuffix = string.Format(
                "estimates/price?start_latitude={0}&start_longitude={1}&end_latitude={2}&end_longitude={3}&seat_count={4}",
                startLatitude.ToString("R"),
                startLongitude.ToString("R"),
                endLatitude.ToString("R"),
                endLongitude.ToString("R"),
                seat_count.ToString("R"));

            var url = UrlUtilities.FormatUrl(_url, _apiVersion, urlSuffix);

            return await HttpGetAsync<Prices>(url);
        }

        /// <summary>
        /// Gets ETAs for all products currently available at a given location, with the ETA for each product expressed as integers in seconds.
        /// See <see cref="https://developer.uber.com/docs/riders/references/api/v1.2/estimates-time-get"/> for more info.
        /// </summary>
        /// <param name="startLatitude">Latitude component.</param>
        /// <param name="startLongitude">Longitude component.</param>
        /// <param name="productId">(Optional) Unique identifier representing a specific product for a given latitude & longitude.</param>
        /// <returns></returns>
        public async Task<Times> GetTimeEstimateAsync(float startLatitude, float startLongitude, string productId = "")
        {
            var urlSuffix = string.Format("estimates/time?start_latitude={0}&start_longitude={1}", startLatitude,
                startLongitude);

            if (!string.IsNullOrWhiteSpace(productId))
                urlSuffix += string.Format("&product_id={0}", productId);

            var url = UrlUtilities.FormatUrl(_url, _apiVersion, urlSuffix);

            return await HttpGetAsync<Times>(url);
        }

        /// <summary>
        /// Gets a limited amount of data about a user’s lifetime activity with Uber. 
        /// See <see cref="https://developer.uber.com/docs/riders/references/api/v1.2/history-get"/> for more info.
        /// </summary>
        /// <param name="offset">(Optional) Offset the list of returned results by this amount. Default is zero.</param>
        /// <param name="limit">(Optional) Number of items to retrieve. Default is 5, maximum is 50.</param>
        /// <returns></returns>
        public async Task<UserActivity> GetUserActivityAsync(int offset = 0, int limit = 5)
        {
            if (_tokenType == TokenTypes.Server) throw new ArgumentException("This endpoint only supports access token.");
            // todo: check scopes...
            if (limit > 50) throw new ArgumentOutOfRangeException(nameof(limit), "Max is 50.");

            var urlSuffix = $"history?offset={offset}&limit={limit}";
            var url = UrlUtilities.FormatUrl(_url, _apiVersion, urlSuffix);

            return await HttpGetAsync<UserActivity>(url);
        }

        /// <summary>
        /// Gets information about the Uber user that has authorized with the application.
        /// </summary>
        /// <returns>The authorized <see cref="User"/>.</returns>
        public async Task<User> GetUserAsync()
        {
            if (_tokenType == TokenTypes.Server) throw new ArgumentException("This endpoint only supports access token.");

            var url = UrlUtilities.FormatUrl(_url, _apiVersion, "me");

            return await HttpGetAsync<User>(url);
        }

        private async Task<T> HttpGetAsync<T>(string url)
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get
            };

            var responseMessage = await _httpClient.SendAsync(request).ConfigureAwait(false);
            var response = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (responseMessage.IsSuccessStatusCode)
            {
                RateLimitRemaining = responseMessage.Headers.GetValues("X-Rate-Limit-Remaining").FirstOrDefault();
                Etag = responseMessage.Headers.GetValues("Etag").FirstOrDefault();
                RateLimitReset = responseMessage.Headers.GetValues("X-Rate-Limit-Reset").FirstOrDefault();
                RateLimitLimit = responseMessage.Headers.GetValues("X-Rate-Limit-Limit").FirstOrDefault();
                UberApp = responseMessage.Headers.GetValues("X-Uber-App").FirstOrDefault();

                var jObject = JObject.Parse(response);
                var payload = JsonConvert.DeserializeObject<T>(jObject.ToString());
                return payload;
            }

            throw new Exception("Erorr");
        }
    }
}
