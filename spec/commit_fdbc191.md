# commit fdbc191

- **Data:** 2026-07-14
- **Autor:** miltec\smaldonado
- **Mensagem original:** Ajustes simples

## Resumo

Faxina na organização dos scripts SQL: move o schema para `Contas_Db/Script/create.sql` (renomeando a partir de `Contas_Db/md/script.sql`) e remove os arquivos redundantes que tinham se acumulado (`md/criar_model.md`, `md/script_create.sql`), consolidando em um único script de referência. Ajusta o `Contas_Db.csproj`.

## Arquivos afetados

- `Contas_Db/Script/create.sql` (renomeado a partir de `md/script.sql`)
- `Contas_Db/md/criar_model.md` (removido)
- `Contas_Db/md/script_create.sql` (removido)
- `Contas_Db/Contas_Db.csproj`

4 arquivos alterados, 16 inserções, 286 remoções.
