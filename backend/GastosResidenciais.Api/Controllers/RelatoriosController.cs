using GastosResidenciais.Api.Data;
using GastosResidenciais.Api.DTOs;
using GastosResidenciais.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GastosResidenciais.Api.Controllers;

/// <summary>
/// Endpoint de consulta de totais: totais por pessoa (receitas, despesas, saldo)
/// e o total geral consolidado de todas as pessoas.
/// </summary>
[ApiController]
[Route("api/relatorios")]
public class RelatoriosController : ControllerBase
{
    private readonly AppDbContext _context;

    public RelatoriosController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retorna, para cada pessoa cadastrada, o total de receitas, o total de despesas
    /// e o saldo (receitas - despesas), além do total geral somando todas as pessoas.
    /// Pessoas sem nenhuma transação aparecem na lista com todos os totais zerados.
    /// </summary>
    [HttpGet("totais")]
    public async Task<ActionResult<TotalGeralDto>> ObterTotais()
    {
        // Carregamos as pessoas junto com suas transações para calcular os totais em memória.
        // Para o volume de dados de um controle de gastos residenciais isso é simples e eficiente;
        // se a base crescesse muito, o agrupamento poderia ser feito diretamente na consulta SQL.
        var pessoas = await _context.Pessoas
            .Include(p => p.Transacoes)
            .OrderBy(p => p.Nome)
            .ToListAsync();

        var totaisPorPessoa = pessoas.Select(p =>
        {
            var totalReceitas = p.Transacoes
                .Where(t => t.Tipo == TipoTransacao.Receita)
                .Sum(t => t.Valor);

            var totalDespesas = p.Transacoes
                .Where(t => t.Tipo == TipoTransacao.Despesa)
                .Sum(t => t.Valor);

            return new TotalPessoaDto
            {
                PessoaId = p.Id,
                Nome = p.Nome,
                TotalReceitas = totalReceitas,
                TotalDespesas = totalDespesas,
                Saldo = totalReceitas - totalDespesas
            };
        }).ToList();

        var resultado = new TotalGeralDto
        {
            Pessoas = totaisPorPessoa,
            TotalReceitas = totaisPorPessoa.Sum(t => t.TotalReceitas),
            TotalDespesas = totaisPorPessoa.Sum(t => t.TotalDespesas),
            SaldoLiquido = totaisPorPessoa.Sum(t => t.Saldo)
        };

        return Ok(resultado);
    }
}
