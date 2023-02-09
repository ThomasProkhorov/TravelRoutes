using AutoMapper;
using TravelRoutes.Services.Infrastructure;

namespace TravelRoutes.Services.Interfaces
{

    // HTTP POST http://provider-one/api/v1/search

    public class ProviderOneSearchRequest : IHaveMappings
    {
        // Mandatory
        // Start point of route, e.g. Moscow 
        public string From { get; set; }

        // Mandatory
        // End point of route, e.g. Sochi
        public string To { get; set; }

        // Mandatory
        // Start date of route
        public DateTime DateFrom { get; set; }

        // Optional
        // End date of route
        public DateTime? DateTo { get; set; }

        // Optional
        // Maximum price of route
        public decimal? MaxPrice { get; set; }

        public void CreateMappings(IMapperConfigurationExpression configuration)
        {
            configuration.CreateMap<SearchRequest, ProviderOneSearchRequest>()
                .ForMember(d => d.From, opt => opt.MapFrom(s => s.Origin))
                .ForMember(d => d.To, opt => opt.MapFrom(s => s.Destination))
                .ForMember(d => d.DateFrom, opt => opt.MapFrom(s => s.OriginDateTime))
                .ForMember(d => d.DateTo, opt => opt.MapFrom(s => s.Filters == null ? null : s.Filters.DestinationDateTime))
                .ForMember(d => d.MaxPrice, opt => opt.MapFrom(s => s.Filters == null ? null : s.Filters.MaxPrice));
        }
    }

    public class ProviderOneSearchResponse : IProviderSearchResponse
    {
        // Mandatory
        // Array of routes
        public ProviderOneRoute[] Routes { get; set; } = new ProviderOneRoute[0];
        public IEnumerable<Route> GetRoutes(IMapper mapper)
        {
            return Routes.Select(r => mapper.Map<Route>(r));
        }
    }

    public class ProviderOneRoute
    {
        // Mandatory
        // Start point of route
        public string From { get; set; }

        // Mandatory
        // End point of route
        public string To { get; set; }

        // Mandatory
        // Start date of route
        public DateTime DateFrom { get; set; }

        // Mandatory
        // End date of route
        public DateTime DateTo { get; set; }

        // Mandatory
        // Price of route
        public decimal Price { get; set; }

        // Mandatory
        // Timelimit. After it expires, route became not actual
        public DateTime TimeLimit { get; set; }
    }

    // HTTP GET http://provider-one/api/v1/ping
    //      - HTTP 200 if provider is ready
    //      - HTTP 500 if provider is down
}