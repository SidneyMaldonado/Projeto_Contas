# commit 3e99ba7

- **Data:** 2026-08-19
- **Autor:** SmaldonadoMiltec (com Claude Opus 5 como co-autor)
- **Mensagem original:** Distingue receita de divida e permite pagar parcela na web
- **Branch:** `Executar-no-Docker`

## Resumo

Duas frentes que se cruzaram no mesmo commit (as duas mexem em `ListaDividas.razor`, não davam para separar por arquivo):

1. **Dívida × receita** — o mesmo cadastro passa a representar dinheiro a pagar e a receber, com o dashboard somando os dois separadamente.
2. **Pagar parcela pela web** — `PATCH api/parcelas/{id}/pagar` e `/desfazer-pagamento`, que existiam na API mas não tinham caller no `Contas_Web`, ganham botão na listagem.

O trabalho já estava no diretório de trabalho quando a sessão começou; o commit organizou e verificou o que havia.

### 1. `dm_divida`: dívida (a pagar) × receita (a receber)

`tb_divida` ganha a coluna `dm_divida` (`bit NOT NULL`, `DEFAULT ((1))`), via `Contas_Db/Script/adicionar_divida_dm_divida.sql` — script incremental manual, como manda a regra 8 da seção 4 do tech-spec. O `WITH VALUES` no `ALTER TABLE` é o detalhe que importa: sem ele o default não é gravado nas linhas existentes. Com ele, todo o histórico já cadastrado vira dívida, que é a leitura correta.

O campo aparece como `Divida.EhDivida` no model e em `AdicionarDividaDto` / `AtualizarDividaDto` / `DividaDto`, sempre com `= true` como valor padrão. Isso mantém compatibilidade com cliente antigo: um POST sem o campo cai em dívida em vez de virar receita silenciosamente (há teste para exatamente isso). `DividaConverter` propaga o campo nos três sentidos (`ToEntity`, `ApplyUpdate`, `ToDto`).

**A `Parcela` não guarda essa distinção** — e essa é a decisão de modelagem do commit. Quem sabe se o dinheiro entra ou sai é a dívida de origem. Consequência prática: qualquer cálculo que precise separar "a pagar" de "a receber" tem que resolver `Parcela.IdDivida → Divida.EhDivida`. Foi o que `ObterResumoDashboardUseCase` passou a fazer: recebe o `ObterTodosDividaUseCase` no construtor, monta um `HashSet` com os ids das dívidas que são receita, e usa esse conjunto para partir as parcelas não pagas em dois totais — `ValorTotalDividasAbertas` (que antes somava tudo) e o novo `ValorTotalReceitasAbertas` em `DashboardResumoDto`. `Home.razor` mostra o card "Receitas a receber" ao lado dos outros.

Na web, `FormDivida` ganha o select **Tipo** no topo do formulário, e título e textos de ajuda acompanham a escolha ("Nova dívida" / "Nova receita"). `ListaDividas` ganha coluna Tipo com badge (vermelho para dívida, verde para receita) e um filtro Todos / Dívidas / Receitas em `btn-group`, com a mensagem de lista vazia acompanhando o filtro escolhido.

### 2. Pagar e desfazer pagamento na listagem de parcelas

Antes, `FormParcela` dizia em nota de rodapé que pagar e desfazer "continuam só na API, fora do escopo deste formulário". Agora estão na listagem, por linha, no novo `Components/Shared/PagamentoParcela.razor`.

O componente tem três estados: normal (badge "Em aberto"/"Pago em ..." + botão Pagar ou Desfazer), pagando (`<input type="date">` com hoje sugerido + Confirmar/Cancelar) e desfazendo ("Desfazer pagamento?" + Sim/Não). Tudo **inline**, mesmo motivo do `AcoesLinha`: um `confirm()`/`prompt()` do JS travaria o circuito do Blazor Server — e aqui isso vale duplo, porque a operação precisa de um *dado* (a data), não só de um sim/não. Os botões desabilitam durante a chamada (`_executando`) e o componente só comunica via `EventCallback` — quem chama a API é a página, pelo `AplicarResultadoAsync` da `PaginaAutenticada`.

Para o pagar, que manda corpo (`PagarParcelaDto`), o `ApiClient` ganhou a sobrecarga `PatchAsync<TCorpo>(url, corpo)` — só existia o PATCH sem corpo, usado por `Inativar`. `ParcelasApiService` ganhou `PagarAsync` e `DesfazerPagamentoAsync`.

