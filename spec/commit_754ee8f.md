# commit 754ee8f

- **Data:** 2026-07-20
- **Autor:** miltec\smaldonado
- **Mensagem original:** Adiciona camada de regras de negocio (Biz) para criacao de entidades

## Resumo

Introduz a camada `Biz` em `Contas_Core/Biz`, seguindo o padrão documentado em `Contas_Core/md/criar_biz.md` (adicionado neste mesmo commit): uma classe `AdicionarXBiz` por entidade, com métodos booleanos pequenos combinados num `IsValid`. Os `AdicionarXUseCase` de `Categoria`, `Conta`, `Credor`, `Divida`, `Parcela` e `Usuario` passam a chamar o `Biz` correspondente antes de persistir, lançando `ArgumentException` quando a entidade é inválida — esse é o padrão que o `Contas_Api` traduz depois em `400 BadRequest`.

Regras implementadas: nome com mínimo de 3 caracteres, e-mail com formato válido, senha forte, e-mail duplicado (nova checagem via `IUsuarioRepository.EmailExisteAsync`), valor/saldo não negativo, número de parcelas ≥ 1, data de vencimento não pode ser passada, e consistência entre dia de vencimento e data do primeiro vencimento (regra que ainda está em vigor em `AdicionarDividaBiz.DiaVencimentoConsistente`).

Também mergeado no PR #2 (`8b22deb`).

## Arquivos afetados

- `Contas_Core/Biz/Adicionar{Categoria,Conta,Credor,Divida,Parcela,Usuario}Biz.cs` (novos)
- `Contas_Core/UseCase/{Categoria,Conta,Credor,Divida,Parcela,Usuario}/Adicionar*UseCase.cs` (chamam o Biz)
- `Contas_Core/md/criar_biz.md` (novo, documenta o padrão)
- `Contas_Db/Repository/IUsuarioRepository.cs`, `UsuarioRepository.cs` (`EmailExisteAsync`)
- `Contas_Test/{Categoria,Credor,Divida,Parcela,Usuario}UseCaseTests.cs` (cenários de validação)

20 arquivos alterados, 375 inserções, 24 remoções.
