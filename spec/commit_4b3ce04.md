# commit 4b3ce04

- **Data:** 2026-08-06
- **Autor:** SmaldonadoMiltec (com Claude Sonnet 5 como co-autor)
- **Mensagem original:** Reutiliza DTOs de Contas_Contratos no Contas_App, removendo duplicatas

## Resumo

Limpeza de duplicação criada no commit anterior: `Contas_App` tinha `LoginRequest`, `LoginResponse`, `RegisterRequest` e `UsuarioInfo` em `Services/AuthModels.cs`, que duplicavam exatamente `LoginUsuarioDto`, `LoginResponseDto`, `AdicionarUsuarioDto` e `UsuarioDto` já existentes em `Contas_Contratos` (criado no commit `006eaeb`). Adiciona a referência de `Contas_App` a `Contas_Contratos` (projeto leve, sem dependências — por isso é seguro referenciar de um app MAUI) e troca `AuthApiService` para usar o contrato compartilhado com `Contas_Api`/`Contas_Web`, em vez de tipos locais equivalentes.

## Arquivos afetados

- `Contas_App/Contas_App.csproj` (referência a `Contas_Contratos`)
- `Contas_App/Services/AuthApiService.cs`, `AuthModels.cs`

3 arquivos alterados, 15 inserções, 34 remoções.
