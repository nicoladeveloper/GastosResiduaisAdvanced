/**
 * Tipos que espelham os DTOs expostos pela API .NET.
 * Mantê-los centralizados aqui facilita o reuso entre os componentes
 * e garante que o front-end e o back-end "falem a mesma língua".
 */

export type TipoTransacao = "Receita" | "Despesa";

export interface Pessoa {
  id: number;
  nome: string;
  idade: number;
  menorDeIdade: boolean;
}

export interface NovaPessoa {
  nome: string;
  idade: number;
}

export interface Transacao {
  id: number;
  descricao: string;
  valor: number;
  tipo: TipoTransacao;
  pessoaId: number;
  pessoaNome: string;
}

export interface NovaTransacao {
  descricao: string;
  valor: number;
  tipo: TipoTransacao;
  pessoaId: number;
}

export interface TotalPessoa {
  pessoaId: number;
  nome: string;
  totalReceitas: number;
  totalDespesas: number;
  saldo: number;
}

export interface TotalGeral {
  pessoas: TotalPessoa[];
  totalReceitas: number;
  totalDespesas: number;
  saldoLiquido: number;
}

/** Formato de erro retornado pela API em respostas 400/404 ({ mensagem: "..." }). */
export interface ApiErro {
  mensagem: string;
}
