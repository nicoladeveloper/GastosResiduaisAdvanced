import type {
  NovaPessoa,
  NovaTransacao,
  Pessoa,
  Transacao,
  TotalGeral,
} from "../types/types";

// URL base da API .NET. Pode ser sobrescrita via variável de ambiente do Vite
// (arquivo .env: VITE_API_URL=http://localhost:5000/api).
const API_URL = import.meta.env.VITE_API_URL ?? "http://localhost:5000/api";

/**
 * Erro customizado usado para propagar mensagens de negócio vindas da API
 * (ex.: "pessoa menor de idade só pode ter despesas") até a interface.
 */
export class ApiError extends Error {}

/** Função auxiliar central: faz a chamada HTTP e trata erros de forma consistente. */
async function request<T>(path: string, options?: RequestInit): Promise<T> {
  const response = await fetch(`${API_URL}${path}`, {
    headers: { "Content-Type": "application/json" },
    ...options,
  });

  if (!response.ok) {
    // A API retorna { mensagem: "..." } para erros de validação/negócio.
    const corpo = await response.json().catch(() => null);
    const mensagem = corpo?.mensagem ?? `Erro inesperado (HTTP ${response.status}).`;
    throw new ApiError(mensagem);
  }

  // Respostas 204 (No Content) não têm corpo para converter em JSON.
  if (response.status === 204) {
    return undefined as T;
  }

  return response.json() as Promise<T>;
}

// ----- Pessoas -----

export const pessoasApi = {
  listar: () => request<Pessoa[]>("/pessoas"),

  criar: (dados: NovaPessoa) =>
    request<Pessoa>("/pessoas", {
      method: "POST",
      body: JSON.stringify(dados),
    }),

  deletar: (id: number) =>
    request<void>(`/pessoas/${id}`, { method: "DELETE" }),
};

// ----- Transações -----

export const transacoesApi = {
  listar: () => request<Transacao[]>("/transacoes"),

  criar: (dados: NovaTransacao) =>
    request<Transacao>("/transacoes", {
      method: "POST",
      body: JSON.stringify(dados),
    }),
};

// ----- Relatórios / Totais -----

export const relatoriosApi = {
  obterTotais: () => request<TotalGeral>("/relatorios/totais"),
};
