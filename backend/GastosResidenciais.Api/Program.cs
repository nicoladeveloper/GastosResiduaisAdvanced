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
            // Serializa enums (Tipo: Receita/Despesa) como texto no JSON, em vez de número,
            // deixando o contrato da API mais legível para o front-end.
            options.JsonSerializerOptions.Converters.Add(
                new System.Text.Json.Serialization.JsonStringEnumConverter());
        });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // Persistência: SQLite salva os dados em um arquivo local (gastos.db),
    // garantindo que as informações sobrevivam ao reinício da aplicação.
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=gastos.db";

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite(connectionString));

    // CORS: permite que o front-end (React, rodando em outra porta/origem) consuma a API.
    // Em produção, as origens adicionais (ex.: URL do front-end hospedado) podem ser
    // definidas via configuração/variável de ambiente "Cors:AllowedOrigins" (array),
    // sem precisar alterar o código.
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

    // Hosts como o Render definem a porta a ser usada via variável de ambiente PORT.
    // Localmente essa variável não existe, então o app continua subindo na porta
    // padrão do .NET (ex.: http://localhost:5000).
    var renderPort = Environment.GetEnvironmentVariable("PORT");
    Console.WriteLine($"[startup] Variável PORT = '{renderPort}'");
    Console.Out.Flush();
    if (!string.IsNullOrEmpty(renderPort))
    {
        app.Urls.Add($"http://0.0.0.0:{renderPort}");
    }

    // Garante que o banco de dados SQLite (arquivo gastos.db) e suas tabelas existam
    // antes da aplicação começar a atender requisições. Como o arquivo fica em disco,
    // os dados persistem normalmente entre execuções/reinícios da aplicação.
    Console.WriteLine("[startup] Garantindo criação do banco SQLite...");
    Console.Out.Flush();
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();
    }
    Console.WriteLine("[startup] Banco OK. Configurando pipeline HTTP...");
    Console.Out.Flush();

    // ----- Pipeline HTTP -----

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // Deve ser o primeiro middleware da pipeline, para capturar exceções
    // lançadas por qualquer middleware/controller executado depois dele.
    app.UseGlobalExceptionHandling();

    app.UseCors(CorsPolicyName);
    app.UseAuthorization();
    app.MapControllers();

    Console.WriteLine("[startup] Pipeline configurado, chamando Run()...");
    Console.Out.Flush();

    app.Run();
}
catch (Exception ex)
{
    // Garante que qualquer exceção de inicialização seja impressa e "flushada"
    // antes do processo morrer, mesmo em ambientes com saída bufferizada.
    Console.Error.WriteLine("[startup] ERRO FATAL na inicialização:");
    Console.Error.WriteLine(ex);
    Console.Error.Flush();
    throw;
}
