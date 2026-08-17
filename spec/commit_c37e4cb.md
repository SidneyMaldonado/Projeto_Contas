# commit c37e4cb

- **Data:** 2026-08-17
- **Autor:** SmaldonadoMiltec (com Claude Opus 5 como co-autor)
- **Mensagem original:** Adiciona CRUD web das 10 models em Contas_Web
- **Branch:** `Executar-no-Docker`

## Resumo

A pedido do usuário: uma tela de consulta por model, com filtro por nome ou descrição, no menu e roteada. Perguntado antes de começar, o escopo foi ampliado para **CRUD completo** (não só listagem), e três decisões foram tomadas pelo usuário: filtrar Operações pelo nome do investimento, resolver as chaves estrangeiras para nomes em vez de mostrar ids, e criar a tela de Usuários normalmente mesmo com a limitação de privacidade da API.

Todos os endpoints usados **já existiam** — `GET/POST/PUT/PATCH/DELETE api/<recurso>` para as 10 entidades. Nenhuma linha de `Contas_Api`, `Contas_Core`, `Contas_Db` ou `Contas_Contratos` foi alterada.

### Estrutura

Uma subpasta por entidade em `Components/Pages/<Entidade>/`, com `Lista<Entidades>.razor` (`/<entidades>`) e `Form<Entidade>.razor` (`/<entidades>/novo` + `/<entidades>/{Id:int}/editar`). São 10 pares: Contas, Categorias, Credores, Usuários, Dívidas, Parcelas, Carteiras, Investimentos, Operações e Históricos. O `NavMenu` agrupa os 10 em Cadastros, Dívidas e Investimentos; `Counter` e `Weather` saíram do menu (os arquivos ficaram).

Toda listagem tem o mesmo formato: busca por texto, alternador "Mostrar inativos" (padrão só ativos), coluna Situação e Editar/Inativar/Excluir por linha, com contador "X de Y registro(s)".

### Camada de acesso à API

`Services/ApiClient.cs` concentra o `HttpClient`, o Bearer token anexado **por requisição** (nunca via `DelegatingHandler` — ver `commit_3307b09.md`) e a tradução da resposta em `ApiResultado(Sucesso, Erro)`. As validações de `Biz` voltam da API como `BadRequest(string)` com a mensagem pronta, que é exibida direto na tela — verificado salvando uma categoria com nome de 2 caracteres e recebendo "Categoria inválida: verifique o nome.". Acima do `ApiClient`, um serviço fino por entidade só mapeia as rotas do recurso.

`Components/PaginaAutenticada.cs` é a base de todas as páginas autenticadas: restaura o token, redireciona para `/login`, chama o `CarregarAsync()` abstrato e expõe `Carregando`/`UsuarioId`/`Mensagem`/`AplicarResultadoAsync`. `OnInitializedAsync` é `sealed` para forçar o padrão. Todas as páginas usam `prerender: false` — obrigatório, porque `ProtectedSessionStorage` não existe durante o prerender.

`Components/Shared/` tem `BarraFiltro`, `AcoesLinha`, `Alerta` e `Situacao`. `AcoesLinha` confirma **inline** ("Inativar?" / "Excluir de vez?" + Sim/Não) em vez de usar `confirm()` do JS, que bloquearia o circuito do Blazor Server. `Filtro.Corresponde` compara sem acento nem caixa (`habitacao` encontra "Habitação"); `Formato` centraliza moeda/data/quantidade em pt-BR.

### Dois bugs encontrados no teste em execução e corrigidos

**1. `FormHistorico` não salvava, sem nenhuma mensagem na tela.** O `EditForm` usava `AdicionarHistoricoDto` como `Model`, mas `NomeInvestimento` é `[Required]` nesse DTO e **não tem input** — é desnormalizado, derivado do investimento escolhido, e era preenchido dentro do `SalvarAsync`. O `DataAnnotationsValidator` barrava o submit antes disso e, sem um `ValidationMessage` para aquele campo, o botão Salvar simplesmente não fazia nada (confirmado: nenhum registro chegava ao banco). Corrigido com um modelo próprio na página (`ModeloHistorico`), montando o DTO no `SalvarAsync` — mesmo padrão que `FormUsuario` já usava por causa da senha.