Junto veio a navegação pai→filho por querystring: `ListaParcelas` aceita `?idDivida={id}` (`[SupplyParameterFromQuery]`), mostra um badge com o nome da dívida e um "Remover filtro"; `ListaDividas` linka para lá com "Ver parcelas" por linha; e `FormParcela` recebe o mesmo parâmetro para pré-selecionar a dívida no select e voltar para a lista **ainda filtrada** (Cancelar, Voltar e o pós-salvar todos usam `UrlLista`). O `FormParcela` só aceita a sugestão se a dívida estiver entre as opções do select — as inativas ficam de fora, então um id inativo na URL é ignorado em vez de gerar um select em estado inconsistente.

## Testes

`DividaControllerTests` ganhou dois cenários:

- `Adicionar_DeveCriarReceita_QuandoEhDividaForFalse` — cria com `EhDivida = false`, confere na resposta do POST e relendo pelo GET.
- `Adicionar_DeveTratarComoDivida_QuandoEhDividaForOmitido` — posta um objeto anônimo **sem** o campo (simulando cliente anterior à coluna) e confirma que cai em dívida.

## Verificação

- `dotnet build`: **0 erros, 0 avisos.**
- `dotnet test`: **383 testes, 0 falhas** (eram 381 antes; +2 deste commit).
- **Não houve teste em execução no browser** neste commit — diferente do `commit_c37e4cb`, as telas novas (select Tipo, filtro de tipo, botões de pagamento, `?idDivida=`) foram verificadas só por compilação e pelos testes de API. O `PagamentoParcela` em particular nunca foi exercido de ponta a ponta contra a API real.
- **O script SQL não foi aplicado.** `adicionar_divida_dm_divida.sql` precisa rodar no banco antes de subir a aplicação — sem a coluna `dm_divida`, o EF Core estoura ao ler `tb_divida`, porque o model já a declara `[Required]`.

## Arquivos afetados

- `Contas_Db/Script/adicionar_divida_dm_divida.sql` (novo — `ALTER TABLE ... ADD dm_divida bit NOT NULL DEFAULT 1 WITH VALUES`)
- `Contas_Db/Model/Divida.cs` (+ `EhDivida`)
- `Contas_Contratos/Dto/{AdicionarDivida,AtualizarDivida,Divida}Dto.cs` (+ `EhDivida`, default `true`)
- `Contas_Contratos/Dto/DashboardResumoDto.cs` (+ `ValorTotalReceitasAbertas`)
- `Contas_Core/Converters/DividaConverter.cs` (propaga `EhDivida` nos três métodos)
- `Contas_Core/UseCase/Dashboard/ObterResumoDashboardUseCase.cs` (recebe `ObterTodosDividaUseCase`; parte as parcelas em dívidas × receitas)
- `Contas_Web/Components/Shared/PagamentoParcela.razor` (novo — estado do pagamento + Pagar/Desfazer inline)
- `Contas_Web/Components/Pages/Dividas/FormDivida.razor` (select Tipo; título e textos por tipo)
- `Contas_Web/Components/Pages/Dividas/ListaDividas.razor` (coluna Tipo, filtro Todos/Dívidas/Receitas, link "Ver parcelas")
- `Contas_Web/Components/Pages/Parcelas/ListaParcelas.razor` (`PagamentoParcela`, `?idDivida=`, `PagarAsync`/`DesfazerPagamentoAsync`)
- `Contas_Web/Components/Pages/Parcelas/FormParcela.razor` (`?idDivida=` para pré-selecionar e voltar filtrado)
- `Contas_Web/Components/Pages/Home.razor` (card "Receitas a receber")
- `Contas_Web/Services/ApiClient.cs` (+ `PatchAsync<TCorpo>`)
- `Contas_Web/Services/ParcelasApiService.cs` (+ `PagarAsync`, `DesfazerPagamentoAsync`)
- `Contas_Test/Api_Tests/DividaControllerTests.cs` (+2 cenários)

17 arquivos alterados, 345 inserções, 22 remoções.

## Reflexo no tech-spec

- Seção 1 — dívida e receita são o mesmo cadastro; a `Parcela` não carrega a distinção, quem carrega é a dívida de origem.
- Seção 3.5 — `PagamentoParcela` entra na lista de `Shared/`, com o motivo de ser inline; novo item sobre filtro por querystring como padrão de navegação pai→filho entre listagens.
- Seção 7 — nova observação: `ObterResumoDashboardUseCase` varre `tb_divida` inteira (sem filtro por usuário) a cada acesso à Home só para classificar as parcelas.
