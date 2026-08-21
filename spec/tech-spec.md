# Tech Spec — Projeto_Contas

> Documento de referência da arquitetura, convenções e regras de desenvolvimento do projeto. Escrito a partir do estado atual do código (agosto/2026). Atualize este arquivo quando decisões estruturais mudarem.

## 1. Visão geral

Projeto_Contas é um sistema de controle financeiro pessoal: contas bancárias, dívidas/parcelas, investimentos (carteiras, histórico, operações), categorias e credores, por usuário autenticado.

**Dívida e receita são o mesmo cadastro.** `tb_divida` tem a coluna `dm_divida` (`Divida.EhDivida`): `1` = dívida a pagar, `0` = receita a receber. As parcelas geradas são idênticas nos dois casos — a `Parcela` **não** guarda essa distinção, quem a carrega é a dívida de origem. Qualquer cálculo que precise separar "a pagar" de "a receber" tem que resolver `Parcela.IdDivida → Divida.EhDivida` (ver `ObterResumoDashboardUseCase`).

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

- `Components/Pages/*.razor` — páginas roteadas (`@page`). `Home.razor` (`/`) e `Login.razor` (`/login`) na raiz; páginas de template (`Counter`, `Weather`) ainda presentes como arquivo, mas fora do menu e do domínio. `Home.razor` mostra o resumo do dashboard (`DashboardApiService`) e, abaixo, a lista de contas do usuário com saldo editável por linha e um botão "Atualizar Saldos" que salva tudo em lote (`ContasApiService.AtualizarSaldosAsync` → `PUT api/Contas/saldos`) e recarrega o resumo em seguida.
- **CRUD por entidade:** uma subpasta por model em `Components/Pages/<Entidade>/`, com duas páginas — `Lista<Entidades>.razor` (`@page "/<entidades>"`) e `Form<Entidade>.razor` (`@page "/<entidades>/novo"` + `@page "/<entidades>/{Id:int}/editar"`). São 10 pares, um por model: Contas, Categorias, Credores, Usuários, Dívidas, Parcelas, Carteiras, Investimentos, Operações, Históricos. Todas as listagens seguem a mesma forma: busca por texto, alternador "Mostrar inativos" (padrão só ativos), coluna Situação, e Editar/Inativar/Excluir por linha.
- `Components/PaginaAutenticada.cs` — classe base de **todas** as páginas autenticadas. Faz `AuthSession.RestoreAsync()`, redireciona para `/login` se necessário e então chama o `CarregarAsync()` abstrato da página; expõe `Carregando`, `UsuarioId`, `Mensagem`/`MensagemSucesso` e `AplicarResultadoAsync`. `OnInitializedAsync` é `sealed` — a página implementa `CarregarAsync`, nunca `OnInitializedAsync`.
- Toda página autenticada usa `@rendermode @(new InteractiveServerRenderMode(prerender: false))`. O prerender **precisa** estar desligado: `ProtectedSessionStorage` (usado por `AuthSession`) não existe durante o prerender.
- `Components/Shared/` — `BarraFiltro` (busca + inativos + botão Novo), `AcoesLinha` (Editar/Inativar/Excluir), `PagamentoParcela` (estado do pagamento + Pagar/Desfazer), `Alerta` e `Situacao`. `AcoesLinha` e `PagamentoParcela` confirmam **inline**, sem `confirm()`/`prompt()` do JS: um diálogo modal do browser travaria o circuito do Blazor Server — daí o `<input type="date">` na própria linha para escolher a data do pagamento.
- **Filtro por querystring:** `ListaParcelas` aceita `?idDivida={id}` (`[SupplyParameterFromQuery]`) e restringe a lista àquela dívida; `ListaDividas` e o quadro anual linkam para lá com "Ver parcelas", e `FormParcela` recebe o mesmo parâmetro para pré-selecionar a dívida e voltar para a lista ainda filtrada. É o padrão a seguir para navegação pai→filho entre listagens.
- **Filtro persistido:** na própria `ListaParcelas` a dívida também é escolhida numa dropdown, aplicada no clique em "Filtrar" e desfeita em "Limpar filtro" — dois campos (`_idDividaSelecionada`, o que está na dropdown; `_idDividaAplicada`, o que filtra de fato). O id fica no `ProtectedSessionStorage` (chave `parcelas-filtro-divida`) e é restaurado uma vez por circuito, porque campo de componente não sobrevive à troca de circuito do Blazor Server. A querystring tem precedência sobre o valor salvo **e passa a ser o valor salvo**. A restauração é protegida por flag: `CarregarAsync` roda de novo depois de cada escrita e sem a guarda desfaria um filtro recém-trocado. Filtro salvo apontando para dívida inexistente volta para "todas".
- **Relatório fora do par Lista/Form:** `Components/Pages/Parcelas/QuadroAnualParcelas.razor` (`/parcelas/anual`, menu "Quadro anual") cruza dívidas/receitas × 12 meses do ano atual — uma linha por dívida/receita, parcelas somadas na coluna do mês de vencimento, seções Dívidas/subtotal, Receitas/subtotal e resultado. Como a `Parcela` não sabe se é dívida ou receita, o agrupamento resolve `IdDivida` contra a lista de dívidas (`EhDivida`), e parcela órfã entra como dívida com rótulo `#id`. Inclui pagas e em aberto; exclui inativas.
- `Filtro.cs` / `Formato.cs` (raiz do projeto) — `Filtro.Corresponde(termo, campos...)` compara sem diferenciar maiúsculas nem acentuação (para "divida" achar "dívida"); `Formato` centraliza moeda/data/quantidade em pt-BR, mais `Valor` (sem o símbolo da moeda, para tabelas com muitas colunas de dinheiro) e `Mes(int)` (Jan, Fev, Mar…).
- **Cuidados ao montar um formulário novo** (os dois já causaram bug real):
  1. **Não use um `Adicionar<X>Dto` como `Model` do `EditForm` se algum campo `[Required]` dele não tiver input na tela.** O `DataAnnotationsValidator` barra o submit, `OnValidSubmit` nunca roda e — sem um `ValidationMessage` para aquele campo — **nada aparece**: o botão Salvar simplesmente não faz nada. Foi o caso de `AdicionarHistoricoDto.NomeInvestimento` (desnormalizado, derivado do investimento escolhido). Nesses casos use um modelo próprio na página e monte o DTO no `SalvarAsync` — ver `FormHistorico` e `FormUsuario`.
  2. **`[Required]` em `int`/`decimal` não barra o valor 0** (é no-op para tipo de valor não-nulável, mesma observação da regra 5 da seção 4). A opção "Selecione..." dos `InputSelect` de FK vale `0`, então cada formulário com FK obrigatória checa explicitamente `== 0` no `SalvarAsync` antes de chamar a API.
