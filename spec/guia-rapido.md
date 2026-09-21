# Guia rápido — Projeto_Contas

> Referência de consulta rápida: como o código deve ser escrito, arquitetura de cada projeto,
> comandos de build/publish/run e histórico resumido dos commits.
> Para o detalhamento longo (o porquê de cada decisão), ver [`arquitetura-detalhada.md`](arquitetura-detalhada.md).
> Atualizado em 2026-09-21 (commit `ce561ef`).

---

## 1. Regras de escrita do projeto

Valem para **todo** código novo, em qualquer projeto da solução.

| # | Regra |
|---|---|
| 1 | **Nomenclatura em português** para domínio (classes, propriedades, rotas, métodos de negócio). Inglês só em termos técnicos consagrados: `UseCase`, `Repository`, `Converter`, `Dto`, `Service`. |
| 2 | **Um arquivo por operação/responsabilidade.** `AdicionarContaUseCase`, `ObterTodosParcelaUseCase` — nunca uma classe "Service" com 10 métodos. |
| 3 | **UseCase é a menor unidade de regra de negócio**, classe concreta, sem interface, com um único `ExecuteAsync`. Orquestração entre entidades diferentes fica no Controller. |
| 4 | **Controller fino:** nunca acessa `DbContext`/`Repository`, nunca faz `new Model {...}` (papel do `Converter`), nunca embute validação (papel do `Biz`). |
| 5 | **Validação de negócio vive em `Contas_Core/Biz`** (`AdicionarXBiz`, métodos booleanos pequenos + um `IsValid`). O UseCase lança `ArgumentException`; o Controller traduz em `400 BadRequest` com a mensagem pronta para a tela. |
| 6 | **Tradução Model↔DTO só em `Contas_Core/Converters`** (`ToEntity`, `ApplyUpdate`, `ToDto`). |
| 7 | **DTOs não têm comportamento**, só propriedades + DataAnnotations. Atenção: `[Required]` em `int`/`decimal` **não** barra o valor `0`. |
| 8 | **Exclusão lógica por padrão** (`ISoftDelete`/`Ativo`, via `SoftDeleteAsync`). `Inativar` = lógico, `Excluir` = físico; os dois coexistem por entidade. |
| 9 | **Checagem de posse obrigatória** em qualquer entidade ligada a usuário. Sem posse → `404` (nunca `403`, para não revelar que o recurso existe). O `IdUsuario` vem **sempre** do token (`User.GetUsuarioId()`), nunca do corpo. |
| 10 | **Clientes (`Contas_Web`, `Contas_App`) referenciam apenas `Contas_Contratos`.** Nunca adicionar `ProjectReference` para `Contas_Core`/`Contas_Db`. Tipo novo de tela nasce em `Contas_Contratos/Dto`. |
| 11 | **Sem EF Migrations.** Toda mudança de schema é um `.sql` novo em `Contas_Db/Script/`, aplicado à mão. Mudou Model → escreveu script. |
| 12 | **Todo endpoint novo já nasce autenticado** (`FallbackPolicy` global). Público exige `[AllowAnonymous]` explícito. |
| 13 | **Formatação sempre pt-BR** (moeda, data). No Web via `Formato.cs`; no App via `CultureInfo("pt-BR")`. |
| 14 | **Comentário só explica o "porquê"**, quando a decisão não é óbvia pelo código. Sem comentário narrando o que a linha faz. |

**Fluxo de uma feature de CRUD nova:**
`Model` (+ script SQL) → `Repository/Interface` (só se precisar de operação além do `IRepository<T>`) → DTOs em `Contas_Contratos` → `Converter` → `UseCase` (+ `Biz`) → `Controller` → registrar no `Program.cs` da API → testes em `Contas_Test` → telas em `Contas_Web`/`Contas_App`.

---

## 2. Arquitetura — `Contas_App` (.NET MAUI)

Cliente mobile/desktop. Targets: `net10.0-android`, `-ios`, `-maccatalyst`, `-windows10.0.19041.0`.

