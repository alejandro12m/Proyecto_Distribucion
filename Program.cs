using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Distribucion.Core.Interfaces;
using Distribucion.Infraestructura.Repositorio;
using Distribucion.Infraestructura.Data;

var builder = WebApplication.CreateBuilder(args);

// 1️⃣ Intentar usar variables de Railway (PGHOST, PGUSER, etc)
string? host = Environment.GetEnvironmentVariable("PGHOST");
string? port = Environment.GetEnvironmentVariable("PGPORT");
string? user = Environment.GetEnvironmentVariable("PGUSER");
string? pass = Environment.GetEnvironmentVariable("PGPASSWORD");
string? dbname = Environment.GetEnvironmentVariable("PGDATABASE");

string connectionString;

if (!string.IsNullOrEmpty(host))
{
    // 2️⃣ Railway nos dio las variables correctamente
    connectionString =
        $"Host={host};Port={port};Database={dbname};Username={user};Password={pass};SSL Mode=Require;Trust Server Certificate=True";
}
else
{
    // 3️⃣ Modo local
    connectionString = builder.Configuration.GetConnectionString("DistribucionContext")
                       ?? throw new Exception("No se encontró cadena de conexión.");
}

Console.WriteLine("Conectando a PostgreSQL con:");
Console.WriteLine(connectionString);

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

var app = builder.Build();

// Aplicar migraciones
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
        Console.WriteLine("Error aplicando migraciones: " + ex.Message);
    }
}

// Middleware
app.UseCors("myApp");
app.UseAuthorization();
app.MapControllers();
app.Run();
