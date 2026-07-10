import { useMemo, useState } from "react";
import type { FormEvent } from "react";
import { transacoesApi, ApiError } from "../api/api";
import type { Pessoa, Transacao, TipoTransacao } from "../types/types";

interface TransacaoFormProps {
  pessoas: Pessoa[];
  onCriada: (transacao: Transacao) => void;
}

/**
 * Formulário de cadastro de transações.
 * Reflete no próprio formulário a regra de negócio "menor de idade só pode ter despesa":
 * ao selecionar uma pessoa menor de idade, a opção "Receita" é desabilitada e o tipo
 * é travado automaticamente em "Despesa". A validação definitiva, porém, sempre
 * acontece no back-end (o front-end apenas evita o erro antes de enviar).
 */
export function TransacaoForm({ pessoas, onCriada }: TransacaoFormProps) {
  const [descricao, setDescricao] = useState("");
  const [valor, setValor] = useState("");
  const [tipo, setTipo] = useState<TipoTransacao>("Despesa");
  const [pessoaId, setPessoaId] = useState<string>("");
  const [erro, setErro] = useState<string | null>(null);
  const [enviando, setEnviando] = useState(false);

  const pessoaSelecionada = useMemo(
    () => pessoas.find((p) => p.id === Number(pessoaId)),
    [pessoas, pessoaId]
  );

  const somenteDespesa = pessoaSelecionada?.menorDeIdade ?? false;

  function handleSelecionarPessoa(id: string) {
    setPessoaId(id);
    const pessoa = pessoas.find((p) => p.id === Number(id));
    if (pessoa?.menorDeIdade) {
      setTipo("Despesa");
    }
  }

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setErro(null);

    if (!descricao.trim()) {
      setErro("Informe a descrição da transação.");
      return;
    }
    const valorNumero = Number(valor);
    if (valor === "" || Number.isNaN(valorNumero) || valorNumero <= 0) {
      setErro("Informe um valor maior que zero.");
      return;
    }
    if (!pessoaId) {
      setErro("Selecione a pessoa responsável pela transação.");
      return;
    }

    setEnviando(true);
    try {
      const transacaoCriada = await transacoesApi.criar({
        descricao: descricao.trim(),
        valor: valorNumero,
        tipo,
        pessoaId: Number(pessoaId),
      });
      onCriada(transacaoCriada);
      setDescricao("");
      setValor("");
    } catch (err) {
      setErro(err instanceof ApiError ? err.message : "Não foi possível cadastrar a transação.");
    } finally {
      setEnviando(false);
    }
  }

  if (pessoas.length === 0) {
    return (
      <p className="texto-vazio">
        Cadastre ao menos uma pessoa antes de lançar transações.
      </p>
    );
  }

  return (
    <form className="form" onSubmit={handleSubmit}>
      <div className="form-row">
        <label htmlFor="transacao-pessoa">Pessoa</label>
        <select
          id="transacao-pessoa"
          value={pessoaId}
          onChange={(e) => handleSelecionarPessoa(e.target.value)}
        >
          <option value="">Selecione…</option>
          {pessoas.map((pessoa) => (
            <option key={pessoa.id} value={pessoa.id}>
              {pessoa.nome} {pessoa.menorDeIdade ? "(menor de idade)" : ""}
            </option>
          ))}
        </select>
      </div>

      <div className="form-row">
        <label htmlFor="transacao-descricao">Descrição</label>
        <input
          id="transacao-descricao"
          type="text"
          value={descricao}
          onChange={(e) => setDescricao(e.target.value)}
          placeholder="Ex.: Conta de luz"
        />
      </div>

      <div className="form-row form-row--small">
        <label htmlFor="transacao-valor">Valor (R$)</label>
        <input
          id="transacao-valor"
          type="number"
          min={0.01}
          step="0.01"
          value={valor}
          onChange={(e) => setValor(e.target.value)}
          placeholder="Ex.: 150.00"
        />
      </div>

      <div className="form-row form-row--small">
        <label htmlFor="transacao-tipo">Tipo</label>
        <select
          id="transacao-tipo"
          value={tipo}
          onChange={(e) => setTipo(e.target.value as TipoTransacao)}
          disabled={somenteDespesa}
        >
          <option value="Despesa">Despesa</option>
          <option value="Receita" disabled={somenteDespesa}>
            Receita
          </option>
        </select>
        {somenteDespesa && (
          <span className="dica">
            {pessoaSelecionada?.nome} é menor de idade: apenas despesas são permitidas.
          </span>
        )}
      </div>

      <button type="submit" className="btn btn--primary" disabled={enviando}>
        {enviando ? "Cadastrando…" : "Cadastrar transação"}
      </button>

      {erro && <p className="mensagem-erro">{erro}</p>}
    </form>
  );
}
