namespace GastosResidenciais.Api.Models;

/// <summary>
/// Entidade que representa um morador/pessoa cadastrada no controle de gastos residenciais.
/// </summary>
public class Pessoa
{
    /// <summary>Identificador único, gerado automaticamente pelo banco de dados (auto-increment).</summary>
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public int Idade { get; set; }

    /// <summary>
    /// Propriedade calculada (não persistida) que indica se a pessoa é menor de idade.
    /// Centraliza a regra "menor de 18 anos" em um único lugar, evitando espalhar
    /// o número mágico "18" pelo restante do código.
    /// </summary>
    public bool MenorDeIdade => Idade < 18;

    /// <summary>
    /// Transações associadas a essa pessoa. O EF Core, configurado no AppDbContext com
    /// DeleteBehavior.Cascade, apaga automaticamente todas as transações relacionadas
    /// quando a pessoa é excluída - atendendo à regra de negócio de exclusão em cascata.
    /// </summary>
    public List<Transacao> Transacoes { get; set; } = new();
}
