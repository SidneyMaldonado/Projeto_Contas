# commit 8f934cc

- **Data:** 2026-08-06
- **Autor:** SmaldonadoMiltec
- **Mensagem original:** Implementação do Blazor

## Resumo

Cria o cliente web (`Contas_Web`, Blazor Server) e a tela de login do app mobile. É o maior commit do histórico em número de linhas (61 mil+), mas a enorme maioria é código de terceiros: os arquivos estáticos do Bootstrap (`Contas_Web/wwwroot/lib/bootstrap/dist/**`, CSS/JS minificados e não-minificados, gerados pelo template padrão do Blazor) — o código de aplicação em si é pequeno.

Em `Contas_Web`: scaffolding padrão do template Blazor Server (`App.razor`, `MainLayout`, `NavMenu`, `ReconnectModal`, páginas de template `Counter`/`Weather`/`Error`/`NotFound`), mais o que já era domínio do projeto: `Login.razor` (tela de login funcional), `Home.razor` (placeholder "Hello, world!" na época, com o guard de autenticação já presente), `Services/AuthApiService.cs` (chama `POST /api/auth/login`) e `Services/AuthSession.cs` (guarda token/usuário via `ProtectedSessionStorage`).

Em `Contas_App`: adiciona `Pages/LoginPage.xaml`/`.xaml.cs` e `Pages/RegisterPage.xaml`/`.xaml.cs` (funcionais, com suporte a biometria), `Services/AuthApiService.cs` (versão própria, ainda não compartilhada com o Web), `Services/AuthModels.cs` e `Services/CredentialStore.cs` (guarda e-mail/senha via `SecureStorage`/`Preferences` para login biométrico — o token JWT em si não é persistido, uma lacuna que continua até hoje).

## Arquivos afetados

- `Contas_App/Pages/{Login,Register}Page.xaml{,.cs}`, `Services/{AuthApiService,AuthModels,CredentialStore}.cs` (novos)
- `Contas_App/AppShell.xaml`, `MauiProgram.cs`, `Contas_App.csproj` (rotas e registro dos novos serviços)
- `Contas_Web/*` (novo projeto Blazor Server completo: Program.cs, Components/**, Services/{AuthApiService,AuthSession}.cs, appsettings, wwwroot)
- `Contas_Web/wwwroot/lib/bootstrap/**` (assets de terceiros, maior parte das linhas do commit)

85 arquivos alterados, 61324 inserções, 3 remoções.
