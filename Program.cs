using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Distribucion.Core.Interfaces;
using Distribucion.Infraestructura.Repositorio;
using Distribucion.Infraestructura.Data;

var builder = WebApplication.CreateBuilder(args);

// Leer DATABASE_URL de Railway (ya viene como connection string)
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");

// Si no existe, intentar leer desde appsettings.json
if (string.IsNullOrEmpty(connectionString))
{
    connectionString = builder.Configuration.GetConnectionString("DistribucionContext");
}

// Inyectar el contexto
builder.Services.AddDbContext<DistribucionContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddCors(options =>
{
    options.AddPolicy("myApp", policibuilder =>
    {
        policibuilder.AllowAnyOrigin();
        policibuilder.AllowAnyHeader();
        policibuilder.AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

builder.Services.AddScoped<IEnvioRepositorio, EnvioRepositorio>();
builder.Services.AddScoped<IDetalleEnvioRepositorio, DetalleEnvioRepositorio>();

var app = builder.Build();

// Aplicar migraciones automáticamente al iniciar
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DistribucionContext>();
    db.Database.Migrate();
}

app.UseCors("myApp");
app.UseAuthorization();
app.MapControllers();
app.Run();
