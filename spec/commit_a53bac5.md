# commit a53bac5

- **Data:** 2026-07-23
- **Autor:** miltec\smaldonado
- **Mensagem original:** Reorganiza interfaces de Model e Repository em subpastas Interface

## Resumo

Refatoração de organização, sem mudança de comportamento: move `ISoftDelete` para `Contas_Db/Model/Interface/` e as interfaces de repositório (`IRepository`, `IContaRepository`, `IParcelaRepository`, `IUsuarioRepository`) para `Contas_Db/Repository/Interface/`, separando contrato de implementação dentro de `Contas_Db`. Ajusta os `using` em todo `Contas_Core` (Biz, UseCases), nos repositórios concretos e nos testes afetados. Essa é a estrutura de pastas (`Model/Interface`, `Repository/Interface`) que o projeto mantém até hoje.

## Arquivos afetados

- `Contas_Db/Model/{ => Interface}/ISoftDelete.cs`
- `Contas_Db/Repository/{ => Interface}/{IContaRepository,IParcelaRepository,IRepository,IUsuarioRepository}.cs`
- `Contas_Db/Model/*.cs`, `Contas_Db/Repository/*.cs` (usings atualizados)
- `Contas_Core/Biz/AdicionarUsuarioBiz.cs`, `Contas_Core/UseCase/**/*.cs` (usings atualizados)
- `Contas_Test/*UseCaseTests.cs` (usings atualizados)

64 arquivos alterados, 66 inserções, 49 remoções.
