# commit 006eaeb

- **Data:** 2026-08-06
- **Autor:** SmaldonadoMiltec (com Claude Sonnet 5 como co-autor)
- **Mensagem original:** Extrai DTOs para Contas_Contratos, removendo dependencia do Web ao backend

## Resumo

Commit arquitetural importante: até este ponto, um futuro `Contas_Web` só precisaria de 3 DTOs (`LoginUsuarioDto`, `LoginResponseDto`, `UsuarioDto`), mas a única forma de obtê-los seria referenciar o projeto `Contas_Core` inteiro — o que arrastaria transitivamente EF Core, SqlServer e o pacote JWT (dependências de `Contas_Db`/`Contas_Core`) para o build de qualquer cliente web. Este commit move **todos os 35 DTOs** (nenhum deles dependia de EF Core) para um novo projeto `Contas_Contratos`, sem nenhuma dependência própria, que passa a ser o contrato compartilhado entre `Contas_Api` e os clientes.

É a decisão que abre caminho para o commit seguinte (`8f934cc`, implementação do Blazor): sem essa extração, `Contas_Web` teria que referenciar `Contas_Core`/`Contas_Db` só para usar 3 tipos.

## Arquivos afetados

- `Contas_Contratos/Contas_Contratos.csproj` (novo projeto)
- `{Contas_Core => Contas_Contratos}/Dto/*.cs` (35 DTOs movidos)
- `Contas_Api/Controllers/*.cs` (usings atualizados)
- `Contas_Core/Converters/*.cs`, `UseCase/Conta/{AtualizarSaldosContaUseCase,ObterResumoContaUseCase}.cs` (usings atualizados)
- `Contas_Test/Api_Tests/*.cs`, `UseCase_Tests/ContaUseCaseTests.cs` (usings atualizados)
- `Projeto_Contas.slnx`

73 arquivos alterados, 188 inserções, 176 remoções.
