# commit 5c4ded8

- **Data:** 2026-07-27
- **Autor:** miltec\smaldonado (com Claude Sonnet 5 como co-autor)
- **Mensagem original:** Corrige Swagger: libera OpenAPI do fallback de auth e adiciona esquema Bearer

## Resumo

Correção pós-autenticação: como o `FallbackPolicy` do commit anterior passou a exigir usuário autenticado em qualquer endpoint por padrão, isso também bloqueava `/openapi/v1.json` com `401`, e o Swagger UI não tinha botão "Authorize" por faltar um esquema de segurança JWT declarado no documento OpenAPI. Corrige com três ajustes em `Program.cs`: `AllowAnonymous()` no endpoint do OpenAPI, um document transformer que registra o esquema Bearer (http/bearer/JWT), e um operation transformer que aplica o requisito Bearer em todo endpoint sem `[AllowAnonymous]` — é exatamente esse mecanismo que está em produção hoje.

Também mergeado no PR #3 (`e4961cc`).

## Arquivos afetados

- `Contas_Api/Program.cs`

1 arquivo alterado, 38 inserções, 2 remoções.
