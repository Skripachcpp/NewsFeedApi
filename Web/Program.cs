using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// bd _
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (connectionString == null) throw new Exception("Отсутствует connection string");
builder.Services.AddDbContext<EfContext>(options => options.UseNpgsql(connectionString));
// bd ^

builder.Services.AddControllers();

builder.Services.AddOpenApiDocument();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment()) {
  app.UseOpenApi();
  app.UseSwaggerUi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();