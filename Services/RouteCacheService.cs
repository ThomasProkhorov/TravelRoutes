using TravelRoutes.Services.Interfaces;
using Route = TravelRoutes.Services.Interfaces.Route;

namespace TravelRoutes.Services
{
    public class RouteCacheService : IRouteCacheService
    {
        private readonly IEnvironmentService _environmentService;
        private readonly Timer _timer;
        private Dictionary<string, Route> _cache = new Dictionary<string, Route>();
        private Dictionary<string, SortedSet<string>> _reverseOrigin = new Dictionary<string, SortedSet<string>>();
        private Dictionary<string, SortedSet<string>> _reverseDestination = new Dictionary<string, SortedSet<string>>();
        private Dictionary<long, SortedSet<string>> _reverseOriginDate = new Dictionary<long, SortedSet<string>>();
        
        public RouteCacheService(IEnvironmentService environmentService) 
        {
            _environmentService = environmentService;
            _timer = new Timer(new TimerCallback((arg) =>
            {  
                lock (_reverseOrigin)
                    lock (_reverseDestination)
                        lock (_reverseOriginDate)
                            lock (_cache)
                            {
                                _cache.Where(r => r.Value.TimeLimit <= DateTime.Now)
                                    .ToList()
                                    .ForEach(r => {
                                    _reverseOrigin[r.Value.Origin].Remove(r.Key);
                                    _reverseDestination[r.Value.Destination].Remove(r.Key);
                                    _reverseOriginDate[r.Value.OriginDateTime.Ticks].Remove(r.Key);
                                    _cache.Remove(r.Key);
                                });
                            }
            }), null, 
                TimeSpan.FromSeconds(_environmentService.UpdateCacheExpiredIemsInterval), 
                TimeSpan.FromSeconds(_environmentService.UpdateCacheExpiredIemsInterval));
        }

        public Route? GetById(string id)
        {
            return _cache.ContainsKey(id) ? _cache[id] : null;
        }
         
        public async Task UpdateCache(IEnumerable<Route> routes, CancellationToken token)
        {
            await Parallel.ForEachAsync(routes, async (route, token) =>
            {
                await Task.Run(() => {
                    lock (_reverseOrigin)
                        lock (_reverseDestination)
                            lock (_reverseOriginDate)
                                lock (_cache)
                                {
                                    var oKeys = _reverseOrigin.ContainsKey(route.Origin) ? 
                                        _reverseOrigin[route.Origin] : new SortedSet<string>();
                                    var dKeys = _reverseDestination.ContainsKey(route.Destination) ? 
                                        _reverseDestination[route.Destination] : new SortedSet<string>();
                                    var odKeys = _reverseOriginDate.ContainsKey(route.OriginDateTime.Ticks) ? 
                                        _reverseOriginDate[route.OriginDateTime.Ticks] : new SortedSet<string>();

                                    var keys = oKeys.Intersect(dKeys).Intersect(odKeys);
                                    if (keys.Count() == 0)
                                    {
                                        token.ThrowIfCancellationRequested();
                                        
                                        var key = route.Id.ToString();
                                        _cache.Add(key, route);
                                        oKeys.Add(key);
                                        dKeys.Add(key);
                                        odKeys.Add(key);
                                        if (!_reverseOrigin.ContainsKey(route.Origin))
                                        {
                                            _reverseOrigin.Add(route.Origin, oKeys);
                                        }
                                        if (!_reverseDestination.ContainsKey(route.Destination))
                                        {
                                            _reverseDestination.Add(route.Destination, dKeys);
                                        }
                                        if (!_reverseOriginDate.ContainsKey(route.OriginDateTime.Ticks))
                                        {
                                            _reverseOriginDate.Add(route.OriginDateTime.Ticks, odKeys);
                                        }

                                    }
                                }
                });
            });
        }

        public IEnumerable<Route> SearchCache(SearchRequest request)
        {
            var destinationDateTime = request.Filters?.DestinationDateTime;
            var maxPrice = request.Filters?.MaxPrice;
            var minTimeLimit = request.Filters?.MinTimeLimit;

            return _cache.Where(r =>
                (string.IsNullOrEmpty(request.Origin) || r.Value.Origin.StartsWith(request.Origin))
                && (string.IsNullOrEmpty(request.Destination) || r.Value.Destination.StartsWith(request.Destination))
                && (request.OriginDateTime.Ticks == 0 || r.Value.OriginDateTime.Date == request.OriginDateTime.Date)
                && (destinationDateTime != null && destinationDateTime.Value.Date == r.Value.DestinationDateTime.Date || destinationDateTime == null)
                && (maxPrice != null && maxPrice.Value >= r.Value.Price || maxPrice == null)
                && (minTimeLimit != null && minTimeLimit.Value <= r.Value.TimeLimit || minTimeLimit == null))
                .Select(r => r.Value)
                .AsEnumerable();
        }
       
    }
}
