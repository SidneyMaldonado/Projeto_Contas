# commit 956090b

- **Data:** 2026-07-23
- **Autor:** miltec\smaldonado
- **Mensagem original:** Adiciona controllers REST, DTOs e converters para todas as entidades

## Resumo

Cria o projeto `Contas_Api` (ASP.NET Core Web API): um controller por entidade (`CategoriasController`, `ContasController`, `CredoresController`, `DividasController`, `ParcelasController`, `UsuariosController`), cada um chamando os UseCases já existentes em `Contas_Core`. Introduz os DTOs de entrada/saída (`AdicionarXDto`, `AtualizarXDto`, `XDto`, mais casos especiais como `LoginUsuarioDto`, `PagarParcelaDto`, `AlterarSenhaUsuarioDto`) em `Contas_Core/Dto`, e os `XConverter` estáticos (`ToEntity`/`ApplyUpdate`/`ToDto`) em `Contas_Core/Converters` — o padrão Model↔DTO que se mantém até hoje.

Registra `DbContext`, repositórios e UseCases no `Program.cs` (o padrão de DI inline que o projeto usa desde então) e documenta a receita em `Contas_Core/md/criar_controller.md`. Fixa a versão do pacote `Microsoft.OpenApi` em 2.7.5 para corrigir uma falha de build do gerador de OpenAPI, mantendo a correção da vulnerabilidade NU1903.

Ainda **sem autenticação** neste ponto — isso vem no commit `7799e95`, um dia depois.

## Arquivos afetados

- `Contas_Api/*` (novo projeto: csproj, Program.cs, Controllers/*, appsettings, launchSettings, .http)
- `Contas_Core/Converters/*Converter.cs` (novos: Categoria, Conta, Credor, Divida, Parcela, Usuario)
- `Contas_Core/Dto/*.cs` (novos: ~24 DTOs)
- `Contas_Core/md/criar_controller.md` (novo)
- `Projeto_Contas.slnx`

41 arquivos alterados, 1462 inserções.