- `Services/AuthSession.cs` — Scoped, guarda `Token`/`Usuario` em `ProtectedSessionStorage` (sobrevive à troca de circuito do Blazor Server). Toda página autenticada chama `RestoreAsync()` no `OnInitializedAsync` e redireciona para `/login` se `!IsAuthenticated`.
- **Anexar o Bearer token:** o header é montado por requisição, num `HttpRequestMessage` (`request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authSession.Token)`, antes de `httpClient.SendAsync(request)`) — hoje centralizado em `Services/ApiClient.cs`. **Não usar um `DelegatingHandler` registrado via `.AddHttpMessageHandler<T>()` para isso**: o `IHttpClientFactory` resolve os handlers de um escopo de DI interno, reaproveitado entre circuitos por até `HandlerLifetime` (2 min por padrão) — um `AuthSession` Scoped injetado nesse handler acaba sendo uma instância "congelada" de outro circuito, nunca a do usuário atual, e o token nunca é enviado (bug real encontrado e corrigido: `AuthHeaderHandler` existiu e foi removido por esse motivo — só `AuthApiService`, usado para login, não precisa de token).
- **Camada de acesso à API:** `Services/ApiClient.cs` concentra o `HttpClient` (registrado com `AddHttpClient<ApiClient>`), o Bearer token e a tradução da resposta em `ApiResultado(Sucesso, Erro)` — as validações de `Biz` voltam da API como `BadRequest(string)` e a mensagem é exibida direto na tela. Acima dele há um serviço fino por entidade (`ContasApiService`, `CategoriasApiService`, …, 10 no total, `AddScoped`), que só mapeia as rotas do recurso. `AuthApiService` e `DashboardApiService` continuam com `HttpClient` próprio via `AddHttpClient<XApiService>`.
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

