using Contas_Core.Security;
using Contas_Db.Repository;

namespace Contas_Core.UseCase.Usuario;

public class LoginUsuarioUseCase
{
    private readonly IUsuarioRepository _repository;

    public LoginUsuarioUseCase(IUsuarioRepository repository)
    {
        _repository = repository;
    }

    public async Task<Contas_Db.Model.Usuario?> ExecuteAsync(string email, string senha)
    {
        var usuario = await _repository.ObterPorEmailAsync(email);
        if (usuario is null || !PasswordHasher.Verify(senha, usuario.Senha))
            return null;

        return usuario;
    }
}
