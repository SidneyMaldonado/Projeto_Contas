# commit 3307b09

- **Data:** 2026-08-12
- **Autor:** SmaldonadoMiltec (com Claude Sonnet 5 como co-autor)
- **Mensagem original:** Corrige token JWT nao enviado no dashboard e adiciona lista de contas com saldo editavel

## Resumo

Dois pontos resolvidos na mesma sessão, o primeiro descoberto ao investigar um bug relatado pelo usuário ("Não foi possível carregar o resumo, tente novamente mais tarde" ao logar no Contas_Web, mesmo com login e `GET api/dashboard/resumo` funcionando via Swagger).

**Bug:** o `AuthHeaderHandler` (introduzido no commit `23d5885`), um `DelegatingHandler` registrado via `.AddHttpMessageHandler<T>()` para anexar o Bearer token, injetava `AuthSession` (Scoped por circuito Blazor) no construtor. O `IHttpClientFactory`, porém, resolve a cadeia de handlers a partir de um escopo de DI **interno próprio**, reaproveitado entre circuitos por até `HandlerLifetime` (2 min por padrão) — o handler acabava preso a uma instância de `AuthSession` "congelada" de outro circuito, com `Token = null`, e nunca enviava o header. Resultado: `401` silencioso (sem log, sem exceção visível) e `DashboardApiService.ObterResumoAsync()` retornando `null`. Confirmado com logs de debug temporários comparando o hash de instância de `AuthSession` visto pela página Home (token setado) contra o visto pelo handler (token nulo) — instâncias diferentes.

**Correção:** removido `AuthHeaderHandler`; `DashboardApiService` (e o novo `ContasApiService`) agora injetam `AuthSession` direto no próprio construtor (que É resolvido corretamente por circuito, diferente do pipeline de handlers) e montam `HttpRequestMessage` com o header `Authorization` por chamada, antes de `httpClient.SendAsync(...)`.

**Feature:** a pedido do usuário, `Home.razor` ganhou uma lista das contas do usuário com o saldo de cada uma, com um botão "Atualizar Saldos". Antes de implementar, foi levantado que pagar uma parcela não atualiza `Conta.Saldo` (não existe, em nenhum lugar do código, uma fórmula para derivar saldo a partir de parcelas pagas), mas já existia — pronto e nunca usado por nenhuma tela — o endpoint `PUT api/Contas/saldos` (desde o commit `4c78d0e`), feito para edição/conciliação em lote. Perguntado ao usuário, confirmado que era exatamente esse o comportamento desejado: lista editável, botão salva tudo de uma vez. Implementado sem nenhuma mudança de backend — só o novo `ContasApiService` (`GET api/Contas/resumo` + `PUT api/Contas/saldos`) e a UI em `Home.razor`. Após salvar com sucesso, o resumo do dashboard (`SaldoTotalContas`) é recarregado automaticamente.

Testado ponta a ponta subindo `Contas_Api` + `Contas_Web` localmente e operando via browser (Chrome DevTools MCP): reprodução do bug antes da correção, confirmação do fix, e o fluxo completo da nova lista de contas (estado vazio, edição, salvar, persistência após reload).

## Arquivos afetados

- `Contas_Web/Services/AuthHeaderHandler.cs` (removido)
- `Contas_Web/Services/DashboardApiService.cs` (injeta `AuthSession` direto, monta header por requisição)
- `Contas_Web/Services/ContasApiService.cs` (novo — `ObterResumoAsync`/`AtualizarSaldosAsync`)
- `Contas_Web/Program.cs` (remove registro do handler antigo, registra `ContasApiService`)
- `Contas_Web/Components/Pages/Home.razor` (lista de contas com saldo editável + botão "Atualizar Saldos")
- `spec/tech-spec.md` (documenta o padrão correto de anexar o Bearer token e o novo uso do endpoint de saldos)

6 arquivos alterados, 171 inserções, 57 remoções.