- **`GET api/Dividas` está quebrado pelos dados, não pelo código.** `tb_divida.id_conta` e `tb_divida.id_categoria` foram adicionadas ao banco como colunas **nullable** (aparecem no fim da ordem de colunas), mas o model `Divida` as declara `[Required] int` (não-nulável). Existem 2 linhas legadas (`Funlec`, `IPTU`, ambas do usuário 1) com `NULL` nas duas, e como `ObterTodosDividaUseCase` materializa **todas** as linhas antes de filtrar por usuário, o EF Core estoura ao ler essas linhas e o endpoint responde 500. Isso derruba a tela `/dividas` e esvazia o dropdown de dívida em `/parcelas/novo`. Resolver escolhendo uma das três: preencher `id_conta`/`id_categoria` das 2 linhas, apagá-las, ou tornar as duas propriedades do model `int?` (e ajustar as checagens de posse que dependem de `IdConta`).
- **`GET api/Usuarios` devolve todos os usuários do sistema.** Não há filtro por dono nem papel de administrador, então a tela `/usuarios` do `Contas_Web` expõe nome e e-mail de qualquer usuário cadastrado para qualquer usuário logado. Aceitável enquanto o sistema é de uso pessoal; se ganhar mais de um usuário real, esse endpoint precisa de autorização antes de qualquer coisa.
- **`ObterResumoDashboardUseCase` lê todas as dívidas do banco** (`ObterTodosDividaUseCase.ExecuteAsync()` não filtra por usuário) só para montar o conjunto de ids cujo `EhDivida` é `false` e assim separar dívidas de receitas nos totais. Não há vazamento — as parcelas somadas já são as do usuário autenticado, e só os ids são usados —, mas é uma varredura de tabela cheia por acesso à Home, e herda o problema das linhas legadas de `tb_divida` descrito acima.
- O quadro anual (`/parcelas/anual`) segue a mesma linha: baixa **todas** as parcelas e **todas** as dívidas e faz o cruzamento em memória, sem endpoint de agregação na API. Em volume pessoal é irrelevante; se a base crescer, esse relatório é o primeiro candidato a virar um `GET` que já devolva os totais por mês.
- As listagens do `Contas_Web` carregam a coleção inteira e filtram em memória — não há paginação nem filtro no servidor (`GET api/<recurso>` não aceita parâmetros de busca). As telas que mostram nomes de FK (Dívidas, Parcelas, Investimentos, Operações, Históricos) fazem GETs adicionais nas listas relacionadas e cruzam por `Dictionary<int, string>`. Funciona bem em volume pessoal; com muitos registros vale mover filtro e paginação para a API.
- **`ApplyUpdate` dos Converters sobrescreve as colunas de imagem** (`img_conta`, `img_categoria`, `img_logo`, imagem do usuário) com o que vier no DTO. Os formulários do `Contas_Web` não editam imagem, mas precisam devolver os bytes atuais no `PUT` — caso contrário a imagem existente é apagada. Qualquer novo cliente que chame `PUT` precisa do mesmo cuidado.
- `AtualizarDividaDto` não tem `IdConta` nem `IdCategoria` (ao contrário de `AdicionarDividaDto`), então não é possível trocar a conta ou a categoria de uma dívida existente — o formulário mostra os dois campos como somente-leitura na edição.
- `OperacaoDto` não tem nenhum campo de texto (nome/descrição). A listagem de Operações filtra pelo **nome do investimento**, resolvido a partir de `IdInvestimento`.
- `Contas_App` não persiste o token JWT após o login — qualquer chamada autenticada feita pelo App hoje falharia com 401 até isso ser implementado.
- `ContasDbContext.OnConfiguring` tem uma connection string hardcoded como fallback (nome de servidor interno). Só é usada se a aplicação não configurar o `DbContext` via DI — a API sempre configura, então isso é inofensivo em produção, mas vale trocar por algo neutro se o projeto for aberto para fora.
- Há pastas vazias remanescentes em `Contas_Core` (`Dto/`, `NovaPasta/`) que não são rastreadas pelo Git — podem ser removidas com segurança quando notadas no Explorador de Soluções.
