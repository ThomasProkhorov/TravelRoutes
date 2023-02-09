using AutoMapper;
using System.Reflection;

namespace TravelRoutes.Services.Infrastructure
{
    public class AutoMapperConfig
    {
        public static void Init(Assembly[] modelsAssemblies, IMapperConfigurationExpression cfg)
        {
            var types = Enumerable.Empty<Type>();
            foreach (var ma in modelsAssemblies)
            {
                types = types.Concat(ma.GetExportedTypes());
            }

            types.SelectMany(t => t.GetInterfaces().Where(i =>
                        i.IsGenericType
                        && i.GetGenericTypeDefinition() == typeof(IMapFrom<>)
                        && !t.IsAbstract
                        && !t.IsInterface)
                    .Select(i => new { Source = i.GetGenericArguments()[0], Destination = t }))
                    .ToList()
                    .ForEach(m =>
                        cfg.CreateMap(m.Source, m.Destination));

            types.SelectMany(t => t.GetInterfaces().Where(i => 
                        typeof(IHaveMappings).IsAssignableFrom(t)
                        && !t.IsAbstract
                        && i.IsInterface)
                    .Select(i => Activator.CreateInstance(t) as IHaveMappings))
                    .ToList()
                    .ForEach(m => 
                        m.CreateMappings(cfg));
        }
    }
}
