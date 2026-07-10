namespace GastosResidenciais.Api.Models;

/// <summary>
/// Entidade que representa uma transação financeira (receita ou despesa)
/// vinculada a uma pessoa.
/// </summary>
public class Transacao
{
    /// <summary>Identificador único, gerado automaticamente pelo banco de dados (auto-increment).</summary>
    public int Id { get; set; }

    public string Descricao { get; set; } = string.Empty;

    public decimal Valor { get; set; }

    public TipoTransacao Tipo { get; set; }

    /// <summary>Chave estrangeira: identificador da pessoa dona da transação.</summary>
    public int PessoaId { get; set; }

    /// <summary>Propriedade de navegação do EF Core para a pessoa relacionada.</summary>
    public Pessoa? Pessoa { get; set; }
}
