using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Distribucion.Core.Interfaces;
using Distribucion.Infraestructura.Repositorio;
using Distribucion.Infraestructura.Data;

var builder = WebApplication.CreateBuilder(args);

// 1️⃣ Leemos la variable que Railway inyecta: DATABASE_URL
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

// 2️⃣ Si no existe, tomamos la conexión interna directa (Railway internal)
if (string.IsNullOrEmpty(databaseUrl))
{
    databaseUrl = "Host=postgres.railway.internal;Port=5432;Database=railway;Username=postgres;Password=foqXkDDumQSNWvhKHRLOTFpfhxeGuGok;SSL Mode=Require;Trust Server Certificate=true";
}

// 3️⃣ Registrar DbContext
builder.Services.AddDbContext<DistribucionContext>(options =>
    options.UseNpgsql(databaseUrl));

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("myApp", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

// Repositorios
builder.Services.AddScoped<IEnvioRepositorio, EnvioRepositorio>();
builder.Services.AddScoped<IDetalleEnvioRepositorio, DetalleEnvioRepositorio>();

var app = builder.Build();

// 4️⃣ Migraciones automáticas
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DistribucionContext>();
    try
    {
        db.Database.Migrate();
        Console.WriteLine("Migraciones aplicadas correctamente.");
    }
    catch (Exception ex)
    {
        Console.WriteLine("ERROR aplicando migraciones: " + ex.Message);
        Console.WriteLine(ex.StackTrace);
    }
}

app.UseCors("myApp");
app.UseAuthorization();
app.MapControllers();
app.Run();
