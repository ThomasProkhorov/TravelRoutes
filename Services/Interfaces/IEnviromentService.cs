using System.Reflection;

namespace TravelRoutes.Services.Interfaces
{
    public class ProviderSettings
    {
        public string Name { get; set; }
        public string OriginUrl { get; set; }
        public string SearchApi { get; set; }
        public string PingApi { get; set; }
        public Type SearchRequestType
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetExportedTypes().FirstOrDefault(t => t.Name == $"{Name}SearchRequest");
            }
        }
        public Type SearchResponseType
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetExportedTypes().FirstOrDefault(t => t.Name == $"{Name}SearchResponse");
            }
        }
        public Type RouteType
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetExportedTypes().FirstOrDefault(t => t.Name == $"{Name}Route");
            }
        }
    }
    public interface IEnvironmentService
    {
        IEnumerable<ProviderSettings> SearchProviders { get; }
        int UpdateCacheExpiredIemsInterval { get; }
    }
}
