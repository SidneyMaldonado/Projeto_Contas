using Contas_Web.Services;
using Microsoft.AspNetCore.Components;

namespace Contas_Web.Components;

/// <summary>
/// Base das páginas que exigem login. Cada navegação do Blazor Server cria um circuito
/// novo, então o token precisa ser restaurado do ProtectedSessionStorage antes de
/// qualquer chamada à API — por isso todas as páginas herdeiras usam
/// <c>prerender: false</c> (o storage não existe durante o prerender).
/// </summary>
public abstract class PaginaAutenticada : ComponentBase
{
    [Inject] protected AuthSession AuthSession { get; set; } = default!;

    [Inject] protected NavigationManager Navegacao { get; set; } = default!;

    protected bool Carregando { get; private set; } = true;

    protected string? Mensagem { get; private set; }

    protected bool MensagemSucesso { get; private set; }

    protected sealed override async Task OnInitializedAsync()
    {
        await AuthSession.RestoreAsync();

        if (!AuthSession.IsAuthenticated)
        {
            Navegacao.NavigateTo("/login");
            return;
        }

        await CarregarAsync();
        Carregando = false;
    }

    /// <summary>Carrega os dados da página. Também é chamado após cada operação de escrita.</summary>
    protected abstract Task CarregarAsync();

    /// <summary>Id do usuário autenticado, exigido pelos DTOs de Conta e Dívida.</summary>
    protected int UsuarioId => AuthSession.Usuario?.Id ?? 0;

    protected void LimparMensagem() => Mensagem = null;

    protected void DefinirErro(string? erro)
    {
        MensagemSucesso = false;
        Mensagem = erro ?? "Não foi possível concluir a operação.";
    }

    /// <summary>Aplica o resultado de uma escrita: mostra a mensagem e recarrega em caso de sucesso.</summary>
    protected async Task AplicarResultadoAsync(ApiResultado resultado, string mensagemSucesso)
    {
        MensagemSucesso = resultado.Sucesso;
        Mensagem = resultado.Sucesso ? mensagemSucesso : resultado.Erro;

        if (resultado.Sucesso)
            await CarregarAsync();
    }
}
