# commit d8fc454

- **Data:** 2026-08-21
- **Autor:** SmaldonadoMiltec (com Claude Opus 5 como co-autor)
- **Mensagem original:** Ordena parcelas por vencimento, filtra por divida e cria quadro anual
- **Branch:** `Executar-no-Docker`

## Resumo

Commit só de `Contas_Web` — nada de banco, API, `Contas_Core` ou DTOs. Três pedidos encadeados sobre a mesma área (parcelas), na ordem em que foram feitos:

1. **Ordenação padrão** da listagem de parcelas por vencimento crescente.
2. **Filtro por dívida em dropdown**, aplicado por botão e **persistido** até ser limpo.
3. **Quadro anual** novo: dívidas/receitas × 12 meses do ano, com subtotais e resultado.

## 1. Ordenação por vencimento

`ListaParcelas` já ordenava por `DataVencimento`, mas **descendente** — o vencimento mais distante no topo. Virou `OrderBy` crescente: o que está para vencer (e o que está atrasado) aparece primeiro, que é a leitura útil da tela. Uma palavra de diferença no código, mudança grande de comportamento.

## 2. Filtro por dívida persistido

A dropdown lista todas as dívidas ordenadas por nome (**inclusive as inativas** — parcelas de dívida inativa continuam na lista, e sem elas no select um filtro salvo ficaria inalcançável), com "Todas as dívidas" como primeira opção. O `@bind` é um `int?`: o `BindConverter` do Blazor trata string vazia como `null` para tipo nulável, então a opção "todas" cai em `null` sem parse manual.

São **dois** campos, e essa separação é o ponto do requisito "filtre quando clicar em Filtrar":

- `_idDividaSelecionada` — o que está escolhido na dropdown, ainda sem efeito.
- `_idDividaAplicada` — o filtro em vigor, o único que `Filtradas` e `UrlNovo` consultam.

O clique em "Filtrar" copia um no outro; "Limpar filtro" zera os dois. O botão Limpar fica desabilitado quando não há nada selecionado nem aplicado.

**Persistência:** `AplicarFiltroAsync` grava o id no `ProtectedSessionStorage` (chave `parcelas-filtro-divida`) e o apaga quando o filtro é limpo. O mesmo mecanismo que a `AuthSession` já usava — o que resolve o problema de fundo aqui: **cada navegação do Blazor Server cria um circuito novo**, então campo de componente não sobrevive a sair e voltar da página. Com o storage, sobrevive também ao F5, até ser limpo. A restauração roda em `RestaurarFiltroAsync`, chamada no início do `CarregarAsync` e protegida por `_filtroRestaurado` — importante porque `CarregarAsync` também roda depois de cada escrita (`AplicarResultadoAsync`), e sem a guarda cada pagamento de parcela releria o storage e desfaria um filtro recém-trocado.

**Precedência:** `?idDivida=` na querystring ganha do valor salvo **e passa a ser o valor salvo**. É o que mantém o link "Ver parcelas" da lista de dívidas funcionando como antes, agora refletido na dropdown. Consequência: o badge "Dívida: X" + link "Remover filtro" foi removido — a dropdown mostra a dívida escolhida e o botão Limpar faz o que o link fazia. `UrlNovo` também deixou de olhar a querystring e passou a olhar `_idDividaAplicada`, senão o botão "Novo" perderia a dívida quando o filtro viesse do storage em vez da URL.

**Filtro órfão:** depois de carregar as dívidas, se `_idDividaAplicada` não estiver entre elas (dívida excluída depois do filtro salvo), o filtro volta para "todas" em vez de exibir uma lista vazia sem explicação. A checagem só roda se a lista de dívidas veio com algo — assim uma falha no GET de dívidas não apaga um filtro válido.

## 3. `/parcelas/anual` — quadro anual

Página nova (`Components/Pages/Parcelas/QuadroAnualParcelas.razor`, menu "Quadro anual" na seção Dívidas), 14 colunas: nome, Jan…Dez, Total.

A pergunta de modelagem foi **o que é uma linha**. O pedido dizia "linhas das parcelas", mas cada parcela vence num único mês (`DataPrimeiroVencimento.AddMonths(numero - 1)`, em `GerarParcelasDividaUseCase`), então linha-por-parcela daria uma matriz quase toda vazia, com uma célula preenchida por linha. Optou-se (confirmado com o usuário antes de codar) por **uma linha por dívida/receita**, com os valores das parcelas somados na coluna do mês de vencimento — parcelas da mesma dívida no mesmo mês somam na mesma célula.

