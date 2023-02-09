using AutoMapper;
using Newtonsoft.Json;
using TravelRoutes.Services.Interfaces;
using System.Text;

namespace TravelRoutes.Services
{
    public class SearchService : ISearchService
    {
        private readonly IMapper _mapper;
        private readonly IEnvironmentService _enviroment;
        private readonly IRouteCacheService _routeCache;
        private readonly IHttpClientProvider _httpClient;

        public SearchService(IMapper mapper, IEnvironmentService enviroment, IRouteCacheService routeCache, IHttpClientProvider httpClient) 
        {
            _mapper = mapper;
            _enviroment = enviroment;
            _routeCache = routeCache;
            _httpClient = httpClient;
        }

        public async Task<SearchResponse> SearchAsync(SearchRequest options, CancellationToken cancellationToken)
        {
            if (options.Filters?.OnlyCached == null || !options.Filters.OnlyCached.Value)
            {
                foreach (var searchProvider in _enviroment.SearchProviders)
                {
                    var httpRequest = _mapper.Map(options, typeof(SearchRequest), searchProvider.SearchRequestType);

                    var response = await _httpClient.PostAsync(
                        searchProvider.OriginUrl + searchProvider.SearchApi,
                        new StringContent(JsonConvert.SerializeObject(httpRequest), Encoding.UTF8, "application/json"),
                        cancellationToken);

                    var searchResp = JsonConvert.DeserializeObject(
                        await response.Content.ReadAsStringAsync(cancellationToken),
                        searchProvider.SearchResponseType) as IProviderSearchResponse;

                    if (searchResp != null)
                    {
                        await _routeCache.UpdateCache(searchResp.GetRoutes(_mapper), cancellationToken);
                    }
                }
            }

            return new SearchResponse()
            {
                Routes = _routeCache.SearchCache(options).ToArray()
            };
        }

        public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken)
        {
            foreach (var searchProvider in _enviroment.SearchProviders)
            {                
                var response = await _httpClient.GetAsync(searchProvider.OriginUrl + searchProvider.PingApi, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
            }
            return false;
        }

        public Route? Get(string id)
        {
            return _routeCache.GetById(id);
        }
    }
}
