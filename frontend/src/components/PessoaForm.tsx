import { useState } from "react";
import type { FormEvent } from "react";
import { pessoasApi, ApiError } from "../api/api";
import type { Pessoa } from "../types/types";

interface PessoaFormProps {
  /** Chamado após o cadastro ser criado com sucesso, para o pai atualizar a lista. */
  onCriada: (pessoa: Pessoa) => void;
}

/**
 * Formulário simples para cadastrar uma nova pessoa (nome + idade).
 * O identificador é gerado automaticamente pelo back-end, então não aparece aqui.
 */
export function PessoaForm({ onCriada }: PessoaFormProps) {
  const [nome, setNome] = useState("");
  const [idade, setIdade] = useState("");
  const [erro, setErro] = useState<string | null>(null);
  const [enviando, setEnviando] = useState(false);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setErro(null);

    if (!nome.trim()) {
      setErro("Informe o nome da pessoa.");
      return;
    }

    const idadeNumero = Number(idade);
    if (idade === "" || Number.isNaN(idadeNumero) || idadeNumero < 0) {
      setErro("Informe uma idade válida.");
      return;
    }

    setEnviando(true);
    try {
      const pessoaCriada = await pessoasApi.criar({ nome: nome.trim(), idade: idadeNumero });
      onCriada(pessoaCriada);
      setNome("");
      setIdade("");
    } catch (err) {
      setErro(err instanceof ApiError ? err.message : "Não foi possível cadastrar a pessoa.");
    } finally {
      setEnviando(false);
    }
  }

  return (
    <form className="form" onSubmit={handleSubmit}>
      <div className="form-row">
        <label htmlFor="pessoa-nome">Nome</label>
        <input
          id="pessoa-nome"
          type="text"
          value={nome}
          onChange={(e) => setNome(e.target.value)}
          placeholder="Ex.: Maria da Silva"
        />
      </div>

      <div className="form-row form-row--small">
        <label htmlFor="pessoa-idade">Idade</label>
        <input
          id="pessoa-idade"
          type="number"
          min={0}
          max={150}
          value={idade}
          onChange={(e) => setIdade(e.target.value)}
          placeholder="Ex.: 32"
        />
      </div>

      <button type="submit" className="btn btn--primary" disabled={enviando}>
        {enviando ? "Cadastrando…" : "Cadastrar pessoa"}
      </button>

      {erro && <p className="mensagem-erro">{erro}</p>}
    </form>
  );
}
