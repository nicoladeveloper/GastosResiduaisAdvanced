namespace GastosResidenciais.Api.Models;

/// <summary>
/// Representa a natureza de uma transação financeira.
/// Usar um enum (em vez de string livre) evita valores inválidos e
/// deixa a regra de negócio (menor de idade só pode ter Despesa) fácil de validar.
/// </summary>
public enum TipoTransacao
{
    Receita = 0,
    Despesa = 1
}
