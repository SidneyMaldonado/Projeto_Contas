# commit 6af6f90

- **Data:** 2026-09-23
- **Autor:** SmaldonadoMiltec (com Claude Opus 5.5 como co-autor)
- **Mensagem original:** Separa configuracao de desenvolvimento e publicacao da Api em /fin_back
- **Branch:** `Executar-no-Docker`

## Resumo

Commit de configuração, sem regra de negócio nova. A `Contas_Api` passa a ter dois destinos com configuração própria:

| | Desenvolvimento | Publicação |
|---|---|---|
| Onde roda | `http://localhost:5210` (`dotnet run` / VS) | `http://lab.miltecti.com.br/fin_back/` (IIS em `MS208`) |
| Ambiente | `Development` (via `launchSettings.json`) | `Production` (gravado no `web.config` pelo perfil de publicação) |
| Arquivo lido | `appsettings.json` + `appsettings.Development.json` | só `appsettings.json` |
| Banco | `MS211,1434` / `test_fin`, `Integrated Security` | `MS211,1434` / `test_fin`, login SQL `user_db_dev` |
| `PathBase` | vazio | `/fin_back` |
| Swagger | sempre | ligado por `Swagger:Habilitado: true` |

Os clientes (`Contas_Web` e `Contas_App`) foram alinhados ao endereço publicado, e o `ContasDbContext` ganhou a mesma separação para quando é criado sem DI.

## 1. Publicação por FTP

O perfil `Contas_Api/Properties/PublishProfiles/FTPLabFin_Back.pubxml` publica por FTP (modo passivo) em `MS208`, pasta `/laboratorio/fin_back`, e abre `lab.miltecti.com.br/fin_back` ao terminar. **O `.pubxml` e o `.pubxml.user` (senha criptografada) estão no `.gitignore`** — só existem na máquina de quem publica.

Nesta sessão o perfil recebeu `<EnvironmentName>Production</EnvironmentName>`. Com isso o `dotnet publish` escreve no `web.config`:

```xml
<environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
```

Production já seria o padrão do ASP.NET Core sem essa linha. Ela está ali para deixar explícito e para que trocar o ambiente de publicação (ex.: `Staging`) seja só editar o perfil. Como o arquivo não é versionado, **quem recriar o perfil precisa repetir essa linha** — ou aceitar o padrão.

## 2. `appsettings` da Api

- **`appsettings.json` é a versão de publicação**, não uma base neutra. É o único lido em Production. Tem a conexão com login SQL porque o IIS roda a Api com a identidade do pool de aplicativos, que não é um login do SQL Server — `Integrated Security` falharia lá.
- **`appsettings.Development.json` sobrescreve** só o que muda local: conexão com `Integrated Security` (conta Windows de quem desenvolve) e `PathBase` vazio.
- `DefaultConnection2` foi removida — nada a lia.
- `Jwt` fica só no `appsettings.json`; vale para os dois ambientes.

## 3. `PathBase` e Swagger (`Program.cs`)

`app.UsePathBase("/fin_back")` roda quando `PathBase` vem preenchido. Ele só remove o prefixo **se a requisição chegar com ele**; sem prefixo a requisição passa intacta. Por isso o mesmo código serve para os três cenários:

- **IIS (publicação):** o ASP.NET Core Module já remove o nome da aplicação virtual, então a requisição chega como `/api/...` e o `UsePathBase` não faz nada. Ele não é necessário aqui, mas também não atrapalha.
- **Docker atrás de proxy:** o proxy repassa `/fin_back/api/...` e é o `UsePathBase` que tira o prefixo. O `docker-compose.yml` passa `PathBase` por variável (`API_PATH_BASE`, padrão `/fin_back`).
- **Local:** `PathBase` vazio, sem efeito.

O Swagger deixou de ser exclusivo de Development: liga também com `Swagger:Habilitado: true`, que está ligado no `appsettings.json` por ser um ambiente de laboratório. O endpoint da UI virou relativo (`../openapi/v1.json`), porque o absoluto `/openapi/v1.json` ignorava o `/fin_back` e a UI publicada não achava o documento. **Em um ambiente de produção de verdade, desligar `Swagger:Habilitado`.**

## 4. `ContasDbContext` sem DI