```
Contas_App/
├── App.xaml(.cs)            janela 390x844 (simula celular no Windows)
├── AppShell.xaml            Shell com 3 rotas: LoginPage, RegisterPage, MainPage
├── MauiProgram.cs           único ponto de DI
├── MainPage.xaml(.cs)       tabela de contas + saldo editável em lote
├── Pages/                   LoginPage, RegisterPage (.xaml + code-behind)
└── Services/
    ├── ApiConfig.cs         BaseUrl (10.0.2.2 no Android, localhost no resto)
    ├── AppSession.cs        token JWT + usuário logado (memória, enquanto o app vive)
    ├── ApiClient.cs         HttpClient único, Bearer por requisição, ApiResultado
    ├── AuthApiService.cs    login/registro (não precisa de token)
    ├── ContasApiService.cs  mapeia api/contas/*
    ├── ContaSaldoItem.cs    item de tela (INotifyPropertyChanged)
    └── CredentialStore.cs   e-mail/senha em SecureStorage para biometria
```

**Regras do App:**

- **Sem MVVM framework.** Não há `CommunityToolkit.Mvvm` nem pasta `ViewModels` — lógica de UI fica no code-behind (`.xaml.cs`). Manter assim até haver decisão explícita de mudar.
- **Camadas de acesso à API espelham o `Contas_Web`:** `ApiClient` (transporte + token + tradução de erro) → um `XApiService` fino por entidade (só mapeia rotas). Serviço novo nunca cria `HttpClient` próprio.
- **`AppSession` é `Singleton`**; `ApiClient` lê o token dela a cada requisição. O login chama `_session.SignIn(token, usuario)` — sem isso toda tela seguinte toma `401`.
- **Página com dependência entra no DI** (`AddTransient<XPage>()` no `MauiProgram`) e recebe os serviços por construtor.
- **Objetos de tela ficam em `Services/`** (ex.: `ContaSaldoItem`) implementando `INotifyPropertyChanged`, com `CollectionView` + `DataTemplate` tipado (`x:DataType`).
- **Entrada numérica aceita vírgula e ponto** — o teclado numérico do Android nem sempre oferece a vírgula.
- **Operação em lote valida tudo antes de enviar** (ex.: salvar saldos): um item inválido não pode deixar os outros irem pela metade.
- **O token não é persistido** entre execuções (só e-mail/senha, para biometria). Reabrir o app exige login.

---

## 3. Arquitetura — `Contas_Db`

Única camada que conhece EF Core e SQL Server. Não depende de nenhum outro projeto do repositório.

```
Contas_Db/
├── Model/
│   ├── ContasDbContext.cs       um DbSet<T> por entidade
│   ├── Interface/ISoftDelete.cs bool Ativo { get; set; }
│   └── <10 entidades>.cs        Carteira, Categoria, Conta, Credor, Divida,
│                                Historico, Investimento, Operacao, Parcela, Usuario
├── Repository/
│   ├── Interface/               IRepository<T> + IContaRepository, IParcelaRepository, ...
│   └── *.cs                     Repository<T> genérico + repositórios específicos
└── Script/                      create.sql + adicionar_*.sql (schema manual)
```

**Regras:**

- **Mapeamento por DataAnnotations**, sem Fluent API / `OnModelCreating`. Padrão de cada entidade:
  `[Table("tb_conta")]`, `[Key] [Column("id_conta")]`, `[Column("nm_conta")]`, `[Column("nr_saldo", TypeName = "numeric(10,2)")]`, `[Column("dm_ativo")]`.
  Prefixos das colunas: `id_` (chave/FK), `nm_` (nome), `ds_` (descrição), `nr_` (número/valor), `dt_` (data), `dm_` (domínio/flag), `img_` (imagem).
- **Toda entidade implementa `ISoftDelete`** e expõe `Ativo`.
- **FK sempre em par:** `int IdX` + propriedade de navegação `X? X` com `[ForeignKey(nameof(IdX))]`.
- **Campo calculado usa `[NotMapped]`** (ex.: `Parcela.Pago => DataPagamento.HasValue`).
- **`Repository<T>` genérico** cobre `GetByIdAsync`, `GetAllAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync`, `SoftDeleteAsync`. Cada método salva sozinho (`SaveChangesAsync` dentro do repositório) — não há Unit of Work.
- **Repositório específico só quando há operação extra** (`IContaRepository.AtualizarSaldosAsync`, `IParcelaRepository.PagarAsync`). O contrato fica em `Repository/Interface/`.
- **Schema é manual.** `create.sql` é o schema original; cada mudança posterior é um `adicionar_*.sql`. Rodar a aplicação **não** cria nem migra tabelas.
- `OnConfiguring` tem uma connection string de fallback (`MS211,1434 / test_fin`), usada só quando ninguém configura via DI (a API sempre configura).

