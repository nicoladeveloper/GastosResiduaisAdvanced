import type { Transacao } from "../types/types";
import { formatarMoeda } from "../utils/format";

interface TransacaoListProps {
  transacoes: Transacao[];
}

/** Lista somente-leitura de todas as transações cadastradas (mais recentes primeiro). */
export function TransacaoList({ transacoes }: TransacaoListProps) {
  if (transacoes.length === 0) {
    return <p className="texto-vazio">Nenhuma transação cadastrada ainda.</p>;
  }

  return (
    <table className="tabela">
      <thead>
        <tr>
          <th>Id</th>
          <th>Descrição</th>
          <th>Pessoa</th>
          <th>Tipo</th>
          <th className="col-valor">Valor</th>
        </tr>
      </thead>
      <tbody>
        {transacoes.map((transacao) => (
          <tr key={transacao.id}>
            <td className="col-id">{transacao.id}</td>
            <td>{transacao.descricao}</td>
            <td>{transacao.pessoaNome}</td>
            <td>
              <span
                className={`etiqueta ${
                  transacao.tipo === "Receita" ? "etiqueta--receita" : "etiqueta--despesa"
                }`}
              >
                {transacao.tipo}
              </span>
            </td>
            <td
              className={`col-valor valor ${
                transacao.tipo === "Receita" ? "valor--positivo" : "valor--negativo"
              }`}
            >
              {transacao.tipo === "Receita" ? "+" : "-"} {formatarMoeda(transacao.valor)}
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}
