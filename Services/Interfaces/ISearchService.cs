using AutoMapper;
using TravelRoutes.Services.Infrastructure;

namespace TravelRoutes.Services.Interfaces
{

    public interface ISearchService
    {
        Task<SearchResponse> SearchAsync(SearchRequest request, CancellationToken cancellationToken);
        Task<bool> IsAvailableAsync(CancellationToken cancellationToken);
        Route? Get(string id);
    }

    public class SearchRequest
    {
        // Mandatory
        // Start point of route, e.g. Moscow 
        public string Origin { get; set; }

        // Mandatory
        // End point of route, e.g. Sochi
        public string Destination { get; set; }

        // Mandatory
        // Start date of route    
        public DateTime OriginDateTime { get; set; }

        // Optional
        public SearchFilters? Filters { get; set; }
    }

    public class SearchFilters
    {
        // Optional
        // End date of route
        public DateTime? DestinationDateTime { get; set; }

        // Optional
        // Maximum price of route
        public decimal? MaxPrice { get; set; }

        // Optional
        // Minimum value of timelimit for route
        public DateTime? MinTimeLimit { get; set; }

        // Optional
        // Forcibly search in cached data
        public bool? OnlyCached { get; set; }
    }

    public class SearchResponse
    {
        private Route[] _routes = new Route[0];
        private decimal _minPrice;
        private decimal _maxPrice;
        private int _minMinutesRoute;
        private int _maxMinutesRoute;

        // Mandatory
        // Array of routes
        public Route[] Routes
        {
            get { return _routes; }
            set
            {
                _routes = value;
                _minPrice = _routes.Select(r => r.Price).Min();
                _maxPrice = _routes.Select(r => r.Price).Max();
                _minMinutesRoute = (int)_routes.Select(r => r.DestinationDateTime.Subtract(r.OriginDateTime).TotalMinutes).Min();
                _maxMinutesRoute = (int)_routes.Select(r => r.DestinationDateTime.Subtract(r.OriginDateTime).TotalMinutes).Max();
            }
        }

        // Mandatory
        // The cheapest route
        public decimal MinPrice => _minPrice;

        // Mandatory
        // Most expensive route
        public decimal MaxPrice => _maxPrice;

        // Mandatory
        // The fastest route
        public int MinMinutesRoute => _minMinutesRoute;

        // Mandatory
        // The longest route
        public int MaxMinutesRoute => _maxMinutesRoute;
    }

    public class Route : IHaveMappings
    {
        // Mandatory
        // Identifier of the whole route
        public Guid Id { get; set; }

        // Mandatory
        // Start point of route
        public string Origin { get; set; }

        // Mandatory
        // End point of route
        public string Destination { get; set; }

        // Mandatory
        // Start date of route
        public DateTime OriginDateTime { get; set; }

        // Mandatory
        // End date of route
        public DateTime DestinationDateTime { get; set; }

        // Mandatory
        // Price of route
        public decimal Price { get; set; }

        // Mandatory
        // Timelimit. After it expires, route became not actual
        public DateTime TimeLimit { get; set; }

        public void CreateMappings(IMapperConfigurationExpression configuration)
        {
            configuration.CreateMap<ProviderOneRoute, Route>()
                .ForMember(d => d.Id, opt => opt.MapFrom(s => Guid.NewGuid()))
                .ForMember(d => d.Origin, opt => opt.MapFrom(s => s.From))
                .ForMember(d => d.Destination, opt => opt.MapFrom(s => s.To))
                .ForMember(d => d.OriginDateTime, opt => opt.MapFrom(s => s.DateFrom))
                .ForMember(d => d.DestinationDateTime, opt => opt.MapFrom(s => s.DateTo));

            configuration.CreateMap<ProviderTwoRoute, Route>()
                .ForMember(d => d.Id, opt => opt.MapFrom(s => Guid.NewGuid()))
                .ForMember(d => d.Origin, opt => opt.MapFrom(s => s.Departure.Point))
                .ForMember(d => d.Destination, opt => opt.MapFrom(s => s.Arrival.Point))
                .ForMember(d => d.OriginDateTime, opt => opt.MapFrom(s => s.Departure.Date))
                .ForMember(d => d.DestinationDateTime, opt => opt.MapFrom(s => s.Arrival.Date));
        }
    }
}