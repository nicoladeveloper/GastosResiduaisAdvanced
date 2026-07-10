namespace GastosResidenciais.Api.DTOs;

/// <summary>Totais consolidados de uma única pessoa.</summary>
public class TotalPessoaDto
{
    public int PessoaId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal TotalReceitas { get; set; }
    public decimal TotalDespesas { get; set; }

    /// <summary>Saldo = Receitas - Despesas.</summary>
    public decimal Saldo { get; set; }
}

/// <summary>Resposta completa da consulta de totais: lista por pessoa + totais gerais.</summary>
public class TotalGeralDto
{
    public List<TotalPessoaDto> Pessoas { get; set; } = new();
    public decimal TotalReceitas { get; set; }
    public decimal TotalDespesas { get; set; }
    public decimal SaldoLiquido { get; set; }
}
