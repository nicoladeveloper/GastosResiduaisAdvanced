using GastosResidenciais.Api.Data;
using GastosResidenciais.Api.DTOs;
using GastosResidenciais.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GastosResidenciais.Api.Controllers;

/// <summary>
/// Endpoints para o cadastro de pessoas: criação, listagem e exclusão.
/// </summary>
[ApiController]
[Route("api/pessoas")]
public class PessoasController : ControllerBase
{
    private readonly AppDbContext _context;

    public PessoasController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>Lista todas as pessoas cadastradas, ordenadas por nome.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PessoaDto>>> Listar()
    {
        var pessoas = await _context.Pessoas
            .OrderBy(p => p.Nome)
            .Select(p => new PessoaDto
            {
                Id = p.Id,
                Nome = p.Nome,
                Idade = p.Idade,
                MenorDeIdade = p.MenorDeIdade
            })
            .ToListAsync();

        return Ok(pessoas);
    }

    /// <summary>Cadastra uma nova pessoa. O Id é gerado automaticamente pelo banco.</summary>
    [HttpPost]
    public async Task<ActionResult<PessoaDto>> Criar([FromBody] CreatePessoaDto dto)
    {
        // [ApiController] já valida automaticamente as DataAnnotations do DTO
        // (nome obrigatório, idade entre 0 e 150) e retorna 400 em caso de erro.

        var pessoa = new Pessoa
        {
            Nome = dto.Nome.Trim(),
            Idade = dto.Idade
        };

        _context.Pessoas.Add(pessoa);
        await _context.SaveChangesAsync();

        var resultado = new PessoaDto
        {
            Id = pessoa.Id,
            Nome = pessoa.Nome,
            Idade = pessoa.Idade,
            MenorDeIdade = pessoa.MenorDeIdade
        };

        return CreatedAtAction(nameof(Listar), new { id = pessoa.Id }, resultado);
    }

    /// <summary>
    /// Remove uma pessoa. Graças à configuração de cascata no AppDbContext,
    /// todas as transações vinculadas a essa pessoa são removidas automaticamente.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Deletar(int id)
    {
        var pessoa = await _context.Pessoas.FindAsync(id);

        if (pessoa is null)
        {
            return NotFound(new { mensagem = "Pessoa não encontrada." });
        }

        _context.Pessoas.Remove(pessoa);
        await _context.SaveChangesAsync(); // dispara a exclusão em cascata das transações

        return NoContent();
    }
}
