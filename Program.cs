using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Distribucion.Core.Interfaces;
using Distribucion.Infraestructura.Repositorio;
using Distribucion.Infraestructura.Data;

var builder = WebApplication.CreateBuilder(args);

// Obtener cadena de conexión desde Railway
var connectionString =
    Environment.GetEnvironmentVariable("CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("DistribucionContext");

// Configurar DbContext
builder.Services.AddDbContext<DistribucionContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure();
    })
);

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("myApp", policibuilder =>
    {
        policibuilder.AllowAnyOrigin();
        policibuilder.AllowAnyHeader();
        policibuilder.AllowAnyMethod();
    });
});

// Swagger, Controllers, HttpClient
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
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error aplicando migraciones: " + ex.Message);
    }
}

// Configurar puerto de Railway
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://0.0.0.0:{port}");

// Swagger SIEMPRE activado
app.UseSwagger();
app.UseSwaggerUI();

// Middleware 
app.UseCors("myApp");
app.UseAuthorization();
app.MapControllers();

app.Run();
