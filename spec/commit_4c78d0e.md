# commit 4c78d0e

- **Data:** 2026-07-20
- **Autor:** miltec\smaldonado
- **Mensagem original:** Adiciona resumo de contas e atualizacao em lote de saldos

## Resumo

Cria `ContaResumoDto` (`Codigo`, `Nome`, `Saldo`) e `ObterResumoContaUseCase`, que retorna somente as contas ativas do usuário nesse formato reduzido — sem alterar o `ObterTodosContaUseCase` já existente. Este UseCase é, até hoje, o único endpoint "resumo" pré-existente no projeto, e serviu de referência de padrão para o `GerarParcelasDividaUseCase`/`ObterResumoDashboardUseCase` implementados depois.

Cria `IContaRepository`/`ContaRepository` com `AtualizarSaldosAsync`, para persistir vários saldos de uma vez, e `AtualizarSaldosContaUseCase`, que recebe uma lista de `ContaResumoDto` e repassa os pares (código, saldo) ao repositório — usado por telas que editam saldo de várias contas de uma vez (ex.: conciliação em lote).

Também mergeado no PR #2 (`8b22deb`).

## Arquivos afetados

- `Contas_Core/Dto/ContaResumoDto.cs` (novo — depois movido para `Contas_Contratos` no commit `006eaeb`)
- `Contas_Core/UseCase/Conta/AtualizarSaldosContaUseCase.cs`, `ObterResumoContaUseCase.cs` (novos)
- `Contas_Db/Repository/ContaRepository.cs`, `IContaRepository.cs` (novos)
- `Contas_Test/ContaRepositoryTests.cs`, `ContaUseCaseTests.cs`

7 arquivos alterados, 171 inserções, 2 remoções.
