import { useEffect, useState } from "react";
import { pessoasApi, transacoesApi, relatoriosApi } from "./api/api";
import type { Pessoa, Transacao, TotalGeral } from "./types/types";
import { PessoaForm } from "./components/PessoaForm";
import { PessoaList } from "./components/PessoaList";
import { TransacaoForm } from "./components/TransacaoForm";
import { TransacaoList } from "./components/TransacaoList";
import { Totais } from "./components/Totais";
import "./App.css";

type Secao = "pessoas" | "transacoes" | "totais";

/**
 * Componente raiz: mantém o estado central (pessoas, transações e totais) e
 * organiza a interface em três seções, uma para cada requisito funcional do sistema.
 */
export default function App() {
  const [secaoAtiva, setSecaoAtiva] = useState<Secao>("pessoas");
  const [pessoas, setPessoas] = useState<Pessoa[]>([]);
  const [transacoes, setTransacoes] = useState<Transacao[]>([]);
  const [totais, setTotais] = useState<TotalGeral | null>(null);
  const [carregando, setCarregando] = useState(true);
  const [erroCarregamento, setErroCarregamento] = useState<string | null>(null);

  async function carregarTudo() {
    try {
      setErroCarregamento(null);
      const [pessoasResp, transacoesResp, totaisResp] = await Promise.all([
        pessoasApi.listar(),
        transacoesApi.listar(),
        relatoriosApi.obterTotais(),
      ]);
      setPessoas(pessoasResp);
      setTransacoes(transacoesResp);
      setTotais(totaisResp);
    } catch {
      setErroCarregamento(
        "Não foi possível conectar à API. Verifique se o back-end está em execução."
      );
    } finally {
      setCarregando(false);
    }
  }

  useEffect(() => {
    carregarTudo();
  }, []);

  // Após qualquer mudança (criação de pessoa/transação, exclusão de pessoa),
  // os totais são recalculados no back-end — então basta buscá-los de novo.
  async function atualizarTotais() {
    const totaisResp = await relatoriosApi.obterTotais();
    setTotais(totaisResp);
  }

  function handlePessoaCriada(pessoa: Pessoa) {
    setPessoas((atual) => [...atual, pessoa].sort((a, b) => a.nome.localeCompare(b.nome)));
    atualizarTotais();
  }

  function handlePessoaDeletada(id: number) {
    setPessoas((atual) => atual.filter((p) => p.id !== id));
    // A exclusão em cascata no back-end também remove as transações dessa pessoa.
    setTransacoes((atual) => atual.filter((t) => t.pessoaId !== id));
    atualizarTotais();
  }

  function handleTransacaoCriada(transacao: Transacao) {
    setTransacoes((atual) => [transacao, ...atual]);
    atualizarTotais();
  }

  return (
    <div className="app">
      <header className="cabecalho">
        <div className="cabecalho-conteudo">
          <span className="cabecalho-eyebrow">Livro-caixa da casa</span>
          <h1>Controle de Gastos Residenciais</h1>
          <p>Cadastre quem mora na casa, lance receitas e despesas e acompanhe o saldo de cada um.</p>
        </div>
      </header>

      <nav className="abas">
        <button
          className={`aba ${secaoAtiva === "pessoas" ? "aba--ativa" : ""}`}
          onClick={() => setSecaoAtiva("pessoas")}
        >
          Pessoas
        </button>
        <button
          className={`aba ${secaoAtiva === "transacoes" ? "aba--ativa" : ""}`}
          onClick={() => setSecaoAtiva("transacoes")}
        >
          Transações
        </button>
        <button
          className={`aba ${secaoAtiva === "totais" ? "aba--ativa" : ""}`}
          onClick={() => setSecaoAtiva("totais")}
        >
          Totais
        </button>
      </nav>

      <main className="conteudo">
        {erroCarregamento && <p className="mensagem-erro mensagem-erro--bloco">{erroCarregamento}</p>}

        {carregando ? (
          <p className="texto-vazio">Carregando dados…</p>
        ) : (
          <>
            {secaoAtiva === "pessoas" && (
              <section className="secao">
                <div className="secao-grid">
                  <div className="cartao">
                    <h2>Nova pessoa</h2>
                    <PessoaForm onCriada={handlePessoaCriada} />
                  </div>
                  <div className="cartao cartao--lista">
                    <h2>Pessoas cadastradas</h2>
                    <PessoaList pessoas={pessoas} onDeletada={handlePessoaDeletada} />
                  </div>
                </div>
              </section>
            )}

            {secaoAtiva === "transacoes" && (
              <section className="secao">
                <div className="secao-grid">
                  <div className="cartao">
                    <h2>Nova transação</h2>
                    <TransacaoForm pessoas={pessoas} onCriada={handleTransacaoCriada} />
                  </div>
                  <div className="cartao cartao--lista">
                    <h2>Transações cadastradas</h2>
                    <TransacaoList transacoes={transacoes} />
                  </div>
                </div>
              </section>
            )}

            {secaoAtiva === "totais" && (
              <section className="secao">
                <div className="cartao cartao--lista cartao--largo">
                  <h2>Totais por pessoa</h2>
                  <Totais totais={totais} />
                </div>
              </section>
            )}
          </>
        )}
      </main>
    </div>
  );
}
