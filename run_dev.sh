#!/bin/bash

BACKEND_PROJECT="backend/GameServer.Web/GameServer.Web.csproj"
TEST_PROJECT="backend/GameServer.Tests/GameServer.Tests.csproj"
FRONTEND_DIR="frontend/mmo-frontend"
BACKEND_PORT=5258

_tmp=$(mktemp)

_cleanup() {
    echo -e "\n\033[33mFinalizando...\033[0m"
    [ -n "$BACKEND_PID" ] && kill "$BACKEND_PID" 2>/dev/null
    [ -n "$FRONTEND_PID" ] && kill "$FRONTEND_PID" 2>/dev/null
    rm -f "$_tmp"
    exit
}
trap _cleanup SIGINT

_ok()   { echo -e "\033[32m✓\033[0m"; }
_fail() { echo -e "\033[31m✗\033[0m\n"; cat "$_tmp"; rm -f "$_tmp"; exit 1; }

# ── Testes ────────────────────────────────────────────────────
echo -e "\033[1mTestes\033[0m"

printf "  Backend  (xUnit)   ... "
dotnet test "$TEST_PROJECT" --verbosity quiet --nologo > "$_tmp" 2>&1 && _ok || _fail

printf "  Frontend (Vitest)  ... "
(cd "$FRONTEND_DIR" && npm test) > "$_tmp" 2>&1 && _ok || _fail

# ── Portas ────────────────────────────────────────────────────
if lsof -t -i :"$BACKEND_PORT" > /dev/null 2>&1; then
    printf "\n  Liberando porta %s ... " "$BACKEND_PORT"
    fuser -k "${BACKEND_PORT}/tcp" 2>/dev/null && _ok
fi

# ── Servidores ────────────────────────────────────────────────
echo -e "\n\033[1mServidores\033[0m"

dotnet run --project "$BACKEND_PROJECT" > /dev/null 2>&1 &
BACKEND_PID=$!
echo "  Backend  → http://localhost:$BACKEND_PORT"

(cd "$FRONTEND_DIR" && exec npm run dev > /dev/null 2>&1) &
FRONTEND_PID=$!
echo "  Frontend → http://localhost:5173"

echo -e "\n\033[32mPronto!\033[0m Ctrl+C para encerrar.\n"
rm -f "$_tmp"
wait
