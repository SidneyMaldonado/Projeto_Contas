# commit fb7f4fd

- **Data:** 2026-07-29
- **Autor:** miltec\smaldonado
- **Mensagem original:** Move login para AuthController e adiciona endpoint de health check

## Resumo

Extrai o endpoint de login de `UsuariosController` para um novo `AuthController` (`POST /api/auth/login`), removendo o parâmetro `JwtTokenGenerator` que ficava sem uso em `UsuariosController` depois da extração. Adiciona `GET /api/auth/health` como verificação simples de disponibilidade da API (usado, por exemplo, pelo `Contas_App` para checar conectividade). Atualiza os testes de integração para a nova rota de login.

## Arquivos afetados

- `Contas_Api/Controllers/AuthController.cs` (novo)
- `Contas_Api/Controllers/UsuariosController.cs` (remove login)
- `Contas_Test/Api_Tests/AutenticacaoTests.cs`, `UsuarioControllerTests.cs`

4 arquivos alterados, 44 inserções, 24 remoções.
