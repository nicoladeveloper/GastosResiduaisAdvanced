# Controle de Gastos Residenciais

Sistema simples de controle de gastos residenciais, com cadastro de pessoas,
cadastro de transações (receitas/despesas) e consulta de totais.

- **Back-end:** .NET 8 (ASP.NET Core Web API) + Entity Framework Core + SQLite
- **Front-end:** React + TypeScript (Vite)
- **Persistência:** banco SQLite em arquivo (`gastos.db`), então os dados
  continuam existindo após fechar a aplicação.

```
gastos-residenciais/
├── start.sh                        # Sobe back-end + front-end com um único comando
├── backend/
│   ├── GastosResidenciais.sln
│   ├── GastosResidenciais.Api/      # API .NET
│   │   ├── Models/                  # Entidades (Pessoa, Transacao, TipoTransacao)
│   │   ├── DTOs/                    # Objetos de entrada/saída da API
│   │   ├── Data/                    # AppDbContext (EF Core)
│   │   ├── Controllers/             # PessoasController, TransacoesController, RelatoriosController
│   │   ├── Middleware/              # Tratamento global de exceções
│   │   └── Program.cs               # Configuração (CORS, Swagger, banco de dados)
│   └── GastosResidenciais.Api.Tests/  # Testes automatizados (xUnit + EF Core InMemory)
└── frontend/
    └── src/
        ├── api/                    # Camada de comunicação com a API
        ├── types/                  # Tipos TypeScript (espelham os DTOs)
        ├── components/             # PessoaForm, PessoaList, TransacaoForm, TransacaoList, Totais
        └── App.tsx                 # Navegação entre as três seções (Pessoas / Transações / Totais)
```

## Regras de negócio implementadas

1. **Pessoas**: criação, listagem e exclusão. Identificador único gerado
   automaticamente pelo banco (auto-increment).
2. **Exclusão em cascata**: ao excluir uma pessoa, todas as suas transações
   são apagadas junto (configurado em `AppDbContext` com `DeleteBehavior.Cascade`).
3. **Transações**: criação e listagem (sem edição/exclusão, conforme especificação).
4. **Menor de idade**: se a pessoa informada tiver menos de 18 anos, somente
   transações do tipo **Despesa** podem ser cadastradas para ela. Essa regra é
   validada no back-end (`TransacoesController`) e também refletida no
   formulário do front-end, para orientar o usuário antes mesmo do envio.
5. **Pessoa inexistente**: o `PessoaId` informado em uma transação precisa
   existir previamente no cadastro; caso contrário a API retorna erro 400.
6. **Totais**: para cada pessoa são calculados o total de receitas, o total
   de despesas e o saldo (receitas − despesas); ao final é exibido o total
   geral somando todas as pessoas.

## Como executar

### Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org/)

### Opção rápida (um único comando)

```bash
./start.sh
```

Sobe o back-end em `http://localhost:5000` e o front-end em `http://localhost:5173`
simultaneamente. `Ctrl+C` encerra os dois processos.

### Back-end (manual)

```bash
cd backend/GastosResidenciais.Api
dotnet restore
dotnet run
```

A API sobe em `http://localhost:5000` (Swagger disponível em `/swagger`).
Na primeira execução, o arquivo `gastos.db` (SQLite) e as tabelas são criados
automaticamente — não é necessário rodar migrations manualmente.

### Front-end

Em outro terminal:

```bash
cd frontend
npm install
npm run dev
```

A aplicação abre em `http://localhost:5173` e já aponta para a API em
`http://localhost:5000/api` (pode ser customizado copiando `.env.example`
para `.env` e ajustando `VITE_API_URL`).

## Testes automatizados

O projeto `GastosResidenciais.Api.Tests` cobre as regras de negócio da especificação
usando xUnit + o provider "InMemory" do EF Core (não precisa de banco real):

```bash
cd backend
dotnet test
```

Cenários cobertos: geração automática de Id, exclusão em cascata de transações ao
excluir uma pessoa, transação rejeitada para menor de idade com receita, transação
aceita para menor de idade com despesa, rejeição de `PessoaId` inexistente, e o
cálculo de totais (por pessoa e geral, incluindo saldo negativo e pessoa sem
transações).

## Nota sobre complexidade algorítmica

Nem toda operação pode (ou deve) ser O(1):

- **Criar/buscar/excluir uma pessoa ou transação por Id**: O(1) amortizado, via
  índice da chave primária.
- **Listar pessoas/transações**: O(n) — é inerente a devolver n itens.
- **Calcular totais** (`RelatoriosController`): O(n) na quantidade de transações,
  pois cada uma precisa ser somada pelo menos uma vez. Seria possível reduzir a
  leitura para O(1) mantendo um saldo já calculado por pessoa e atualizando-o a
  cada nova transação (denormalização), mas isso troca simplicidade e
  consistência garantida por performance — não compensa na escala de um
  controle de gastos residencial. Optou-se deliberadamente por O(n) simples e
  correto.

## Endpoints da API

| Método | Rota                     | Descrição                              |
|--------|--------------------------|-----------------------------------------|
| GET    | `/api/pessoas`           | Lista pessoas cadastradas               |
| POST   | `/api/pessoas`           | Cadastra uma pessoa                     |
| DELETE | `/api/pessoas/{id}`      | Remove uma pessoa (e suas transações)   |
| GET    | `/api/transacoes`        | Lista transações (filtro opcional `?pessoaId=`) |
| POST   | `/api/transacoes`        | Cadastra uma transação                  |
| GET    | `/api/relatorios/totais` | Totais por pessoa + total geral         |
