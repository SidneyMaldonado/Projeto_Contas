# commit dfd103a

- **Data:** 2026-08-17
- **Autor:** SmaldonadoMiltec (com Claude Opus 5 como co-autor)
- **Mensagem original:** Adiciona execucao da Contas_Api em Docker
- **Branch:** `Executar-no-Docker`

## Resumo

Primeiro commit de infraestrutura do projeto: empacota a `Contas_Api` em container Linux. **Nenhuma linha de código da aplicação foi alterada** — só arquivos novos de build/deploy e a seção 6 do `tech-spec.md`.

Escopo deliberadamente restrito à API. `Contas_Web` (Blazor Server) continua rodando fora do container e `Contas_App` (MAUI) não compila em Linux, então nenhum dos dois — nem `Contas_Test` — é copiado para a imagem.

### Decisões e restrições que apareceram no caminho

**Contexto de build é a raiz da solução, não `Contas_Api/`.** A API referencia `Contas_Core` → `Contas_Db` → `Contas_Contratos`, e o Docker não consegue copiar de fora do contexto. Daí a forma de invocação ser `docker build -f Contas_Api/Dockerfile … .` (documentada em comentário no próprio Dockerfile, porque é o erro mais fácil de cometer aqui). Os quatro `.csproj` são copiados antes do código-fonte para que o `dotnet restore` fique em uma camada cacheável — mudança de código não invalida o restore.

**Globalização.** O runtime `aspnet` roda em modo invariant por padrão em algumas configurações; como a aplicação formata valores monetários e datas em pt-BR, `DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false` e `LANG=pt_BR.UTF-8` são explícitos na imagem final.

**Autenticação do banco.** `Integrated Security` (autenticação Windows) não funciona a partir de um container Linux — o container não participa do domínio. É obrigatório um login SQL Server (autenticação SQL), o que está anotado tanto no `.env.example` quanto no `docker-compose.yml`. O banco em si **não** foi containerizado: continua sendo um SQL Server externo, com schema aplicado manualmente pelos scripts de `Contas_Db/Script/` (regra 8 da seção 4 do tech-spec). Subir o container não cria nem migra tabelas.

**Porta no host.** A 8080 está reservada pelo Windows/Hyper-V na máquina de desenvolvimento, então o padrão publicado é `${API_PORT:-5210}` → `8080` do container. A porta interna é fixa em 8080 (`ASPNETCORE_HTTP_PORTS`).

**Credenciais.** O Compose monta a connection string interpolando `${DB_SERVER}`, `${DB_NAME}`, `${DB_USER}` e `${DB_PASSWORD}`, que o próprio Compose lê do `.env` do diretório do projeto. Optou-se por **não** usar `env_file`: assim a senha existe apenas dentro da `ConnectionStrings__DefaultConnection` e não fica exposta como variável de ambiente solta no container (visível em `docker inspect`/`docker exec env`). O `.env` entrou no `.gitignore` com exceção explícita para `.env.example` (`!.env.example`), e o `.dockerignore` bloqueia os dois na imagem.

### Estado de verificação

A imagem `contas-api:latest` foi construída com sucesso a partir deste Dockerfile (379 MB, camadas de runtime ~106 MB). O ajuste da porta de 8080 para 5210 veio justamente de tentar subir o serviço via Compose e bater no bind reservado do Hyper-V. Não há container em execução ao final da sessão.

## Arquivos afetados

- `Contas_Api/Dockerfile` (novo — multi-stage `sdk:10.0` → `aspnet:10.0`, usuário não-root `$APP_UID`, `EXPOSE 8080`)
- `docker-compose.yml` (novo — serviço `contas-api`, `restart: unless-stopped`, connection string por interpolação)
- `.dockerignore` (novo — exclui `bin/`, `obj/`, `.git/`, `spec/`, `.env*` e os projetos fora do build da API)
- `.env.example` (novo — modelo versionado: `DB_SERVER`, `DB_NAME`, `DB_USER`, `DB_PASSWORD`, `ASPNETCORE_ENVIRONMENT`, `API_PORT`)
- `.gitignore` (ignora `.env` e `.env.local`, mantém `.env.example`)
- `spec/tech-spec.md` (nova seção 6 "Execução em Docker"; a seção de observações passou a ser a 7)

6 arquivos alterados, 147 inserções, 2 remoções.
