using GastosResidenciais.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GastosResidenciais.Api.Data;

/// <summary>
/// Contexto do Entity Framework Core responsável por mapear as entidades do domínio
/// para o banco de dados SQLite e por persistir os dados em disco (arquivo gastos.db),
/// garantindo que as informações não se percam quando a aplicação for encerrada.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Pessoa> Pessoas => Set<Pessoa>();
    public DbSet<Transacao> Transacoes => Set<Transacao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ----- Pessoa -----
        modelBuilder.Entity<Pessoa>(entity =>
        {
            entity.Property(p => p.Nome)
                  .IsRequired()
                  .HasMaxLength(120);
        });

        // ----- Transacao -----
        modelBuilder.Entity<Transacao>(entity =>
        {
            entity.Property(t => t.Descricao)
                  .IsRequired()
                  .HasMaxLength(200);

            // decimal(18,2) evita problemas de arredondamento com valores monetários.
            entity.Property(t => t.Valor)
                  .HasColumnType("decimal(18,2)");

            // Regra de negócio: ao excluir uma pessoa, todas as suas transações
            // devem ser excluídas junto (exclusão em cascata).
            entity.HasOne(t => t.Pessoa)
                  .WithMany(p => p.Transacoes)
                  .HasForeignKey(t => t.PessoaId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        base.OnModelCreating(modelBuilder);
    }
}
