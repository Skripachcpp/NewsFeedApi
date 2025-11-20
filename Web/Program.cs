using System.Text;
using Domain.Interfaces;
using Infrastructure;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Web.Application;

var builder = WebApplication.CreateBuilder(args);

// bd _
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (connectionString == null) throw new Exception("Отсутствует connection string");
builder.Services.AddDbContext<EfContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddScoped<DpContext>(_ => new DpContext(connectionString));
// bd ^

builder.Services.AddCors(options => {
  options.AddDefaultPolicy(policy => {
    policy.AllowAnyOrigin()
      .AllowAnyMethod()
      .AllowAnyHeader();
  });
});

builder.Services.AddScoped<ITagsRepository, TagsRepository>();
builder.Services.AddScoped<INewsRepository, NewsRepository>();

// настройки авторизации
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"]!;

builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
  })
  .AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters {
      ValidateIssuerSigningKey = true,
      IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
      ValidateIssuer = true,
      ValidIssuer = jwtSettings["Issuer"],
      ValidateAudience = true,
      ValidAudience = jwtSettings["Audience"],
      ValidateLifetime = true,
      ClockSkew = TimeSpan.Zero
    };
  });

builder.Services.AddControllers();

builder.Services.AddOpenApiDocument(); // swagger

builder.Services.AddOpenApi();

// кастомный обработчик ошибок
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ExceptionHandler>();

builder.Services.Configure<RouteOptions>(options => {
  options.LowercaseUrls = true;
  options.LowercaseQueryStrings = true;
});

var app = builder.Build();

// применение миграций при старте
using (var scope = app.Services.CreateScope()) {
  var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
  var context = scope.ServiceProvider.GetRequiredService<EfContext>();

  try {
    logger.LogInformation("Применение миграций базы данных");
    await context.Database.MigrateAsync();
    logger.LogInformation("Миграции успешно применены.");
  }
  catch (Exception ex) {
    logger.LogError(ex, "Ошибка при применении миграций");
    throw;
  }
}

if (app.Environment.IsDevelopment()) {
  app.Urls.Add("http://localhost:5058");
}

// свагер пусть будет и в продакшене
app.UseOpenApi();
app.UseSwaggerUi();

app.UseHttpsRedirection();

app.UseCors();

// кастомный обработчик ошибок
app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();