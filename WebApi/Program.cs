using Microsoft.AspNetCore.Mvc;
using TravelRoutes.Services;
using TravelRoutes.Services.Infrastructure;
using TravelRoutes.Services.Interfaces;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});
builder.Services.AddControllers();
builder.Services.AddAutoMapper(cfg =>
    AutoMapperConfig.Init(new Assembly[] { Assembly.GetExecutingAssembly() }, cfg));
builder.Services.AddSingleton<IEnvironmentService, EnvironmentService>();
builder.Services.AddSingleton<IRouteCacheService, RouteCacheService>();
builder.Services.AddScoped<IHttpClientProvider, HttpClientProvider>();
builder.Services.AddScoped<ISearchService, SearchService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapControllers();

app.Run();
