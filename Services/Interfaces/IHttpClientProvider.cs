namespace TravelRoutes.Services.Interfaces
{
    public interface IHttpClientProvider
    {
        Task<HttpResponseMessage> GetAsync(string requestUri, CancellationToken token);
        Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content, CancellationToken token);
    }
}
