# Tech Spec — Projeto_Contas

> Documento de referência da arquitetura, convenções e regras de desenvolvimento do projeto. Escrito a partir do estado atual do código (agosto/2026). Atualize este arquivo quando decisões estruturais mudarem.

## 1. Visão geral

Projeto_Contas é um sistema de controle financeiro pessoal: contas bancárias, dívidas/parcelas, investimentos (carteiras, histórico, operações), categorias e credores, por usuário autenticado.

A solução é dividida em 7 projetos .NET (todos `net10.0`, exceto o app mobile que também tem targets Android/iOS/MacCatalyst/Windows):

| Projeto | Tipo | Papel |
|---|---|---|
| `Contas_Db` | Class library | Acesso a dados: EF Core, Models, Repositories |
| `Contas_Contratos` | Class library | DTOs compartilhados (contrato entre API e clientes) |
| `Contas_Core` | Class library | Regras de negócio: UseCases, validação (Biz), Converters, Security |
| `Contas_Api` | ASP.NET Core Web API | Endpoints REST, autenticação JWT |
| `Contas_Web` | Blazor Server | Cliente web |
| `Contas_App` | .NET MAUI | Cliente mobile/desktop |
| `Contas_Test` | MSTest | Testes de repositório, use case e integração de API |

## 2. Dependências entre projetos

```
Contas_Db  <───────────────┐
    ▲                      │
    │                      │
Contas_Contratos ◄── Contas_Core
    ▲                  ▲
    │                  │
Contas_Web          Contas_Api
Contas_App        (Db + Core)

Contas_Test ──► Contas_Api, Contas_Core, Contas_Db
```

- `Contas_Db` não depende de nenhum outro projeto do repositório.
- `Contas_Contratos` não depende de nenhum outro projeto (nem de `Contas_Db`) — é por isso que ela existe separada de `Contas_Core`: os clientes (`Contas_Web`, `Contas_App`) referenciam **só** `Contas_Contratos`, e não arrastam EF Core/SqlServer/JWT para o build deles (ver commit `006eaeb`).
- `Contas_Core` depende de `Contas_Db` (acessa Models/Repositories) e de `Contas_Contratos` (os UseCases recebem/retornam DTOs em alguns casos, ex. `ObterResumoContaUseCase`).
- `Contas_Api` depende de `Contas_Core` e `Contas_Db` (para DI do `DbContext`/repositórios concretos).
- `Contas_Web` e `Contas_App` dependem **só** de `Contas_Contratos`.
- `Contas_Test` depende de `Contas_Api`, `Contas_Core` e `Contas_Db` (testes de integração usam `WebApplicationFactory<Program>` da API).

**Regra a preservar:** nunca adicionar uma `ProjectReference` de `Contas_Web`/`Contas_App` para `Contas_Core` ou `Contas_Db`. Se um cliente precisar de um novo tipo (DTO), o tipo deve nascer em `Contas_Contratos`.

## 3. Onde fica cada coisa

### 3.1 `Contas_Db` — dados

- `Model/` — entidades EF Core (`Conta`, `Divida`, `Parcela`, `Investimento`, `Carteira`, `Historico`, `Operacao`, `Categoria`, `Credor`, `Usuario`). Todas implementam `Model/Interface/ISoftDelete.cs` (`bool Ativo { get; set; }`) — exclusão lógica em vez de física é o padrão em toda a base (ver `Repository.SoftDeleteAsync`).
- `Model/ContasDbContext.cs` — um `DbSet<T>` por entidade. Connection string vem de `appsettings`/config da API; há um fallback hardcoded no `OnConfiguring` (só usado se ninguém configurar via DI — na prática a API sempre configura via `AddDbContext`).
- `Repository/Interface/` — `IRepository<T>` genérico (`GetByIdAsync`, `GetAllAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync`, `SoftDeleteAsync`) e interfaces específicas quando a entidade precisa de uma operação extra (`IContaRepository.AtualizarSaldosAsync`, `IParcelaRepository.PagarAsync`/`DesfazerPagamentoAsync`).
- `Repository/` — implementações. `Repository<T>` implementa `IRepository<T>` para qualquer entidade `ISoftDelete`; entidades com necessidades extras têm repositório próprio que herda dessa base (`ContaRepository : IContaRepository`, etc.) chamando o `ContasDbContext` diretamente.
- `Script/` — schema do banco é gerenciado **manualmente** via scripts SQL Server (não há EF Core Migrations no projeto). `create.sql` é o schema completo original; `adicionar.sql` e os demais `adicionar_*.sql` são incrementos aplicados depois, no estilo "Generate Change Script" do SSMS. **Ao mudar um Model, sempre escrever/atualizar o script SQL correspondente** — o EF Core aqui só mapeia um schema que já existe, não cria/migra nada em runtime.

