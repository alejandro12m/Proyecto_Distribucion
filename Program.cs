using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Distribucion.Core.Interfaces;
using Distribucion.Infraestructura.Repositorio;
using Distribucion.Infraestructura.Data;
//DistribucionContext

var url = Environment.GetEnvironmentVariable("DATABASE_URL");
var builder WebApplication.CreateBuilder(args);
builder. Services. AddDbContext<DistribucionContext>(options =>
options. UseNpgsql(url));
// Add services to the container.
builder. Services. AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder. Services.AddEndpointsApiExplorer();
builder. Services.AddSwaggerGen();
var app builder. Build();
using (var scope app. Services.CreateScope())
var db = scope. ServiceProvider. GetRequiredService<DistribucionContext>();
db. Database. Migrate();
}
app.UseSwagger();
app. UseSwaggerUI();
app. UseAuthorization();
app. MapControllers();
app.Run();