---

## 4. Regras do `Contas_Test`

MSTest 4 + `Microsoft.EntityFrameworkCore.InMemory` + `Microsoft.AspNetCore.Mvc.Testing`.

```
Contas_Test/
├── MSTestSettings.cs        [assembly: Parallelize(Scope = MethodLevel)]
├── Repository_Tests/        1 arquivo por entidade — repositório contra InMemory
├── UseCase_Tests/           1 arquivo por entidade — UseCase + Biz contra InMemory
├── Api_Tests/               ApiTestBase + 1 XControllerTests por controller
└── Conexao_Tests/           único teste que toca o SQL Server real
```

**Regras obrigatórias:**

1. **Banco em memória, sempre.** Todo teste usa `UseInMemoryDatabase(Guid.NewGuid().ToString())` — **nome novo por teste**, nunca um nome fixo. Só `Conexao_Tests/ConexaoTest` fala com SQL Server real (e é o único que falha sem banco).
2. **Ciclo de vida padrão:** `[TestInitialize] Setup()` cria `DbContextOptionsBuilder` → `ContasDbContext` → repositório; `[TestCleanup] Cleanup()` faz `_context.Dispose()`.
3. **Testes rodam em paralelo por método** — por isso nada de estado estático compartilhado nem nome de banco fixo.
4. **Classe de teste é `sealed`**, decorada com `[TestClass]`; campos `null!` inicializados no `Setup`.
5. **Nome do teste descreve cenário e expectativa**, em português:
   `Metodo_DeveFazerAlgo`, `Metodo_DeveLancarExcecao_QuandoCondicao`, `Adicionar_DeveRetornarNotFound_QuandoContaNaoPertenceAoUsuarioAtual`.
6. **Factory local de entidade no topo da classe** (`private static Conta CriarConta(string nome = "Conta Corrente")`), com valores padrão e parâmetro para o que o teste varia.
7. **Teste de UseCase usa repositório real sobre InMemory** (não mock) — a validação `Biz` é parte do que se quer testar. Erro esperado: `Assert.ThrowsExactlyAsync<ArgumentException>`.
8. **Teste de API herda de `ApiTestBase`**, que dá:
   - `Client` — `HttpClient` já autenticado como `CurrentUser` (Bearer emitido pelo `JwtTokenGenerator` real);
   - `CurrentUser` — usuário seedado com e-mail único (`Guid`) e senha hasheada;
   - `CreateAnonymousClient()` — para cenários de `401`;
   - `SeedAsync<T>(entity)` — insere direto no `DbContext` InMemory, sem passar pela API.

   A `ApiFactory` remove os descritores do `ContasDbContext` registrados pela API e registra o InMemory no lugar — **nenhum teste de API toca SQL Server**.
9. **Cada `XControllerTests` segue a mesma ordem:** helpers `SeedXAsync`/`SeedDependenciasAsync` no topo, depois um `[TestMethod]` por cenário.
10. **Cobertura mínima de um endpoint novo:** caminho feliz + validação inválida (`400`) + recurso de outro usuário (`404`).
11. **Limitação conhecida do InMemory:** não valida constraints de SQL (CHECK, FK, tamanho de coluna) nem executa SQL bruto. Regra que dependa disso precisa estar no `Biz`, não no banco.

---

## 5. Arquitetura — `Contas_Web` (Blazor Server)

```
Contas_Web/
├── Program.cs                     DI: AuthSession, ApiClient, 10 XApiService
├── Filtro.cs / Formato.cs         busca sem acento/caixa; moeda, data, mês pt-BR
├── Components/
│   ├── PaginaAutenticada.cs       classe base de toda página autenticada
│   ├── Layout/                    MainLayout, NavMenu
│   ├── Shared/                    BarraFiltro, AcoesLinha, PagamentoParcela,
│   │                              Alerta, Situacao
│   └── Pages/
│       ├── Home.razor (/)         dashboard + contas com saldo editável
│       ├── Login.razor (/login)
│       ├── <Entidade>/            Lista<Entidades>.razor + Form<Entidade>.razor
│       │                          (10 pares: Contas, Categorias, Credores, Usuarios,
│       │                           Dividas, Parcelas, Carteiras, Investimentos,
│       │                           Operacoes, Historicos)
│       └── Parcelas/QuadroAnualParcelas.razor  (/parcelas/anual)
└── Services/
    ├── AuthSession.cs             token/usuário em ProtectedSessionStorage
    ├── ApiClient.cs               HttpClient + Bearer + ApiResultado
    ├── AuthApiService.cs / DashboardApiService.cs   (HttpClient próprio)
    └── <Entidade>ApiService.cs    10 serviços finos, só rotas
```