Ordem do corpo, como pedido: seção Dívidas → `Subtotal dívidas` → seção Receitas → `Subtotal receitas` → `Resultado (receitas - dívidas)`, negativo em vermelho, célula zerada como `—`.

Detalhes que valem para qualquer relatório novo:

- **A classificação vem da dívida, não da parcela.** `ParcelaDto` não tem `EhDivida` — é a mesma consequência de modelagem registrada em `commit_3e99ba7`. O agrupamento resolve `IdDivida` contra o `Dictionary` de dívidas; parcela cujo `IdDivida` não existe mais entra em **Dívidas** com o rótulo `#id` (mesmo fallback das outras telas), porque tratá-la como receita inflaria o resultado.
- **Pagas e em aberto entram juntas** (pedido explícito). **Inativas ficam de fora**, coerente com o padrão das listagens; está escrito na legenda abaixo do título.
- Ano fixo em `DateTime.Today.Year`, sem seletor.
- O array de 12 posições (`decimal[]`, índice 0 = janeiro) é a estrutura de tudo: `SomarPorMes` monta a linha, `Somar` acumula as linhas num subtotal e `Resultado` subtrai os dois subtotais. Subtotal e resultado são recalculados no `@code` durante o render, não em campo.
- Cada linha tem **"Ver parcelas"** → `/parcelas?idDivida={id}`, reaproveitando a precedência da querystring do item 2 (o filtro chega aplicado e persistido). Por isso a `Linha` guarda `IdDivida` além do nome: o botão funciona até nas linhas `#id`, já que o filtro é por id e não depende da dívida existir.
- `QuadroAnualParcelas.razor.css` (scoped) só impede quebra de linha nas células; a rolagem horizontal fica no `table-responsive`.

`Formato` ganhou dois métodos: `Valor` (`N2`, sem `R$` — 14 colunas com símbolo de moeda não caberiam) e `Mes(int)` (abreviação pt-BR com inicial maiúscula: Jan, Fev, Mar…).

## Verificação

- `dotnet build Contas_Web`: **0 erros, 0 avisos.**
- **Não houve build da solução nem `dotnet test`.** A `Contas_Api` estava rodando na máquina (PID 20148) e mantinha `Contas_Db.dll`/`Contas_Core.dll` travadas, o que faz o build da solução falhar com `MSB3027`/`MSB3021` (lock de arquivo, não erro de código). O processo do usuário não foi encerrado. O commit não toca em nada coberto por teste — `Contas_Test` não referencia `Contas_Web` —, então nenhum teste ficou desatualizado.
- **Nenhuma tela foi exercida no browser.** Ordenação, persistência do filtro no `ProtectedSessionStorage` e os números do quadro anual (subtotais, resultado, meses) foram verificados só por compilação e leitura. Subir a tela exige API + SQL Server com o `.env` do compose.

## Arquivos afetados

- `Contas_Web/Components/Pages/Parcelas/ListaParcelas.razor` (ordenação crescente; dropdown + Filtrar/Limpar; persistência no `ProtectedSessionStorage`; remoção do badge/"Remover filtro")
- `Contas_Web/Components/Pages/Parcelas/QuadroAnualParcelas.razor` (novo — `/parcelas/anual`)
- `Contas_Web/Components/Pages/Parcelas/QuadroAnualParcelas.razor.css` (novo — `white-space: nowrap` nas células)
- `Contas_Web/Components/Layout/NavMenu.razor` (+ link "Quadro anual")
- `Contas_Web/Formato.cs` (+ `Valor`, + `Mes`)

5 arquivos alterados, 312 inserções, 14 remoções.

## Reflexo no tech-spec

- Seção 3.5 — o filtro por querystring de `ListaParcelas` agora é dropdown + persistência em `ProtectedSessionStorage`, com a querystring tendo precedência; nova página `/parcelas/anual` fora do par Lista/Form; `Formato` ganhou `Valor` e `Mes`.
- Seção 7 — o quadro anual carrega todas as parcelas e dívidas e agrega em memória, como o resto das listagens.
