# commit c6b64ff

- **Data:** 2026-07-08
- **Autor:** miltec\smaldonado
- **Mensagem original:** Adicionar arquivos de projeto.

## Resumo

Cria a estrutura inicial da solução. Adiciona o projeto `Contas_App` (.NET MAUI) já com os arquivos padrão de template (App.xaml, AppShell, MainPage vazia, plataformas Android/iOS/MacCatalyst/Windows, recursos de ícone/splash/fontes). Cria também dois projetos ainda vazios, só com o `Class1.cs` de template: `Contas_Biz` (que mais tarde seria renomeado para `Contas_Core`) e `Contas_Db` (que já nasce com um rascunho de schema em `Contas_Db/md/script_create.sql` e notas de modelagem em `criar_model.md`). Adiciona o arquivo de solução `Projeto_Contas.slnx`.

Neste ponto o projeto é só o esqueleto do app mobile mais dois class libraries vazios — nenhuma regra de negócio, API ou banco real ainda.

## Arquivos afetados

- `Contas_App/*` (projeto MAUI completo de template: App.xaml, AppShell, MainPage, MauiProgram, plataformas, recursos)
- `Contas_Biz/Class1.cs`, `Contas_Biz/Contas_Biz.csproj` (novo projeto, vazio)
- `Contas_Db/Class1.cs`, `Contas_Db/Contas_Db.csproj` (novo projeto)
- `Contas_Db/md/criar_model.md`, `Contas_Db/md/script_create.sql` (rascunho inicial de schema)
- `Projeto_Contas.slnx` (novo)

41 arquivos alterados, 1226 inserções.