**Regras do Web:**

- **Padrão de tela: par `Lista` + `Form` por entidade.** `Lista<Entidades>` em `/<entidades>`; `Form<Entidade>` em `/<entidades>/novo` e `/<entidades>/{Id:int}/editar`.
- **Toda listagem tem a mesma forma:** busca por texto, alternador "Mostrar inativos" (padrão: só ativos), coluna Situação, e Editar/Inativar/Excluir por linha.
- **Página autenticada herda de `PaginaAutenticada`** e implementa `CarregarAsync()` — nunca `OnInitializedAsync` (é `sealed` na base, que restaura a sessão e redireciona para `/login`).
- **`@rendermode @(new InteractiveServerRenderMode(prerender: false))` é obrigatório**: `ProtectedSessionStorage` não existe durante o prerender.
- **Nada de `confirm()`/`prompt()` do JS** — diálogo modal do browser trava o circuito do Blazor Server. Confirmação é inline (`AcoesLinha`, `PagamentoParcela`).
- **Bearer token é montado por requisição** dentro do `ApiClient` (`HttpRequestMessage` + `AuthenticationHeaderValue`). **Nunca via `DelegatingHandler`**: o `IHttpClientFactory` reusa handlers entre circuitos e congela um `AuthSession` de outro usuário.
- **Acesso à API em duas camadas:** `ApiClient` (transporte, token, `ApiResultado(Sucesso, Erro)`) → `XApiService` fino por entidade. A mensagem de `BadRequest` da API vai direto para a tela.
- **Navegação pai→filho por querystring** (`/parcelas?idDivida={id}`, via `[SupplyParameterFromQuery]`); filtro que deve sobreviver ao circuito vai para `ProtectedSessionStorage`.
- **Dois cuidados em formulário novo:**
  1. Não usar `Adicionar<X>Dto` como `Model` do `EditForm` se algum `[Required]` dele não tiver input na tela — o submit é barrado em silêncio. Use modelo próprio (ver `FormHistorico`, `FormUsuario`).
  2. `InputSelect` de FK tem "Selecione..." = `0`; checar `== 0` explicitamente no `SalvarAsync`.
- **Endereço da API** vem de `Api:BaseUrl` no `appsettings.json` (padrão `http://localhost:5210/`).

---

## 6. Comandos — compilar, publicar, executar

Todos a partir da **raiz da solução**. Pré-requisitos: .NET SDK 10, SQL Server acessível, workload MAUI (`dotnet workload install maui`) para o App.

### Solução inteira

```bash
dotnet restore Projeto_Contas.slnx
dotnet build Projeto_Contas.slnx                 # inclui o MAUI (exige workload)
dotnet build Projeto_Contas.slnx -c Release
```

### Contas_Api (REST — porta 5210)

```bash
dotnet build Contas_Api/Contas_Api.csproj
dotnet run   --project Contas_Api/Contas_Api.csproj          # http://localhost:5210
dotnet run   --project Contas_Api/Contas_Api.csproj --launch-profile https
dotnet publish Contas_Api/Contas_Api.csproj -c Release -o ./publish/api
```

Swagger (só em Development): `http://localhost:5210/swagger` · OpenAPI: `/openapi/v1.json`

### Contas_Web (Blazor Server — porta 5095)

```bash
dotnet build Contas_Web/Contas_Web.csproj
dotnet run   --project Contas_Web/Contas_Web.csproj          # http://localhost:5095
dotnet run   --project Contas_Web/Contas_Web.csproj --launch-profile https   # https://localhost:7106
dotnet publish Contas_Web/Contas_Web.csproj -c Release -o ./publish/web
```

> A API precisa estar no ar antes do Web (`Api:BaseUrl` no `appsettings.json`).

### Contas_App (MAUI)

