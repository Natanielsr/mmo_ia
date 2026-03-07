#!/bin/bash

# Configurações
BACKEND_PROJECT="backend/GameServer.Web/GameServer.Web.csproj"
TEST_PROJECT="backend/GameServer.Tests/GameServer.Tests.csproj"
FRONTEND_DIR="frontend/mmo-frontend"
BACKEND_PORT=5258
FRONTEND_PORT=5173

# Função para limpar processos ao sair
cleanup() {
    echo -e "\n\033[1;33mFinalizando servidores...\033[0m"
    [ ! -z "$BACKEND_PID" ] && kill $BACKEND_PID 2>/dev/null
    [ ! -z "$FRONTEND_PID" ] && kill $FRONTEND_PID 2>/dev/null
    exit
}

# Captura o sinal de interrupção (Ctrl+C)
trap cleanup SIGINT

echo -e "\033[1;34m1. Rodando testes do backend...\033[0m"
dotnet test $TEST_PROJECT
if [ $? -ne 0 ]; then
    echo -e "\033[1;31mErro nos testes. Abortando.\033[0m"
    exit 1
fi

echo -e "\n\033[1;34m2. Verificando portas...\033[0m"
# Libera a porta do backend se estiver ocupada
if lsof -t -i :$BACKEND_PORT >/dev/null; then
    echo "Porta $BACKEND_PORT ocupada. Liberando..."
    fuser -k $BACKEND_PORT/tcp 2>/dev/null
fi

echo -e "\n\033[1;34m3. Iniciando servidor backend...\033[0m"
dotnet run --project $BACKEND_PROJECT &
BACKEND_PID=$!

echo -e "\n\033[1;34m4. Iniciando servidor frontend...\033[0m"
cd $FRONTEND_DIR
npm run dev &
FRONTEND_PID=$!

echo -e "\n\033[1;32mTudo pronto! Pressione Ctrl+C para parar todos os serviços.\033[0m"

# Mantém o script rodando para capturar o log e o Ctrl+C
wait
