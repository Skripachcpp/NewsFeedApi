using System.Data;
using System.Text;
using Domain.Interfaces;
using Infrastructure;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.Configuration;
using Microsoft.IdentityModel.Tokens;
using Prometheus;
using StackExchange.Redis;
using Web.Application;

var builder = WebApplication.CreateBuilder(args);

// bd _
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (connectionString == null) throw new ConstraintException("Отсутствует connection string: DefaultConnectio");

builder.Services.AddDbContext<EfContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddScoped<DpContext>(_ => new DpContext(connectionString));
// bd ^

// для пет проекта просто отключаем Cors 
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddScoped<ITagsRepository, TagsRepository>();
builder.Services.AddScoped<INewsRepository, NewsRepository>();
builder.Services.AddScoped<ICacheRepository, CacheRepository>();

// redis _
var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
if (redisConnectionString == null) throw new InvalidConfigurationException("Отсутствует connection string для redis: Redis");

builder.Services.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(redisConnectionString));
builder.Services.AddSingleton<Lazy<IConnectionMultiplexer>>(sp =>
  new Lazy<IConnectionMultiplexer>(sp.GetRequiredService<IConnectionMultiplexer>));

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = "NewsFeed_";
});
// redis ^

// настройки авторизации
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
if (jwtSettings == null) throw new InvalidOperationException("Не секретного ключа для jwt: JwtSettings");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
  .AddJwtBearer(options =>
  {
      options.TokenValidationParameters = new TokenValidationParameters
      {
          ValidateIssuerSigningKey = true,
          IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
          ValidateIssuer = true,
          ValidIssuer = jwtSettings.Issuer,
          ValidateAudience = true,
          ValidAudience = jwtSettings.Audience,
          ValidateLifetime = true,
          ClockSkew = TimeSpan.Zero,
      };
  });

builder.Services.AddControllers();

// Настройка Swagger/OpenAPI
builder.Services.AddOpenApiDocument(settings =>
{
    settings.Title = "NewsFeed API"; 
    settings.Description = "API для работы с новостной лентой";
});

builder.Services.AddOpenApi();

// кастомный обработчик ошибок
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ExceptionHandler>();

builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

// мониторим доступность баз данных
builder.Services.AddHealthChecks()
  .AddNpgSql(connectionString, name: "postgres")
  .AddRedis(redisConnectionString, name: "redis");

var app = builder.Build();

// метрики
app.MapMetrics("/metrics");

// отчитываемся о том что живы здоровы
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
});
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false,  // только проверка что приложение запущено
});

// применение миграций при старте
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var context = scope.ServiceProvider.GetRequiredService<EfContext>();

    try
    {
        logger.LogInformation("Применение миграций базы данных");
        await context.Database.MigrateAsync().ConfigureAwait(false);
        logger.LogInformation("Миграции успешно применены.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Ошибка при применении миграций");
        throw;
    }
}

if (app.Environment.IsDevelopment())
{
    app.Urls.Add("http://localhost:5058");
}

// свагер пусть будет и в продакшене
app.UseOpenApi();
app.UseSwaggerUi();

app.UseHttpsRedirection();

app.UseCors();

// кастомный обработчик ошибок
app.UseExceptionHandler();

// prometheus HTTP метрики
app.UseHttpMetrics();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();