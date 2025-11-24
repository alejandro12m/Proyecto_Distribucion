using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Distribucion.Core.Interfaces;
using Distribucion.Infraestructura.Repositorio;
using Distribucion.Infraestructura.Data;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://*:{port}");

// Leer cadena de conexión desde variable de entorno (más seguro)
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? "Host=yamanote.proxy.rlwy.net;Port=5432;Database=Distribucion;Username=postgres;Password=foqXkDDumQSNWvhKHRLOTFpfhxeGuGok;SSL Mode=Require;Trust Server Certificate=true";

// Registrar DbContext con Npgsql
builder.Services.AddDbContext<DistribucionContext>(options =>
    options.UseNpgsql(connectionString));

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("myApp", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

// Controllers, Swagger y HttpClient
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

// Repositorios
builder.Services.AddScoped<IEnvioRepositorio, EnvioRepositorio>();
builder.Services.AddScoped<IDetalleEnvioRepositorio, DetalleEnvioRepositorio>();
builder.Services.AddScoped<ICamionRepositorio, CamionRepositorio>();

var app = builder.Build();

// Aplicar migraciones automáticamente
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DistribucionContext>();
    try
    {
        Console.WriteLine("Aplicando migraciones...");
        db.Database.Migrate();
        Console.WriteLine("Migraciones aplicadas correctamente.");
    }
    catch (Exception ex)
    {
        Console.WriteLine("ERROR aplicando migraciones: " + ex.Message);
        Console.WriteLine(ex.StackTrace);
    }
}

// Swagger habilitado siempre
app.UseSwagger();
app.UseSwaggerUI();

// Middleware
app.UseCors("myApp");
app.UseAuthorization();
app.MapControllers();
app.Run();
