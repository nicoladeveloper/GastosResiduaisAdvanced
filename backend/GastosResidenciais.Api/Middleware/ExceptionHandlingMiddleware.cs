using System.Net;
using System.Text.Json;

namespace GastosResidenciais.Api.Middleware;

/// <summary>
/// Middleware que captura qualquer exceção não tratada na pipeline e converte
/// numa resposta JSON consistente ({ mensagem: "..." }), no mesmo formato usado
/// pelos controllers para erros de validação (400/404).
///
/// Sem isso, uma exceção inesperada (ex.: falha de conexão com o banco) vazaria
/// como uma página HTML de erro do ASP.NET, ou um 500 sem corpo — ruim tanto para
/// depuração quanto para quem consome a API.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro não tratado ao processar {Metodo} {Caminho}",
                context.Request.Method, context.Request.Path);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var corpo = JsonSerializer.Serialize(new
            {
                mensagem = "Ocorreu um erro inesperado ao processar a requisição."
            });

            await context.Response.WriteAsync(corpo);
        }
    }
}

/// <summary>Extensão para registrar o middleware de forma legível no Program.cs.</summary>
public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
        => app.UseMiddleware<ExceptionHandlingMiddleware>();
}