```bash
# compilar por target
dotnet build Contas_App/Contas_App.csproj -f net10.0-windows10.0.19041.0
dotnet build Contas_App/Contas_App.csproj -f net10.0-android

# executar (Windows / emulador Android já aberto)
dotnet run   --project Contas_App/Contas_App.csproj -f net10.0-windows10.0.19041.0
dotnet build Contas_App/Contas_App.csproj -f net10.0-android -t:Run

# publicar
dotnet publish Contas_App/Contas_App.csproj -f net10.0-android -c Release          # .apk/.aab
dotnet publish Contas_App/Contas_App.csproj -f net10.0-windows10.0.19041.0 -c Release
```

> No emulador Android a API é alcançada por `10.0.2.2:5210` (já tratado em `ApiConfig`).
> No Visual Studio: definir `Contas_App` como projeto de inicialização e escolher o target na barra de ferramentas.

### Contas_Test

```bash
dotnet test Contas_Test/Contas_Test.csproj
dotnet test Contas_Test/Contas_Test.csproj --filter FullyQualifiedName~Api_Tests
dotnet test Contas_Test/Contas_Test.csproj --filter FullyQualifiedName~ContaUseCaseTests
dotnet test Contas_Test/Contas_Test.csproj --logger "console;verbosity=detailed"
```

> Tudo roda sem SQL Server, exceto `Conexao_Tests`:
> `dotnet test Contas_Test/Contas_Test.csproj --filter FullyQualifiedName!~Conexao_Tests`

### Bibliotecas (Contas_Core, Contas_Db, Contas_Contratos)

```bash
dotnet build Contas_Core/Contas_Core.csproj       # idem para Contas_Db, Contas_Contratos
```

### Docker (só a API)

```bash
cp .env.example .env        # Windows: copy .env.example .env  — preencher DB_USER/DB_PASSWORD

# via compose (recomendado)
docker compose up -d --build
docker compose logs -f contas-api
docker compose down

# build/run manuais — contexto é a RAIZ, não a pasta da API
docker build -f Contas_Api/Dockerfile -t contas-api:latest .
docker run -d -p 5210:8080 --name contas-api \
  -e ConnectionStrings__DefaultConnection="Server=...;Database=test_fin;User Id=...;Password=...;TrustServerCertificate=True" \
  contas-api:latest
```

> Container escuta em `8080` e é publicado em `${API_PORT:-5210}` no host — então **não** suba o container e o `dotnet run` da API ao mesmo tempo.
> Autenticação Windows (`Integrated Security`) **não** funciona em container Linux: use login SQL.

### Banco de dados

Sem EF Migrations. Aplicar manualmente no SQL Server, em ordem:

```
Contas_Db/Script/create.sql
Contas_Db/Script/adicionar.sql
Contas_Db/Script/adicionar_divida_conta_categoria.sql
Contas_Db/Script/adicionar_divida_dm_divida.sql
```

---

## 7. Pontos de atenção conhecidos

Problemas reais já identificados no código atual. Ler antes de mexer nas áreas citadas.

### Quebrado hoje

- **`GET api/Dividas` responde 500 — por dados, não por código.** `tb_divida.id_conta` e `id_categoria` foram adicionadas ao banco como **nullable**, mas o model `Divida` as declara `[Required] int`. Existem 2 linhas legadas (`Funlec`, `IPTU`, usuário 1) com `NULL` nas duas, e `ObterTodosDividaUseCase` materializa **todas** as linhas antes de filtrar por usuário — o EF Core estoura ao ler essas linhas. Derruba a tela `/dividas` e esvazia o dropdown de dívida em `/parcelas/novo`.
  Saídas possíveis: preencher as 2 linhas, apagá-las, ou tornar as duas propriedades `int?` (e ajustar as checagens de posse que dependem de `IdConta`).

### Segurança

- **`GET api/Usuarios` devolve todos os usuários do sistema.** Sem filtro por dono nem papel de administrador — a tela `/usuarios` expõe nome e e-mail de qualquer usuário cadastrado para qualquer usuário logado. Aceitável enquanto o uso é pessoal; com mais de um usuário real, esse endpoint precisa de autorização antes de qualquer outra coisa.
- **Sem refresh token nem revogação** — o JWT vale até expirar (`Jwt:ExpirationMinutes`).
- **CORS não está configurado** na API. Não faz falta hoje (Blazor Server chama server-side, MAUI é nativo), mas qualquer cliente que rode no browser vai esbarrar nisso.

