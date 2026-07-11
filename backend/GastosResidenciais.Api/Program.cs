using GastosResidenciais.Api.Data;
using GastosResidenciais.Api.Middleware;
using Microsoft.EntityFrameworkCore;
using System.Linq;

// Log de diagnóstico: confirma que o processo chegou a executar código
// gerenciado antes de qualquer outra coisa (útil para depurar falhas de
// inicialização em ambientes de hospedagem).
Console.WriteLine("[startup] Iniciando aplicação...");
Console.Out.Flush();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ----- Serviços -----

    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(
                new System.Text.Json.Serialization.JsonStringEnumConverter());
        });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=gastos.db";

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite(connectionString));

    const string CorsPolicyName = "FrontendPolicy";
    var defaultOrigins = new[] { "http://localhost:5173", "http://localhost:3000" };
    var configuredOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
    var allowedOrigins = (configuredOrigins is { Length: > 0 })
        ? defaultOrigins.Concat(configuredOrigins).Distinct().ToArray()
        : defaultOrigins;

    builder.Services.AddCors(options =>
    {
        options.AddPolicy(CorsPolicyName, policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });

    Console.WriteLine("[startup] Serviços registrados, iniciando build do host...");
    Console.Out.Flush();

    var app = builder.Build();

    var renderPort = Environment.GetEnvironmentVariable("PORT");
    Console.WriteLine($"[startup] Variável PORT = '{renderPort}'");
    Console.Out.Flush();
    if (!string.IsNullOrEmpty(renderPort))
    {
        app.Urls.Add($"http://0.0.0.0:{renderPort}");
    }

    Console.WriteLine("[startup] Garantindo criação do banco SQLite...");
    Console.Out.Flush();
    using (var