### 3.2 `Contas_Contratos` — contrato compartilhado

- `Dto/` — todos os DTOs, sem nenhuma dependência de EF Core ou lógica. Convenção de nomes por entidade `X`:
  - `AdicionarXDto` — payload de criação (`[Required]`/`[MaxLength]` via DataAnnotations).
  - `AtualizarXDto` — payload de atualização.
  - `XDto` — formato de leitura/resposta.
  - Casos especiais quando fazem sentido: `XResumoDto` (`ContaResumoDto`), DTOs de ação (`PagarParcelaDto`, `AlterarSenhaUsuarioDto`, `LoginUsuarioDto`/`LoginResponseDto`).
- Este é o único projeto que `Contas_Web` e `Contas_App` podem referenciar para tipos de domínio.

### 3.3 `Contas_Core` — regras de negócio

- `UseCase/<Entidade>/` — um arquivo por operação, nome `VerboEntidadeUseCase` (`AdicionarContaUseCase`, `ObterTodosParcelaUseCase`, `PagarParcelaUseCase`...). **Não há interfaces para UseCase** — são classes concretas, injetadas por tipo concreto no DI e nos controllers. Cada UseCase é enxuto: recebe o repositório (ou outro UseCase, em composições como `GerarParcelasDividaUseCase` reaproveitando `AdicionarParcelaUseCase`) e delega.
- `Biz/` — validação de regras de negócio na criação (`AdicionarXBiz`), com métodos booleanos pequenos e um `IsValid` que os combina. O `Adicionar<Entidade>UseCase` chama o `Biz` correspondente e lança `ArgumentException` se inválido; o Controller traduz isso em `400 BadRequest`.
- `Converters/` — classes estáticas `XConverter` com `ToEntity(AdicionarXDto)`, `ApplyUpdate(entity, AtualizarXDto)` e `ToDto(entity)` / `ToDto(IEnumerable<entity>)`. Toda tradução Model↔DTO passa por aqui; Controllers e UseCases nunca montam DTO/Model na mão.
- `Security/` — `PasswordHasher` (PBKDF2-HMACSHA256), `JwtSettings` (bind de `appsettings:Jwt`), `JwtTokenGenerator` (emite o Bearer token com claims `NameIdentifier`=IdUsuario e `Email`).

### 3.4 `Contas_Api` — REST

- `Controllers/` — um controller por entidade (`ContasController`, `DividasController`, ...), sempre `[ApiController] [Route("api/[controller]")]`. Padrão de cada action:
  - Injeta os UseCases concretos que precisa (construtor primário ou clássico, sem interfaces).
  - Pega o usuário autenticado com `User.GetUsuarioId()` (nunca confia no `IdUsuario` do corpo da requisição — ele é sempre sobrescrito pelo do token).
  - Checagem de posse do recurso: se a entidade tem `IdUsuario` direto (`Conta`, `Divida`, `Carteira`), compara direto; se não tem (`Parcela`, `Investimento`, `Historico`, `Operacao`), resolve a cadeia de pertencimento via um UseCase auxiliar (`ContaPertenceAoUsuarioAsync`, `CarteiraPertenceAoUsuarioAsync` etc., métodos privados no próprio controller) — em alguns casos em dois níveis (`Historico`/`Operacao` → `Investimento` → `Carteira` → `Usuario`). Sem posse comprovada, retorna `404 NotFound` (nunca `403`, para não revelar que o recurso existe).
  - `Categoria` e `Credor` são globais (sem `IdUsuario`), sem checagem de posse.