A Api sempre configura o contexto por `AddDbContext` com `ConnectionStrings:DefaultConnection`, então o `OnConfiguring` nunca é usado por ela. Ele só entra em ação quando o contexto é criado com `new ContasDbContext()` — hoje o `Conexao_Tests/ConexaoTest` e as ferramentas do EF (`dotnet-ef`, registrado em `Contas_Api/dotnet-tools.json`).

A string fixa virou duas constantes (`ConexaoDesenvolvimento`, `ConexaoPublicacao`) e o método público `ObterStringConexao()`, que decide nesta ordem:

1. **`ConnectionStrings__DefaultConnection`** — se a variável existir, vence. É o mesmo nome que o ASP.NET Core e o `docker-compose.yml` usam.
2. **`ASPNETCORE_ENVIRONMENT` ou `DOTNET_ENVIRONMENT`** — `Development` usa a de desenvolvimento; qualquer outro valor usa a de publicação.
3. **Sem ambiente definido** — `#if DEBUG` usa a de desenvolvimento, Release usa a de publicação.

O passo 3 existe porque o `dotnet test` não define ambiente. Seguir a convenção do ASP.NET Core (sem ambiente = Production) faria o teste de conexão, rodado em Debug numa máquina de desenvolvimento, usar o login de publicação.

**As strings estão duplicadas** entre o `appsettings` da Api e as constantes do `ContasDbContext`: o `Contas_Db` não lê `appsettings`, e fazer isso exigiria adicionar `Microsoft.Extensions.Configuration.Json` à biblioteca. Mudou o banco, **mude nos dois lugares**.

## 5. Clientes

- **`Contas_Web`:** mesma separação da Api. `appsettings.json` (publicação) aponta `Api:BaseUrl` para `http://lab.miltecti.com.br/fin_back/` — antes estava `/engine`, que não é onde a Api é publicada. `appsettings.Development.json` aponta para `http://localhost:5210/`. **A barra final é obrigatória:** sem ela, o `HttpClient` trata `fin_back` como nome de arquivo e o descarta ao combinar a `BaseAddress` com as rotas relativas (`api/contas` viraria `http://lab.miltecti.com.br/api/contas`).
- **`Contas_App`:** no Android, `ApiConfig.BaseUrl` aponta para o servidor tanto em Debug quanto em Release. Isso foi uma escolha de quem desenvolve: permite depurar em celular físico contra o servidor. Para usar o emulador contra a Api local, troque pelo endereço comentado ao lado (`http://10.0.2.2:5210/`).
- **`network_security_config.xml`** (novo, referenciado no `AndroidManifest.xml`): o Android 9+ bloqueia HTTP sem TLS por padrão, e o servidor ainda não tem HTTPS. A liberação vale só para `lab.miltecti.com.br` e `10.0.2.2`, sem subdomínios. Qualquer host HTTP novo precisa entrar nessa lista.

## Verificação

- `dotnet build Contas_Api` e `dotnet build Contas_Web`: **0 erros, 0 avisos.**
- `dotnet publish Contas_Api -c Release -p:EnvironmentName=Production` numa pasta temporária: o `web.config` gerado tem `ASPNETCORE_ENVIRONMENT=Production` e os dois `appsettings` vão para a publicação.
- `dotnet test --filter ConexaoTest`: **passou** (Debug, sem ambiente definido → conexão de desenvolvimento).
- **Não houve publicação real por FTP**, nem teste da Api rodando em `/fin_back`.
- **Não houve build do `Contas_App`** nem teste em dispositivo.

## Pendências

1. **Credenciais versionadas:** a senha de `user_db_dev` e a `Jwt:Key` estão no `appsettings.json` commitado, e a senha também está no `ContasDbContext.cs`. O caminho é tirá-las do repositório e passá-las por variável de ambiente no IIS (`ConnectionStrings__DefaultConnection`, `Jwt__Key`) — o passo 1 do `ObterStringConexao()` já aceita isso.
2. **Banco de publicação:** foi mantido o mesmo `test_fin` do desenvolvimento, mudando só o login. Se a publicação ganhar banco próprio, atualizar `appsettings.json` **e** `ConexaoPublicacao`.
3. **Confirmar a Api publicada:** abrir `http://lab.miltecti.com.br/fin_back/swagger` depois da primeira publicação e testar um login pelo Web e pelo App.