### Armadilhas ao escrever código

- **`ApplyUpdate` dos Converters sobrescreve as colunas de imagem** (`img_conta`, `img_categoria`, `img_logo`, imagem do usuário) com o que vier no DTO. Quem chama `PUT` precisa devolver os bytes atuais, senão a imagem existente é apagada. Os formulários do Web não editam imagem, mas já devolvem o valor atual — **qualquer cliente novo precisa do mesmo cuidado**.
- **`AtualizarDividaDto` não tem `IdConta` nem `IdCategoria`** (ao contrário do `AdicionarDividaDto`): não dá para trocar conta ou categoria de uma dívida existente. O formulário mostra os dois campos como somente-leitura na edição.
- **`OperacaoDto` não tem nenhum campo de texto** (nome/descrição). A listagem de Operações filtra pelo **nome do investimento**, resolvido a partir de `IdInvestimento`.
- **`ContasDbContext.OnConfiguring` tem connection string hardcoded** como fallback (nome de servidor interno). Inofensiva na prática — a API sempre configura via DI —, mas vale trocar por algo neutro se o repositório for aberto.

### Desempenho (irrelevante em volume pessoal, primeiro a doer se crescer)

- **`ObterResumoDashboardUseCase` lê todas as dívidas do banco** só para montar o conjunto de ids cujo `EhDivida` é `false` e separar dívidas de receitas. Não há vazamento (as parcelas somadas já são as do usuário), mas é varredura de tabela cheia a cada acesso à Home — e herda o problema das linhas legadas acima.
- **O quadro anual (`/parcelas/anual`) baixa todas as parcelas e todas as dívidas** e cruza em memória, sem endpoint de agregação. Primeiro candidato a virar um `GET` que já devolva os totais por mês.
- **As listagens do Web carregam a coleção inteira e filtram em memória** — não há paginação nem filtro no servidor (`GET api/<recurso>` não aceita parâmetros de busca). As telas com FK ainda fazem GETs adicionais nas listas relacionadas e cruzam por `Dictionary<int, string>`.

### Contas_App

- **O token não sobrevive ao fechamento do app.** `AppSession` guarda token e usuário só em memória; o que é persistido (`SecureStorage`) são e-mail/senha para a biometria. Reabrir o app exige passar pelo login de novo.
- **Só a `MainPage` consome a API autenticada** hoje (contas/saldos). Tela nova precisa do seu `XApiService` registrado no `MauiProgram`.

### Manutenção

- Há pastas vazias remanescentes em `Contas_Core` (`Dto/`, `NovaPasta/`), não rastreadas pelo Git — podem ser removidas quando aparecerem no Explorador de Soluções.
- `Contas_Web` ainda tem as páginas de template `Counter.razor` e `Weather.razor` como arquivo (fora do menu e do domínio).

---

## 8. Histórico de commits (ordem cronológica)

