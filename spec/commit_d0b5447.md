# commit d0b5447

- **Data:** 2026-07-20
- **Autor:** miltec\smaldonado
- **Mensagem original:** Renomeia projeto Contas_Biz para Contas_Core

## Resumo

Puro rename: `Contas_Biz` passa a se chamar `Contas_Core` (pasta, `.csproj`, namespaces de todos os UseCases e do `PasswordHasher`, e as referências em `Contas_Test.csproj` e `Projeto_Contas.slnx`). Nenhuma mudança de comportamento — só ajuste de nome que se mantém até hoje. Faz parte da branch `rename-contas-biz-to-core`, mergeada no commit `c30c6bb` (PR #1).

## Arquivos afetados

- `Contas_Biz/Contas_Biz.csproj` → `Contas_Core/Contas_Core.csproj`
- `{Contas_Biz => Contas_Core}/Security/PasswordHasher.cs`
- `{Contas_Biz => Contas_Core}/UseCase/**/*.cs` (todos os UseCases, só troca de namespace)
- `Contas_Test/*UseCaseTests.cs` (usings atualizados)
- `Projeto_Contas.slnx`

50 arquivos alterados, 53 inserções, 53 remoções.
