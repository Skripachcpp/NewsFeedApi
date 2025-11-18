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

builder.Services.AddScoped<ITagsRepository, TagsRepository>();
builder.Services.AddScoped<INewsRepository, NewsRepository>();

builder.Services.AddControllers();

builder.Services.AddOpenApiDocument();

builder.Services.AddOpenApi();

// кастомный обработчик ошибок
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ExceptionHandler>(); 

var app = builder.Build();

if (app.Environment.IsDevelopment()) {
  app.UseOpenApi();
  app.UseSwaggerUi();
}

app.UseHttpsRedirection();

// кастомный обработчик ошибок
app.UseExceptionHandler();

app.UseAuthorization();

app.MapControllers();

app.Run();
