#!/usr/bin/env bash
# Sobe o back-end (.NET) e o front-end (React/Vite) juntos, com um único comando.
# Uso: ./start.sh
set -e

DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo "→ Restaurando dependências do back-end..."
(cd "$DIR/backend/GastosResidenciais.Api" && dotnet restore)

echo "→ Instalando dependências do front-end (se necessário)..."
if [ ! -d "$DIR/frontend/node_modules" ]; then
  (cd "$DIR/frontend" && npm install)
fi

echo "→ Iniciando back-end em http://localhost:5000 e front-end em http://localhost:5173"
(cd "$DIR/backend/GastosResidenciais.Api" && dotnet run) &
BACKEND_PID=$!

(cd "$DIR/frontend" && npm run dev) &
FRONTEND_PID=$!

trap "kill $BACKEND_PID $FRONTEND_PID 2>/dev/null" EXIT
wait
