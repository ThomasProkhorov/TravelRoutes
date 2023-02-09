using TravelRoutes.Services.Interfaces;

namespace TravelRoutes.Services
{
    public class HttpClientProvider : IHttpClientProvider
    {
        private readonly HttpClient _httpClient = new HttpClient();  

        public Task<HttpResponseMessage> GetAsync(string requestUri, CancellationToken token)
        {
            return _httpClient.GetAsync(requestUri, token);
        }

        public Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content, CancellationToken token)
        {
            return _httpClient.PostAsync(requestUri, content, token);
        }
    }
}
