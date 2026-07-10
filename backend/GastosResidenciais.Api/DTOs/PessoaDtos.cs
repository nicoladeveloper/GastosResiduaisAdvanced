using System.ComponentModel.DataAnnotations;

namespace GastosResidenciais.Api.DTOs;

/// <summary>Dados necessários para cadastrar uma nova pessoa.</summary>
public class CreatePessoaDto
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [MaxLength(120, ErrorMessage = "O nome deve ter no máximo 120 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Range(0, 150, ErrorMessage = "A idade deve estar entre 0 e 150 anos.")]
    public int Idade { get; set; }
}

/// <summary>Representação de uma pessoa retornada pela API.</summary>
public class PessoaDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int Idade { get; set; }
    public bool MenorDeIdade { get; set; }
}
