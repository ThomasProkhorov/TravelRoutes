using Microsoft.Extensions.Configuration;
using TravelRoutes.Services.Interfaces;

namespace TravelRoutes.Services
{
    public class EnvironmentService : IEnvironmentService
    {
        private IConfiguration _configuration;
        public EnvironmentService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IEnumerable<ProviderSettings> SearchProviders => _configuration.GetSection("SearchProviders").Get<IEnumerable<ProviderSettings>>();
        public int UpdateCacheExpiredIemsInterval => _configuration.GetValue<int>("UpdateCacheExpiredIemsInterval");
    }
}