| # | Commit | Data | Feature entregue |
|---|---|---|---|
| 1 | `840fc52` | 2026-07-08 | `.gitattributes` e `.gitignore`. |
| 2 | `c6b64ff` | 2026-07-08 | Esqueleto da solução: `Contas_App` (template MAUI), `Contas_Biz` e `Contas_Db` vazios, `Projeto_Contas.slnx`. |
| 3 | `d0ba7c7` | 2026-07-10 | **Primeiro backend real:** Models EF Core + `ContasDbContext`, `ISoftDelete`, `IRepository<T>`/`Repository<T>`, `ParcelaRepository`; nasce o `Contas_Test` com testes de repositório. |
| 4 | `6138442` | 2026-07-10 | Camada de UseCases (em `Contas_Biz`) e hashing PBKDF2 da senha do usuário. |
| 5 | `02545c2` | 2026-07-10 | Novo export do schema do banco. |
| 6 | `fdbc191` | 2026-07-14 | Consolida os scripts SQL em `Contas_Db/Script/create.sql`. |
| 7 | `d0b5447` | 2026-07-20 | Renomeia `Contas_Biz` → **`Contas_Core`**. |
| 8 | `18610e7` | 2026-07-20 | CHECK constraint de `tb_conta.nr_saldo`. |
| 9 | `c30c6bb` | 2026-07-20 | Merge PR #1 (rename Biz→Core). |
| 10 | `754ee8f` | 2026-07-20 | Camada **`Biz`** de validação de criação (`AdicionarXBiz` + `IsValid`). |
| 11 | `4c78d0e` | 2026-07-20 | `ContaResumoDto`, resumo de contas e **atualização de saldos em lote** (`PUT api/contas/saldos`). |
| 12 | `8b22deb` | 2026-07-20 | Merge PR #2. |
| 13 | `b56717b` | 2026-07-20 | README com visão geral. |
| 14 | `a53bac5` | 2026-07-23 | Reorganiza interfaces em `Model/Interface` e `Repository/Interface`. |
| 15 | `956090b` | 2026-07-23 | **Nasce a `Contas_Api`:** controllers REST, DTOs e converters de todas as entidades. |
| 16 | `7799e95` | 2026-07-24 | **Autenticação JWT**, autorização por dono do recurso (`404` sem posse) e testes de integração (`WebApplicationFactory`). |
| 17 | `5c4ded8` | 2026-07-27 | Corrige Swagger: OpenAPI fora do fallback de auth + esquema Bearer. |
| 18 | `e4961cc` | 2026-07-27 | Merge PR #3. |
| 19 | `980d916` | 2026-07-29 | **Módulo de investimentos:** CRUD completo de `Carteira`, `Investimento`, `Historico` e `Operacao`. |
| 20 | `fb7f4fd` | 2026-07-29 | Login movido para `AuthController` + endpoint de health check. |
| 21 | `9b9669a` | 2026-07-29 | Merge PR #4. |
| 22 | `006eaeb` | 2026-08-06 | **Nasce `Contas_Contratos`:** DTOs extraídos; clientes deixam de depender do backend. |
| 23 | `8f934cc` | 2026-08-06 | **Nasce `Contas_Web`** (Blazor Server) com login funcional, `AuthSession` e `AuthApiService`; tela de login do app mobile. |
| 24 | `4b3ce04` | 2026-08-06 | `Contas_App` passa a reusar os DTOs de `Contas_Contratos`. |
| 25 | `1e1cf22` | 2026-08-11 | Corrige double-encoding UTF-8 (mojibake) em 11 arquivos. |
| 26 | `23d5885` | 2026-08-11 | Tela **Home/dashboard** e **geração automática de parcelas** ao criar dívida. |
| 27 | `453964e` | 2026-08-11 | Cria `spec/` com a documentação de arquitetura e o histórico de commits. |
| 28 | `3307b09` | 2026-08-12 | Corrige token JWT não enviado no dashboard (remove `DelegatingHandler`) e adiciona lista de contas com saldo editável. |
| 29 | `98107e7` | 2026-08-12 | Documenta `3307b09` em `spec/`. |
| 30 | `dfd103a` | 2026-08-17 | **Execução da `Contas_Api` em Docker** (Dockerfile multi-stage + `docker-compose.yml` + `.env`). |
| 31 | `e3a2ce9` | 2026-08-17 | Documenta `dfd103a` em `spec/`. |
| 32 | `c37e4cb` | 2026-08-17 | **CRUD web das 10 models** em `Contas_Web` (par Lista/Form), `ApiClient` + serviços por entidade, `PaginaAutenticada`, `Shared/`, `Filtro`/`Formato`. |
| 33 | `6e72ad5` | 2026-08-17 | Documenta `c37e4cb` em `spec/`. |
| 34 | `3e99ba7` | 2026-08-19 | **Receita × dívida** (`dm_divida`/`EhDivida`), card "Receitas a receber" no dashboard e **pagar/desfazer parcela** inline na web. |
| 35 | `2a44adc` | 2026-08-19 | Documenta `3e99ba7` em `spec/`. |
| 36 | `d8fc454` | 2026-08-21 | Parcelas ordenadas por vencimento, **filtro por dívida persistido** e **quadro anual** (`/parcelas/anual`). |
| 37 | `25c1637` | 2026-08-21 | Documenta `d8fc454` em `spec/`. |
| 38 | `ce561ef` | 2026-09-21 | **`MainPage` do app mostra e edita saldos:** `ApiConfig`, `AppSession`, `ApiClient`, `ContasApiService`, `ContaSaldoItem`; token guardado no login. |

> Detalhe completo de cada commit em `spec/commit_<hash>.md`.