- `Extensions/ClaimsPrincipalExtensions.cs` — `GetUsuarioId()` lê o claim `NameIdentifier` do token.
- Autenticação: JWT Bearer (`AddAuthentication().AddJwtBearer(...)`), com `FallbackPolicy` global exigindo usuário autenticado — **todo endpoint novo é protegido por padrão**; só fica público com `[AllowAnonymous]` (hoje: login e health check em `AuthController`).
- `Program.cs` é o único lugar de registro de DI: repositórios (genérico + específicos) e **um `AddScoped<XUseCase>()` por UseCase**, em ordem alfabética por entidade. Não existe arquivo de extensão `AddCoreServices`/`AddUseCases` — é tudo inline.
- OpenAPI via `Microsoft.AspNetCore.OpenApi` (não Swashbuckle) com transformers que injetam o esquema Bearer e marcam `Security` em todo endpoint sem `[AllowAnonymous]`.

### 3.5 `Contas_Web` — Blazor Server

- `Components/Pages/*.razor` — páginas roteadas (`@page`). `Home.razor` (`/`) e `Login.razor` (`/login`) hoje; páginas de template (`Counter`, `Weather`) ainda presentes mas não fazem parte do domínio. `Home.razor` mostra o resumo do dashboard (`DashboardApiService`) e, abaixo, a lista de contas do usuário com saldo editável por linha e um botão "Atualizar Saldos" que salva tudo em lote (`ContasApiService.AtualizarSaldosAsync` → `PUT api/Contas/saldos`) e recarrega o resumo em seguida.
- `Services/AuthSession.cs` — Scoped, guarda `Token`/`Usuario` em `ProtectedSessionStorage` (sobrevive à troca de circuito do Blazor Server). Toda página autenticada chama `RestoreAsync()` no `OnInitializedAsync` e redireciona para `/login` se `!IsAuthenticated`.
- **Anexar o Bearer token:** cada `XApiService` que precisa de autenticação injeta `AuthSession` direto no construtor e monta o header por requisição (`request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authSession.Token)` num `HttpRequestMessage`, antes de `httpClient.SendAsync(request)`) — ver `DashboardApiService`. **Não usar um `DelegatingHandler` registrado via `.AddHttpMessageHandler<T>()` para isso**: o `IHttpClientFactory` resolve os handlers de um escopo de DI interno, reaproveitado entre circuitos por até `HandlerLifetime` (2 min por padrão) — um `AuthSession` Scoped injetado nesse handler acaba sendo uma instância "congelada" de outro circuito, nunca a do usuário atual, e o token nunca é enviado (bug real encontrado e corrigido: `AuthHeaderHandler` existiu e foi removido por esse motivo — só `AuthApiService`, usado para login, não precisa de token).
- Não há camada de "serviço de API" única: cada família de endpoints tem seu próprio `XApiService` tipado (`AuthApiService`, `DashboardApiService`, `ContasApiService`), registrado em `Program.cs` via `AddHttpClient<XApiService>(...)`.
- `PUT api/Contas/saldos` (`ContasController.AtualizarSaldos` → `AtualizarSaldosContaUseCase` → `IContaRepository.AtualizarSaldosAsync`) existia no backend desde o commit `4c78d0e` mas não tinha nenhum caller em `Contas_Web`/`Contas_App` até `ContasApiService.AtualizarSaldosAsync` (tela Home). Ele só grava os valores de `Saldo` recebidos (`IEnumerable<ContaResumoDto>`) — não recalcula nada a partir de parcelas pagas/investimentos; pagar uma parcela (`PagarParcelaUseCase`) não altera `Conta.Saldo`. Se um dia for necessário recalcular saldo automaticamente a partir de parcelas, essa fórmula ainda não existe em lugar nenhum do código e precisa ser desenhada do zero.

### 3.6 `Contas_App` — MAUI

