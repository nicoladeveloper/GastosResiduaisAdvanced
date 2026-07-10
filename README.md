# Controle de Gastos Residenciais

![CI](https://github.com/nicoladeveloper/GastosResiduaisAdvanced/actions/workflows/ci.yml/badge.svg)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)
![React](https://img.shields.io/badge/React-18-61DAFB?logo=react&logoColor=black)
![TypeScript](https://img.shields.io/badge/TypeScript-5-3178C6?logo=typescript&logoColor=white)
![Vite](https://img.shields.io/badge/Vite-5-646CFF?logo=vite&logoColor=white)
![SQLite](https://img.shields.io/badge/SQLite-banco%20local-003B57?logo=sqlite&logoColor=white)
![xUnit](https://img.shields.io/badge/xUnit-testes%20automatizados-blueviolet)

Sistema simples de controle de gastos residenciais. Permite cadastrar as
pessoas da casa, registrar as transações de cada uma (receitas e despesas)
e consultar os totais, tudo em uma interface única e direta.

## Sobre o projeto

A ideia é ter um livro-caixa digital para uso doméstico: cada pessoa da
residência tem seu próprio histórico de movimentações, e ao final é
possível ver quanto cada uma recebeu, gastou e qual o saldo — além do
total geral da casa. Os dados ficam salvos em um banco local, então nada
se perde ao fechar a aplicação.

Regras importantes que o sistema aplica automaticamente: ao excluir uma
pessoa, todas as suas transações são removidas junto; e pessoas menores
de idade só podem ter despesas cadastradas em seu nome, nunca receitas.

## Tecnologias

O back-end é construído em .NET 8 com ASP.NET Core Web API, Entity
Framework Core para acesso a dados e um banco SQLite em arquivo. O
front-end é feito em React com TypeScript, usando Vite como ferramenta
de build. Os testes automatizados do back-end usam xUnit com o provider
InMemory do Entity Framework, e o projeto conta com um pipeline de
integração contínua no GitHub Actions, que valida o build e os testes do
back-end e o build do front-end a cada alteração enviada ao repositório.

## Capturas de tela

*(espaço reservado — adicione aqui os prints da interface)*

**Cadastro de pessoas**

![Cadastro de pessoas](https://github.com/user-attachments/assets/6700dd39-b1fa-49cf-8257-6597143c5a8d)

**Cadastro de transações**

![Cadastro de transações](docs/screenshots/cadastro-transacoes.png)

**Consulta de totais**

![Consulta de totais](docs/screenshots/totais.png)

## Como executar

É necessário ter o SDK do .NET 8 e o Node.js instalados na máquina.

Existe um script que sobe o back-end e o front-end juntos com um único
comando, disponível na raiz do projeto (`start.sh`, para Mac/Linux/WSL).
Em ambientes Windows sem WSL, o mais simples é abrir dois terminais: um
para o back-end, entrando na pasta do projeto da API e executando o
restore e o run do .NET; outro para o front-end, entrando na pasta
`frontend` e executando a instalação de dependências seguida do comando
de desenvolvimento do Vite.

Por padrão, o back-end sobe em `localhost:5000` (com o Swagger disponível
em `/swagger`) e o front-end em `localhost:5173`, já apontando para a API
local. Na primeira execução, o banco SQLite e suas tabelas são criados
automaticamente, sem necessidade de configuração adicional.

## Testes automatizados

O projeto de testes cobre as regras de negócio da aplicação: geração
automática de identificadores, exclusão em cascata de transações ao
remover uma pessoa, rejeição de receitas para menores de idade, aceitação
de despesas para menores de idade, rejeição de transações vinculadas a
uma pessoa inexistente, e o cálculo correto dos totais — incluindo casos
de saldo negativo e pessoas sem nenhuma transação registrada. Os testes
podem ser executados a partir da pasta do back-end.

## Integração contínua

A cada push ou pull request enviado para a branch principal, o GitHub
Actions executa automaticamente dois processos em paralelo: um valida o
back-end, restaurando as dependências, compilando e rodando a suíte de
testes; o outro valida o front-end, instalando as dependências e gerando
o build de produção, o que também cobre a checagem de tipos do
TypeScript. O selo no topo deste documento reflete o status da execução
mais recente.
