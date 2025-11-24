using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Distribucion.Core.Interfaces;
using Distribucion.Infraestructura.Repositorio;
using Distribucion.Infraestructura.Data;

var builder = WebApplication.CreateBuilder(args);

// Obtener cadena de conexión desde variable de entorno o appsettings.json
var connectionString =
    Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("DistribucionContext");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("No se encontró la cadena de conexión.");
}

// Registrar DbContext
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

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

// Middleware
app.UseCors("myApp");
app.UseAuthorization();
app.MapControllers();
app.Run();