**2. `[Required]` em `int` não barra o valor 0.** A opção "Selecione..." dos `InputSelect` de FK vale `0` e passava pela validação do `EditForm`, chegando à API. Adicionada checagem explícita `== 0` no `SalvarAsync` de Dívidas, Parcelas, Investimentos e Operações. É a mesma armadilha já anotada na regra 5 da seção 4 do tech-spec, agora do lado do cliente.

### Problema de dados encontrado (não do código)

`GET api/Dividas` respondia 500 e derrubava a tela `/dividas`: `tb_divida.id_conta` e `id_categoria` existem no banco como colunas **nullable**, mas o model `Divida` as declara `[Required] int`, e havia 2 linhas legadas (`Funlec`, `IPTU`) com `NULL` nas duas. Como `ObterTodosDividaUseCase` materializa todas as linhas antes de filtrar por usuário, o EF Core estourava ao ler essas duas. Reportado ao usuário, que corrigiu os dados; a tela passou a funcionar. A divergência model/schema continua e está registrada na seção de observações do tech-spec.

### Verificação

Testado em execução no browser (API em `:5210` + `Contas_Web` em `:5095`), com login real do usuário:

- **Categorias:** CRUD completo — criação com erro de validação da API aparecendo na tela, criação válida, filtro sem acento, edição, Inativar com confirmação inline, "Mostrar inativos" revelando o registro (e o botão Inativar corretamente ausente em registro já inativo), e Excluir físico.
- **Contas:** criação com saldo formatado `R$ 1.500,50`, associada ao usuário logado.
- **Carteiras → Investimentos → Operações → Históricos:** cadeia criada de ponta a ponta; as colunas mostram "Carteira Renda Fixa" / "Tesouro Selic 2029" em vez dos ids; filtro de Operações pelo nome do investimento confirmado (`selic` acha, `selicXX` não).
- **Dívidas → Parcelas:** dívida de 3 parcelas criada, as 3 parcelas geradas automaticamente pela API apareceram com dívida, conta e categoria resolvidas, valor rateado e vencimentos mensais no dia derivado da data.
- **Dashboard (`Home`)** refletiu tudo de forma consistente após os cadastros.
- **Credores e Usuários:** listagem, filtro e ações renderizando.
- Build sem avisos e `dotnet test`: **381 testes, 0 falhas.**

Não foi possível verificar empiricamente a preservação das imagens no `PUT` (o `ApplyUpdate` dos Converters sobrescreve as colunas de imagem, e os formulários devolvem os bytes atuais): não há nenhuma linha com imagem gravada no banco.

## Arquivos afetados

- `Contas_Web/Components/Pages/<10 entidades>/{Lista*,Form*}.razor` (novos — 20 páginas)
- `Contas_Web/Components/PaginaAutenticada.cs` (nova — base das páginas autenticadas)
- `Contas_Web/Components/Shared/{BarraFiltro,AcoesLinha,Alerta,Situacao}.razor` (novos)
- `Contas_Web/Services/ApiClient.cs` (novo — `HttpClient` + Bearer + `ApiResultado`)
- `Contas_Web/Services/{Carteiras,Categorias,Credores,Dividas,Historicos,Investimentos,Operacoes,Parcelas,Usuarios}ApiService.cs` (novos)
- `Contas_Web/Services/ContasApiService.cs` (migrado para `ApiClient`, ganhou o CRUD; `ObterResumoAsync`/`AtualizarSaldosAsync` mantidos para a Home)
- `Contas_Web/{Filtro,Formato}.cs` (novos)
- `Contas_Web/Program.cs` (registra `ApiClient` e os 10 serviços; `Api:BaseUrl` lido uma vez)
- `Contas_Web/Components/_Imports.razor` (+ `Shared`, `Services`, `Contas_Contratos.Dto`)
- `Contas_Web/Components/Layout/NavMenu.razor` + `.razor.css` (10 itens agrupados, estilo `.nav-secao`)
- `spec/tech-spec.md` (seção 3.5 reescrita com o padrão de CRUD e os dois cuidados de formulário; novas observações sobre `tb_divida`, ausência de paginação e sobrescrita de imagem)

43 arquivos alterados, 3154 inserções, 47 remoções.
