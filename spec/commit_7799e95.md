# commit 7799e95

- **Data:** 2026-07-24
- **Autor:** miltec\smaldonado (com Claude Sonnet 5 como co-autor)
- **Mensagem original:** Adiciona autenticacao JWT, autorizacao por dono do recurso e testes de integracao

## Resumo

Commit estrutural para a segurança da API. Adiciona autenticação via JWT Bearer: login (em `UsuariosController`, antes de ser extraído para `AuthController` no commit `fb7f4fd`) emite um token, e todos os endpoints passam a exigir usuário autenticado por padrão — só registro e login ficam de fora. Cria `Contas_Core/Security/JwtSettings.cs`/`JwtTokenGenerator.cs` e `Contas_Api/Extensions/ClaimsPrincipalExtensions.cs` (`GetUsuarioId()`).

Introduz a checagem de posse do recurso, que é regra até hoje: listagens de `Conta`/`Divida`/`Parcela` são filtradas pelo usuário autenticado, acessar recurso de outro usuário retorna `404`, e o `IdUsuario` do corpo da requisição é sempre ignorado em favor do usuário do token.

Adiciona testes de integração (`WebApplicationFactory` + banco InMemory) para todos os controllers em `Contas_Test/Api_Tests`, incluindo autenticação e isolamento entre usuários — a base do `ApiTestBase`/`SeedAsync` usado por todos os testes de API desde então. Reorganiza `Contas_Test` em subpastas (`Api_Tests`, `Conexao_Tests`, `Repository_Tests`, `UseCase_Tests`).

## Arquivos afetados

- `Contas_Api/Controllers/{Contas,Dividas,Parcelas,Usuarios}Controller.cs` (checagem de posse)
- `Contas_Api/Extensions/ClaimsPrincipalExtensions.cs` (novo)
- `Contas_Api/Program.cs` (configuração JWT/`FallbackPolicy`)
- `Contas_Core/Dto/LoginResponseDto.cs`, `Security/JwtSettings.cs`, `Security/JwtTokenGenerator.cs` (novos)
- `Contas_Db/Model/*.cs` (ajustes de `IdUsuario`/relacionamentos)
- `Contas_Test/Api_Tests/*` (novo: `ApiTestBase`, `AutenticacaoTests`, um `*ControllerTests.cs` por entidade)
- Reorganização de `Contas_Test` em `Conexao_Tests`/`Repository_Tests`/`UseCase_Tests`

43 arquivos alterados, 2015 inserções, 43 remoções.
