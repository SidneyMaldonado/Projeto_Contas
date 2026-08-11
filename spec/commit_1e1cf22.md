# commit 1e1cf22

- **Data:** 2026-08-11
- **Autor:** SmaldonadoMiltec (com Claude Sonnet 5 como co-autor)
- **Mensagem original:** Corrige double-encoding UTF-8 em strings com acentuacao

## Resumo

Correção de um bug de charset relatado pelo usuário: a mensagem de erro de login no Web aparecia como "E-mail ou senha invÃ¡lidos" em vez de "E-mail ou senha inválidos". Diagnóstico: os arquivos tinham o BOM UTF-8 correto, mas o conteúdo tinha sido salvo com "double-encoding" — bytes UTF-8 de caracteres acentuados foram, em algum momento, interpretados como Latin-1/Windows-1252 e regravados como UTF-8 de novo (ex.: "á", 2 bytes em UTF-8, virou "Ã¡", 4 bytes). Corrigido revertendo essa codificação (reinterpretar o texto como Latin-1 e decodificar como UTF-8) nos 11 arquivos afetados, identificados por grep do padrão de mojibake (`Ã` seguido de caractere Latin-1 alto) em todo o repositório.

## Arquivos afetados

- `Contas_Web/Services/AuthApiService.cs` (mensagens de erro de login)
- `Contas_Core/Converters/HistoricoConverter.cs` (comentário)
- `Contas_Test/Api_Tests/{Carteira,Categoria,Conta,Credor,Divida,Historico,Investimento,Operacao,Parcela}ControllerTests.cs` (strings de teste com acentuação)

11 arquivos alterados, 63 inserções, 63 remoções.
