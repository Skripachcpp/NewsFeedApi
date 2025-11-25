using Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Configuration;

static public class Di
{
    static public IServiceCollection AddConfiguration(this IServiceCollection services)
    {
        services.AddScoped<ITagsRepository, TagsRepository>();
        services.AddScoped<INewsRepository, NewsRepository>();
        services.AddScoped<ICacheRepository, CacheRepository>();

        return services;
    } 
}
