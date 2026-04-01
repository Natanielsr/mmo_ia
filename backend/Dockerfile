FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar os arquivos de projeto
COPY ["GameServer.Web/GameServer.Web.csproj", "GameServer.Web/"]
COPY ["GameServer.Infrastructure/GameServer.Infrastructure.csproj", "GameServer.Infrastructure/"]
COPY ["GameServerApp/GameServerApp.csproj", "GameServerApp/"]

# Restaurar dependências
RUN dotnet restore "GameServer.Web/GameServer.Web.csproj"

# Copiar todo o código restante
COPY . .

# Buildar e publicar
WORKDIR "/src/GameServer.Web"
RUN dotnet publish "GameServer.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Imagem final para runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443
ENV ASPNETCORE_URLS=http://+:80

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "GameServer.Web.dll"]
