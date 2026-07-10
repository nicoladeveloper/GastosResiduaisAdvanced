using GastosResidenciais.Api.Data;
using GastosResidenciais.Api.Middleware;
using Microsoft.EntityFrameworkCore;

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
const string CorsPolicyName = "FrontendPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173", // Vite (dev server padrão)
                "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Garante que o banco de dados SQLite (arquivo gastos.db) e suas tabelas existam
// antes da aplicação começar a atender requisições. Como o arquivo fica em disco,
// os dados persistem normalmente entre execuções/reinícios da aplicação.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

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

app.Run();
