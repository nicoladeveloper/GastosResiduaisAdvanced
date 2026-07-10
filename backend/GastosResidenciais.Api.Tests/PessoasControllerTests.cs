using GastosResidenciais.Api.Controllers;
using GastosResidenciais.Api.DTOs;
using GastosResidenciais.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace GastosResidenciais.Api.Tests;

public class PessoasControllerTests
{
    [Fact]
    public async Task Criar_DeveGerarIdentificadorAutomaticamente()
    {
        // Arrange
        using var context = TestDbContextFactory.Criar();
        var controller = new PessoasController(context);

        // Act
        var resposta = await controller.Criar(new CreatePessoaDto { Nome = "Ana", Idade = 30 });

        // Assert
        var criado = Assert.IsType<CreatedAtActionResult>(resposta.Result);
        var pessoa = Assert.IsType<PessoaDto>(criado.Value);
        Assert.True(pessoa.Id > 0);
        Assert.Equal("Ana", pessoa.Nome);
        Assert.False(pessoa.MenorDeIdade);
    }

    [Fact]
    public async Task Deletar_DeveRemoverTodasAsTransacoesDaPessoaEmCascata()
    {
        // Arrange: uma pessoa com duas transações
        using var context = TestDbContextFactory.Criar();
        var pessoa = new Pessoa { Nome = "Carlos", Idade = 40 };
        context.Pessoas.Add(pessoa);
        await context.SaveChangesAsync();

        context.Transacoes.AddRange(
            new Transacao { Descricao = "Salário", Valor = 3000, Tipo = TipoTransacao.Receita, PessoaId = pessoa.Id },
            new Transacao { Descricao = "Aluguel", Valor = 1200, Tipo = TipoTransacao.Despesa, PessoaId = pessoa.Id }
        );
        await context.SaveChangesAsync();

        var controller = new PessoasController(context);

        // Act
        var resposta = await controller.Deletar(pessoa.Id);

        // Assert
        Assert.IsType<NoContentResult>(resposta);
        Assert.Empty(context.Pessoas);
        Assert.Empty(context.Transacoes); // regra de negócio: exclusão em cascata
    }

    [Fact]
    public async Task Deletar_PessoaInexistente_DeveRetornarNotFound()
    {
        using var context = TestDbContextFactory.Criar();
        var controller = new PessoasController(context);

        var resposta = await controller.Deletar(id: 999);

        Assert.IsType<NotFoundObjectResult>(resposta);
    }
}
