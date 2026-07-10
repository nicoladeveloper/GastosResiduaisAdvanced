using GastosResidenciais.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace GastosResidenciais.Api.Tests;

/// <summary>
/// Cria uma instância isolada de AppDbContext usando o provider "InMemory" do EF Core.
/// Cada teste recebe um banco com nome único (Guid), garantindo que um teste nunca
/// interfira no estado de outro, mesmo rodando em paralelo.
/// </summary>
public static class TestDbContextFactory
{
    public static AppDbContext Criar()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
