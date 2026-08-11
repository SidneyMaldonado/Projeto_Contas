# commit 980d916

- **Data:** 2026-07-29
- **Autor:** miltec\smaldonado
- **Mensagem original:** Adiciona entidades Carteira, Investimento, Historico e Operacao (CRUD completo)

## Resumo

Maior commit de feature do projeto até então: implementa a stack completa (Model → Repository/Interface → UseCases de CRUD → Biz de validação → DTOs → Converters → Controllers REST → testes de UseCase/Repository/integração) para as 4 novas tabelas de investimentos: `tb_carteira`, `tb_investimento`, `tb_historico`, `tb_operacao`.

Estabelece a autorização por dono do recurso em cascata para entidades sem `IdUsuario` direto, o padrão ainda em uso: `Carteira` é dona direta (tem `IdUsuario`), `Investimento` verifica posse via sua `Carteira`, e `Historico`/`Operacao` verificam em dois níveis (`Investimento` → `Carteira` → `Usuario`).

Corrige de passagem dois bugs no script `Contas_Db/Script/adicionar.sql` (novo neste commit): um typo na chave primária de `tb_operacao` e uma foreign key que apontava para a própria PK em vez de `id_investimento`.

## Arquivos afetados

- `Contas_Api/Controllers/{Carteiras,Historicos,Investimentos,Operacoes}Controller.cs` (novos)
- `Contas_Core/Biz/Adicionar{Carteira,Historico,Investimento,Operacao}Biz.cs` (novos)
- `Contas_Core/Converters/{Carteira,Historico,Investimento,Operacao}Converter.cs` (novos)
- `Contas_Core/Dto/{Adicionar,Atualizar}{Carteira,Historico,Investimento,Operacao}Dto.cs`, `{Carteira,Historico,Investimento,Operacao}Dto.cs` (novos)
- `Contas_Core/UseCase/{Carteira,Historico,Investimento,Operacao}/*UseCase.cs` (CRUD completo, novos)
- `Contas_Db/Model/{Carteira,Historico,Investimento,Operacao}.cs`, `ContasDbContext.cs` (novos DbSets)
- `Contas_Db/Repository/{Carteira,Historico,Investimento,Operacao}Repository.cs` + interfaces (novos)
- `Contas_Db/Script/adicionar.sql` (novo, com correção de PK/FK de `tb_operacao`)
- `Contas_Test/Api_Tests/{Carteira,Historico,Investimento,Operacao}ControllerTests.cs`, `Repository_Tests/*`, `UseCase_Tests/*` (novos)

75 arquivos alterados, 4226 inserções.
