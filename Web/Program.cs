using Domain.Interfaces;
using Infrastructure;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
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

builder.Services.AddControllers();

builder.Services.AddOpenApiDocument();

builder.Services.AddOpenApi();

// кастомный обработчик ошибок
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ExceptionHandler>(); 

var app = builder.Build();

// свагер пусть будет и в продакшене
app.UseOpenApi();
app.UseSwaggerUi();

app.UseHttpsRedirection();

app.UseCors();

// кастомный обработчик ошибок
app.UseExceptionHandler();

app.UseAuthorization();

app.MapControllers();

app.Run();
