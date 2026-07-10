import type { TotalGeral } from "../types/types";
import { formatarMoeda } from "../utils/format";

interface TotaisProps {
  totais: TotalGeral | null;
}

/**
 * Exibe a consulta de totais: uma linha por pessoa (receitas, despesas e saldo)
 * e, ao final, uma linha de total geral somando todas as pessoas.
 */
export function Totais({ totais }: TotaisProps) {
  if (!totais) {
    return <p className="texto-vazio">Carregando totais…</p>;
  }

  if (totais.pessoas.length === 0) {
    return <p className="texto-vazio">Cadastre pessoas e transações para ver os totais.</p>;
  }

  return (
    <table className="tabela tabela--totais">
      <thead>
        <tr>
          <th>Pessoa</th>
          <th className="col-valor">Receitas</th>
          <th className="col-valor">Despesas</th>
          <th className="col-valor">Saldo</th>
        </tr>
      </thead>
      <tbody>
        {totais.pessoas.map((pessoa) => (
          <tr key={pessoa.pessoaId}>
            <td>{pessoa.nome}</td>
            <td className="col-valor valor valor--positivo">
              {formatarMoeda(pessoa.totalReceitas)}
            </td>
            <td className="col-valor valor valor--negativo">
              {formatarMoeda(pessoa.totalDespesas)}
            </td>
            <td
              className={`col-valor valor ${
                pessoa.saldo >= 0 ? "valor--positivo" : "valor--negativo"
              }`}
            >
              {formatarMoeda(pessoa.saldo)}
            </td>
          </tr>
        ))}
      </tbody>
      <tfoot>
        <tr className="linha-total-geral">
          <td>Total geral</td>
          <td className="col-valor valor valor--positivo">
            {formatarMoeda(totais.totalReceitas)}
          </td>
          <td className="col-valor valor valor--negativo">
            {formatarMoeda(totais.totalDespesas)}
          </td>
          <td
            className={`col-valor valor ${
              totais.saldoLiquido >= 0 ? "valor--positivo" : "valor--negativo"
            }`}
          >
            {formatarMoeda(totais.saldoLiquido)}
          </td>
        </tr>
      </tfoot>
    </table>
  );
}
