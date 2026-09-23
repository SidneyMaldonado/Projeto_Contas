# Dockerfile da Contas_Api
# IMPORTANTE: o contexto de build deve ser a RAIZ da solucao, pois a Api
# referencia Contas_Core, Contas_Db e Contas_Contratos.
#
#   docker build -f Contas_Api/Dockerfile -t contas-api:latest .

# ---------- Estagio de build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia apenas os csproj primeiro para aproveitar o cache de camadas do restore.
# Contas_App (MAUI) e Contas_Test ficam de fora: nao fazem parte da Api e o MAUI
# nao compila em Linux.
COPY Contas_Api/Contas_Api.csproj             Contas_Api/
COPY Contas_Core/Contas_Core.csproj           Contas_Core/
COPY Contas_Db/Contas_Db.csproj               Contas_Db/
COPY Contas_Contratos/Contas_Contratos.csproj Contas_Contratos/

RUN dotnet restore Contas_Api/Contas_Api.csproj

# Copia o codigo-fonte dos projetos envolvidos
COPY Contas_Api/       Contas_Api/
COPY Contas_Core/      Contas_Core/
COPY Contas_Db/        Contas_Db/
COPY Contas_Contratos/ Contas_Contratos/

RUN dotnet publish Contas_Api/Contas_Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# ---------- Estagio de runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Cultura pt-BR e ICU completo (a aplicacao lida com valores monetarios e datas)
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    LANG=pt_BR.UTF-8 \
    ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_HTTP_PORTS=8080

COPY --from=build /app/publish .

# A imagem aspnet:8+ ja traz o usuario nao-root "app"
USER $APP_UID

EXPOSE 8080

ENTRYPOINT ["dotnet", "Contas_Api.dll"]
