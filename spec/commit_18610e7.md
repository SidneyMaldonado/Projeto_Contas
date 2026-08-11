# commit 18610e7

- **Data:** 2026-07-20
- **Autor:** miltec\smaldonado
- **Mensagem original:** Adiciona CHECK constraint ausente para tb_conta.nr_saldo

## Resumo

Correção pontual no script `Contas_Db/Script/create.sql`: o script referenciava o nome `CK_tb_conta_nr_saldo` (usado, por exemplo, em `DROP CONSTRAINT`/comentários), mas faltava o `ALTER TABLE ... ADD CONSTRAINT` que efetivamente criava esse CHECK constraint, o que fazia o script falhar ao ser executado do zero em um banco novo. Também é parte da branch mergeada em `c30c6bb` (PR #1), junto com o rename para `Contas_Core`.

## Arquivos afetados

- `Contas_Db/Script/create.sql`

1 arquivo alterado, 30 inserções, 6 remoções.
