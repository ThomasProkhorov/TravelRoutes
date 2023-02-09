using AutoMapper;

namespace TravelRoutes.Services.Infrastructure
{  
    public interface IHaveMappings
    {
        void CreateMappings(IMapperConfigurationExpression configuration);
    }   
}
