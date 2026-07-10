using GastosResidenciais.Api.Controllers;
using GastosResidenciais.Api.DTOs;
using GastosResidenciais.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace GastosResidenciais.Api.Tests;

public class RelatoriosControllerTests
{
    [Fact]
    public async Task ObterTotais_DeveCalcularReceitasDespesasESaldoPorPessoa()
    {
        // Arrange: duas pessoas, cada uma com receitas e despesas distintas.
        using var context = TestDbContextFactory.Criar();

        var ana = new Pessoa { Nome = "Ana", Idade = 30 };
        var bruno = new Pessoa { Nome = "Bruno", Idade = 40 };
        context.Pessoas.AddRange(ana, bruno);
        await context.SaveChangesAsync();

        context.Transacoes.AddRange(
            new Transacao { Descricao = "Salário", Valor = 5000, Tipo = TipoTransacao.Receita, PessoaId = ana.Id },
            new Transacao { Descricao = "Mercado", Valor = 800, Tipo = TipoTransacao.Despesa, PessoaId = ana.Id },
            new Transacao { Descricao = "Freelance", Valor = 1000, Tipo = TipoTransacao.Receita, PessoaId = bruno.Id },
            new Transacao { Descricao = "Aluguel", Valor = 1500, Tipo = TipoTransacao.Despesa, PessoaId = bruno.Id }
        );
        await context.SaveChangesAsync();

        var controller = new RelatoriosController(context);

        // Act
        var resposta = await controller.ObterTotais();

        // Assert
        var ok = Assert.IsType<OkObjectResult>(resposta.Result);
        var totais = Assert.IsType<TotalGeralDto>(ok.Value);

        var totalAna = Assert.Single(totais.Pessoas, p => p.Nome == "Ana");
        Assert.Equal(5000, totalAna.TotalReceitas);
        Assert.Equal(800, totalAna.TotalDespesas);
        Assert.Equal(4200, totalAna.Saldo);

        var totalBruno = Assert.Single(totais.Pessoas, p => p.Nome == "Bruno");
        Assert.Equal(1000, totalBruno.TotalReceitas);
        Assert.Equal(1500, totalBruno.TotalDespesas);
        Assert.Equal(-500, totalBruno.Saldo); // saldo pode ser negativo

        // Total geral = soma de todas as pessoas
        Assert.Equal(6000, totais.TotalReceitas);
        Assert.Equal(2300, totais.TotalDespesas);
        Assert.Equal(3700, totais.SaldoLiquido);
    }

    [Fact]
    public async Task ObterTotais_PessoaSemTransacoes_DeveAparecerComTotaisZerados()
    {
        using var context = TestDbContextFactory.Criar();
        context.Pessoas.Add(new Pessoa { Nome = "Sem Movimentação", Idade = 22 });
        await context.SaveChangesAsync();

        var controller = new RelatoriosController(context);
        var resposta = await controller.ObterTotais();

        var ok = Assert.IsType<OkObjectResult>(resposta.Result);
        var totais = Assert.IsType<TotalGeralDto>(ok.Value);

        var pessoa = Assert.Single(totais.Pessoas);
        Assert.Equal(0, pessoa.TotalReceitas);
        Assert.Equal(0, pessoa.TotalDespesas);
        Assert.Equal(0, pessoa.Saldo);
    }
}
