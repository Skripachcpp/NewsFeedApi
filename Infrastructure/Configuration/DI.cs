using Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Configuration;

static public class Di
{
    static public void AddConfiguration(this IServiceCollection services)
    {
        services.AddScoped<ITagsRepository, TagsRepository>();
        services.AddScoped<INewsRepository, NewsRepository>();
        services.AddScoped<ICacheRepository, CacheRepository>();
    }
}
