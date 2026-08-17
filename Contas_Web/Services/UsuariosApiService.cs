using Contas_Contratos.Dto;

namespace Contas_Web.Services;

public class UsuariosApiService(ApiClient api)
{
    private const string Recurso = "api/usuarios";

    /// <remarks>
    /// A API não filtra por dono nem tem papel de administrador: este endpoint devolve
    /// todos os usuários do sistema. Registrado como ponto de atenção no tech-spec.
    /// </remarks>
    public Task<IEnumerable<UsuarioDto>?> ObterTodosAsync() =>
        api.ObterAsync<IEnumerable<UsuarioDto>>(Recurso);

    public Task<UsuarioDto?> ObterPorIdAsync(int id) =>
        api.ObterAsync<UsuarioDto>($"{Recurso}/{id}");

    public Task<ApiResultado> AdicionarAsync(AdicionarUsuarioDto dto) =>
        api.AdicionarAsync(Recurso, dto);

    /// <summary>Não altera a senha — a API expõe isso em PATCH api/usuarios/{id}/senha.</summary>
    public Task<ApiResultado> AtualizarAsync(int id, AtualizarUsuarioDto dto) =>
        api.AtualizarAsync($"{Recurso}/{id}", dto);

    public Task<ApiResultado> InativarAsync(int id) =>
        api.PatchAsync($"{Recurso}/{id}/inativar");

    public Task<ApiResultado> ExcluirAsync(int id) =>
        api.ExcluirAsync($"{Recurso}/{id}");
}
