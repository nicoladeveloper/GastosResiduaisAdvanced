using GastosResidenciais.Api.Controllers;
using GastosResidenciais.Api.DTOs;
using GastosResidenciais.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace GastosResidenciais.Api.Tests;

public class TransacoesControllerTests
{
    private static async Task<Pessoa> CriarPessoaAsync(GastosResidenciais.Api.Data.AppDbContext context, string nome, int idade)
    {
        var pessoa = new Pessoa { Nome = nome, Idade = idade };
        context.Pessoas.Add(pessoa);
        await context.SaveChangesAsync();
        return pessoa;
    }

    [Fact]
    public async Task Criar_PessoaMenorDeIdade_ComReceita_DeveSerRejeitada()
    {
        // Regra de negócio central da especificação: menor de 18 anos só pode ter despesas.
        using var context = TestDbContextFactory.Criar();
        var crianca = await CriarPessoaAsync(context, "João", 15);
        var controller = new TransacoesController(context);

        var resposta = await controller.Criar(new CreateTransacaoDto
        {
            Descricao = "Mesada",
            Valor = 50,
            Tipo = TipoTransacao.Receita,
            PessoaId = crianca.Id
        });

        var badRequest = Assert.IsType<BadRequestObjectResult>(resposta.Result);
        Assert.Empty(context.Transacoes); // nada deve ter sido persistido
    }

    [Fact]
    public async Task Criar_PessoaMenorDeIdade_ComDespesa_DeveSerAceita()
    {
        using var context = TestDbContextFactory.Criar();
        var crianca = await CriarPessoaAsync(context, "Maria", 10);
        var controller = new TransacoesController(context);

        var resposta = await controller.Criar(new CreateTransacaoDto
        {
            Descricao = "Material escolar",
            Valor = 80,
            Tipo = TipoTransacao.Despesa,
            PessoaId = crianca.Id
        });

        var criado = Assert.IsType<CreatedAtActionResult>(resposta.Result);
        var transacao = Assert.IsType<TransacaoDto>(criado.Value);
        Assert.Equal(TipoTransacao.Despesa, transacao.Tipo);
    }

    [Fact]
    public async Task Criar_PessoaMaiorDeIdade_ComReceita_DeveSerAceita()
    {
        using var context = TestDbContextFactory.Criar();
        var adulto = await CriarPessoaAsync(context, "Pedro", 25);
        var controller = new TransacoesController(context);

        var resposta = await controller.Criar(new CreateTransacaoDto
        {
            Descricao = "Salário",
            Valor = 4000,
            Tipo = TipoTransacao.Receita,
            PessoaId = adulto.Id
        });

        Assert.IsType<CreatedAtActionResult>(resposta.Result);
    }

    [Fact]
    public async Task Criar_ComPessoaInexistente_DeveRetornarBadRequest()
    {
        using var context = TestDbContextFactory.Criar();
        var controller = new TransacoesController(context);

        var resposta = await controller.Criar(new CreateTransacaoDto
        {
            Descricao = "Compra qualquer",
            Valor = 10,
            Tipo = TipoTransacao.Despesa,
            PessoaId = 12345 // não existe no cadastro
        });

        Assert.IsType<BadRequestObjectResult>(resposta.Result);
    }
}
