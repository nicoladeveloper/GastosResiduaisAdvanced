using GastosResidenciais.Api.Data;
using GastosResidenciais.Api.DTOs;
using GastosResidenciais.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GastosResidenciais.Api.Controllers;

/// <summary>
/// Endpoints para o cadastro de transações financeiras: criação e listagem.
/// Conforme especificação, não há edição nem exclusão de transações.
/// </summary>
[ApiController]
[Route("api/transacoes")]
public class TransacoesController : ControllerBase
{
    private readonly AppDbContext _context;

    public TransacoesController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Lista todas as transações cadastradas, das mais recentes para as mais antigas.
    /// Opcionalmente pode ser filtrada por pessoa através do query param "pessoaId".
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TransacaoDto>>> Listar([FromQuery] int? pessoaId)
    {
        var query = _context.Transacoes
            .Include(t => t.Pessoa)
            .AsQueryable();

        if (pessoaId.HasValue)
        {
            query = query.Where(t => t.PessoaId == pessoaId.Value);
        }

        var transacoes = await query
            .OrderByDescending(t => t.Id)
            .Select(t => new TransacaoDto
            {
                Id = t.Id,
                Descricao = t.Descricao,
                Valor = t.Valor,
                Tipo = t.Tipo,
                PessoaId = t.PessoaId,
                PessoaNome = t.Pessoa!.Nome
            })
            .ToListAsync();

        return Ok(transacoes);
    }

    /// <summary>
    /// Cadastra uma nova transação.
    /// Regras de negócio aplicadas:
    ///   1) A pessoa informada (PessoaId) precisa existir previamente no cadastro.
    ///   2) Se a pessoa for menor de idade (menor de 18 anos), somente transações
    ///      do tipo Despesa podem ser cadastradas para ela.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TransacaoDto>> Criar([FromBody] CreateTransacaoDto dto)
    {
        var pessoa = await _context.Pessoas.FindAsync(dto.PessoaId);

        if (pessoa is null)
        {
            // Regra: o identificador de pessoa informado precisa existir no cadastro.
            return BadRequest(new { mensagem = "Pessoa informada não existe no cadastro." });
        }

        if (pessoa.MenorDeIdade && dto.Tipo == TipoTransacao.Receita)
        {
            // Regra: pessoa menor de idade só pode ter despesas cadastradas.
            return BadRequest(new
            {
                mensagem = $"{pessoa.Nome} é menor de idade ({pessoa.Idade} anos) e por isso só pode ter transações do tipo Despesa."
            });
        }

        var transacao = new Transacao
        {
            Descricao = dto.Descricao.Trim(),
            Valor = dto.Valor,
            Tipo = dto.Tipo,
            PessoaId = dto.PessoaId
        };

        _context.Transacoes.Add(transacao);
        await _context.SaveChangesAsync();

        var resultado = new TransacaoDto
        {
            Id = transacao.Id,
            Descricao = transacao.Descricao,
            Valor = transacao.Valor,
            Tipo = transacao.Tipo,
            PessoaId = transacao.PessoaId,
            PessoaNome = pessoa.Nome
        };

        return CreatedAtAction(nameof(Listar), new { id = transacao.Id }, resultado);
    }
}