- `Pages/*.xaml` + code-behind — **sem MVVM framework** (não há `CommunityToolkit.Mvvm`, nem pasta `ViewModels`). Lógica de UI e chamada de serviço ficam direto no `.xaml.cs`.
- Navegação via Shell (`AppShell.xaml`), rotas registradas por página.
- `Services/AuthApiService.cs` — chamado pelas páginas de login/registro. **Atenção:** hoje o token retornado pelo login não é persistido em `SecureStorage`/`Preferences` (só e-mail/senha são, para biometria via `CredentialStore`) e nenhuma chamada HTTP no App anexa `Authorization`. Qualquer novo endpoint autenticado chamado pelo App vai falhar com 401 até isso ser resolvido.

### 3.7 `Contas_Test` — testes

- `Repository_Tests/` — testes de repositório contra EF Core InMemory.
- `UseCase_Tests/` — testes de UseCase (mock/InMemory dos repositórios).
- `Api_Tests/` — testes de integração via `WebApplicationFactory<Program>` (`Contas_Api`), com `ApiTestBase` fornecendo `Client` (HttpClient autenticado) e `CurrentUser`, e `SeedAsync<T>` para inserir entidades direto no `DbContext` InMemory. Cada `XControllerTests` segue o padrão: helpers `SeedXAsync`/`SeedDependenciasAsync` no topo, depois um `[TestMethod]` por cenário (`ObterTodos_...`, `Adicionar_...DeveRetornarNotFound_QuandoXNaoPertenceAoUsuarioAtual`, etc.).
- `Conexao_Tests/` — teste de conectividade básica com o banco real.

## 4. Regras de desenvolvimento (o padrão a seguir em features novas)

1. **Fluxo de uma feature de CRUD nova:** Model (+ script SQL) → Repository/Interface (só se precisar de operação além do `IRepository<T>` genérico) → DTOs em `Contas_Contratos` → Converter em `Contas_Core/Converters` → UseCases em `Contas_Core/UseCase/<Entidade>` (+ `Biz` se houver validação de criação) → Controller em `Contas_Api/Controllers` → registrar UseCases novos em `Program.cs` → testes em `Contas_Test`.
2. **Nomenclatura em português** para tudo que é domínio (classes, propriedades, rotas de negócio); inglês só para termos técnicos genéricos (`UseCase`, `Repository`, `Converter`, `Dto`).
3. **UseCase é a menor unidade de regra de negócio.** Um UseCase por operação, sem interface, sem orquestrar múltiplas entidades a menos que seja explicitamente esse o propósito dele (ex.: `GerarParcelasDividaUseCase` reaproveita `AdicionarParcelaUseCase`). Orquestração entre UseCases de entidades diferentes (ex.: "criar dívida e gerar parcelas") fica no Controller, que chama os UseCases em sequência.
4. **Controllers ficam finos.** Nunca acessam `DbContext`/`Repository` diretamente, nunca fazem `new Model {...}` (isso é papel do `Converter`), nunca embutem regra de negócio de validação (isso é papel do `Biz`).
5. **DTOs não têm comportamento**, só propriedades com DataAnnotations para validação de shape (`[Required]`, `[MaxLength]`). Note que `[Required]` em `int`/`decimal` não bloqueia o valor `0` (é no-op para tipo de valor não-nulável) — quando "0 é inválido" for uma regra real, ela precisa estar no `Biz` ou na checagem de posse do Controller, não só na anotação do DTO.
6. **Toda entidade nova usa exclusão lógica** (`ISoftDelete`/`Ativo`), nunca `DELETE` físico, exceto onde o Controller expõe explicitamente um endpoint de exclusão física (`Excluir`, que hoje chama `DeleteAsync`) — os dois padrões coexistem por entidade (`Excluir` = físico, `Inativar` = lógico via `SoftDeleteAsync`).
7. **Checagem de posse do recurso é obrigatória** em qualquer entidade vinculada a usuário, direta ou indiretamente, e deve retornar `404` (não `403`) quando o recurso não pertence ao usuário autenticado.
8. **Banco de dados não usa EF Migrations.** Toda alteração de schema é um novo script `.sql` em `Contas_Db/Script/`, aplicado manualmente pelo desenvolvedor no SQL Server — nunca assumir que rodar a aplicação vai criar/atualizar tabelas.
9. **Clientes (`Contas_Web`, `Contas_App`) só enxergam `Contas_Contratos`.** Se uma tela precisar de um tipo novo, ele nasce ali, nunca em `Contas_Core`.

