using Microsoft.EntityFrameworkCore;
using Distribucion.Core.Interfaces;
using Distribucion.Infraestructura.Repositorio;
using Distribucion.Infraestructura.Data;

var builder = WebApplication.CreateBuilder(args);

// Puerto Railway
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://*:{port}");

// 🔥 Cadena de conexión fija a tu host real
var connectionString = "Host=yamanote.proxy.rlwy.net;Port=5432;Database=Distribucion;Username=postgres;Password=foqXkDDumQSNWvhKHRLOTFpfhxeGuGok;SSL Mode=Require;Trust Server Certificate=true";

// Configurar DbContext con Npgsql
builder.Services.AddDbContext<DistribucionContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(); // reintenta si falla la conexión
    }));

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("myApp", policibuilder =>
    {
        policibuilder.AllowAnyOrigin();
        policibuilder.AllowAnyHeader();
        policibuilder.AllowAnyMethod();
    });
});

// Controllers, Swagger y HttpClient
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

// Inyectar repositorios
builder.Services.AddScoped<IEnvioRepositorio, EnvioRepositorio>();
builder.Services.AddScoped<IDetalleEnvioRepositorio, DetalleEnvioRepositorio>();
builder.Services.AddScoped<ICamionRepositorio, CamionRepositorio>();

var app = builder.Build();

// Aplicar migraciones al iniciar
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DistribucionContext>();
    try
    {
        Console.WriteLine("Aplicando migraciones...");
        db.Database.Migrate(); // Crea las tablas si no existen
        Console.WriteLine("Migraciones aplicadas correctamente.");
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error aplicando migraciones: " + ex.Message);
        Console.WriteLine(ex.StackTrace);
    }
}

// Habilitar Swagger siempre
app.UseSwagger();
app.UseSwaggerUI();

// Middleware
app.UseCors("myApp");
app.UseAuthorization();
app.MapControllers();

app.Run();
