namespace TravelRoutes.Services.Interfaces
{
    public interface IRouteCacheService
    {
        Route? GetById(string id);
        Task UpdateCache(IEnumerable<Route> routes, CancellationToken token);
        IEnumerable<Route> SearchCache(SearchRequest request);        
    }
}
