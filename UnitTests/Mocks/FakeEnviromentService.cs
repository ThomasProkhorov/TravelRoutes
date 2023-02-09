using TravelRoutes.Services.Interfaces;

namespace UnitTests.Mocks
{
    internal class FakeEnviromentService : IEnvironmentService
    {
        public IEnumerable<ProviderSettings> SearchProviders 
        {
            get
            {
                return new ProviderSettings[] { 
                    new ProviderSettings { 
                        Name= "ProviderOne",                    
                        OriginUrl = "http://provider-one/",
                        SearchApi = "api/v1/search",
                        PingApi = "api/v1/ping"
                    }, new ProviderSettings { 
                        Name= "ProviderTwo",
                        OriginUrl = "http://provider-two/",
                        SearchApi = "api/v1/search",
                        PingApi = "api/v1/ping"
                    } 
                };
            }
        }
        public int UpdateCacheExpiredIemsInterval => 3;
    }
}
