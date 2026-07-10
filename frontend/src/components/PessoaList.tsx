import { useState } from "react";
import { pessoasApi, ApiError } from "../api/api";
import type { Pessoa } from "../types/types";

interface PessoaListProps {
  pessoas: Pessoa[];
  /** Chamado após a exclusão ser confirmada pelo back-end, para o pai atualizar o estado. */
  onDeletada: (id: number) => void;
}

/**
 * Lista as pessoas cadastradas. Ao excluir uma pessoa, o back-end também remove
 * (em cascata) todas as transações associadas a ela — por isso o aviso no diálogo de confirmação.
 */
export function PessoaList({ pessoas, onDeletada }: PessoaListProps) {
  const [excluindoId, setExcluindoId] = useState<number | null>(null);
  const [erro, setErro] = useState<string | null>(null);

  async function handleDeletar(pessoa: Pessoa) {
    const confirmado = window.confirm(
      `Excluir "${pessoa.nome}"? Todas as transações dessa pessoa também serão apagadas.`
    );
    if (!confirmado) return;

    setErro(null);
    setExcluindoId(pessoa.id);
    try {
      await pessoasApi.deletar(pessoa.id);
      onDeletada(pessoa.id);
    } catch (err) {
      setErro(err instanceof ApiError ? err.message : "Não foi possível excluir a pessoa.");
    } finally {
      setExcluindoId(null);
    }
  }

  if (pessoas.length === 0) {
    return <p className="texto-vazio">Nenhuma pessoa cadastrada ainda.</p>;
  }

  return (
    <>
      {erro && <p className="mensagem-erro">{erro}</p>}
      <table className="tabela">
        <thead>
          <tr>
            <th>Id</th>
            <th>Nome</th>
            <th>Idade</th>
            <th></th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          {pessoas.map((pessoa) => (
            <tr key={pessoa.id}>
              <td className="col-id">{pessoa.id}</td>
              <td>{pessoa.nome}</td>
              <td>{pessoa.idade} anos</td>
              <td>
                {pessoa.menorDeIdade && (
                  <span className="etiqueta etiqueta--aviso">menor de idade</span>
                )}
              </td>
              <td className="col-acao">
                <button
                  type="button"
                  className="btn btn--perigo btn--pequeno"
                  onClick={() => handleDeletar(pessoa)}
                  disabled={excluindoId === pessoa.id}
                >
                  {excluindoId === pessoa.id ? "Excluindo…" : "Excluir"}
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </>
  );
}
