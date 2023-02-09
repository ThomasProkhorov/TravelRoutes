using AutoMapper;

namespace TravelRoutes.Services.Interfaces
{
    public interface IProviderSearchResponse
    {
        IEnumerable<Route> GetRoutes(IMapper mapper);
    }
}
