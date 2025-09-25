using Core.Search.Abstractions;
using Core.Search.Configurations;
using Core.Search.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Search;

public static class DependencyInjection
{
    public static IServiceCollection AddCoreSearch(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ElasticSearchOptions>(configuration.GetSection("ElasticSearch"));

        services.AddScoped(typeof(ISearchService<>), typeof(ElasticSearchService<>));

        return services;
    }
}