## 5. Autenticação e segurança

- Login (`POST /api/auth/login`) valida e-mail/senha (hash PBKDF2) e emite um JWT (`JwtTokenGenerator`), claims `NameIdentifier` (id do usuário) e `Email`, expiração configurável (`Jwt:ExpirationMinutes`).
- Toda a API exige Bearer token por padrão (`FallbackPolicy`); endpoints públicos usam `[AllowAnonymous]` explicitamente.
- Não há refresh token nem revogação — o token vale até expirar.
- CORS não está configurado na API (nenhum middleware `UseCors`); hoje não há necessidade porque `Contas_Web` roda server-side (Blazor Server) e `Contas_App` é nativo, nenhum dos dois faz chamada cross-origin via browser.

## 6. Execução em Docker

Só a `Contas_Api` é containerizada. `Contas_Web` (Blazor Server) continua rodando fora do container e `Contas_App` (MAUI) não compila em Linux — o `Dockerfile` deliberadamente não copia nenhum dos dois, nem `Contas_Test`.

- `Contas_Api/Dockerfile` — multi-stage (`sdk:10.0` para build/publish, `aspnet:10.0` para runtime), roda com o usuário não-root `app` (`$APP_UID`) e escuta em `8080` dentro do container.
- **O contexto de build é a raiz da solução**, não a pasta da API, porque a API referencia `Contas_Core`, `Contas_Db` e `Contas_Contratos`: `docker build -f Contas_Api/Dockerfile -t contas-api:latest .`. Os `.csproj` são copiados antes do código-fonte para o `restore` aproveitar o cache de camadas.
- `DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false` + `LANG=pt_BR.UTF-8` são obrigatórios: a aplicação formata moeda e datas em pt-BR, e o modo invariant quebraria isso.
- `docker-compose.yml` sobe o serviço `contas-api` e monta a connection string por interpolação das variáveis do `.env` (`DB_SERVER`, `DB_NAME`, `DB_USER`, `DB_PASSWORD`). Não há `env_file` de propósito — assim a senha entra apenas dentro da connection string e não fica solta como variável de ambiente do container.
- **Autenticação Windows (`Integrated Security`) não funciona em container Linux** — é obrigatório usar um login SQL Server (autenticação SQL) para o container alcançar o banco.
- A porta publicada no host é `${API_PORT:-5210}`, não 8080: a 8080 está reservada pelo Windows/Hyper-V na máquina de desenvolvimento.
- `.env` está no `.gitignore` (contém credenciais); `.env.example` é o modelo versionado. O `.dockerignore` bloqueia os dois na imagem, além de `bin/`, `obj/`, `.git/`, `spec/` e dos projetos que não entram no build.
- O banco **não** é containerizado e continua sendo aplicado manualmente pelos scripts de `Contas_Db/Script/` (ver regra 8 da seção 4) — subir o container não cria nem atualiza tabelas.

## 7. Observações / pontos de atenção conhecidos

- `Contas_App` não persiste o token JWT após o login — qualquer chamada autenticada feita pelo App hoje falharia com 401 até isso ser implementado.
- `ContasDbContext.OnConfiguring` tem uma connection string hardcoded como fallback (nome de servidor interno). Só é usada se a aplicação não configurar o `DbContext` via DI — a API sempre configura, então isso é inofensivo em produção, mas vale trocar por algo neutro se o projeto for aberto para fora.
- Há pastas vazias remanescentes em `Contas_Core` (`Dto/`, `NovaPasta/`) que não são rastreadas pelo Git — podem ser removidas com segurança quando notadas no Explorador de Soluções.
