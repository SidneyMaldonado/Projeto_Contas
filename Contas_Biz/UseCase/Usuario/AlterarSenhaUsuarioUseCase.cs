using Contas_Biz.Security;
using Contas_Db.Repository;

namespace Contas_Biz.UseCase.Usuario;

public class AlterarSenhaUsuarioUseCase
{
    private readonly IUsuarioRepository _repository;

    public AlterarSenhaUsuarioUseCase(IUsuarioRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> ExecuteAsync(int id, string senhaAtual, string novaSenha)
    {
        var usuario = await _repository.GetByIdAsync(id);
        if (usuario is null || !PasswordHasher.Verify(senhaAtual, usuario.Senha))
            return false;

        usuario.Senha = PasswordHasher.Hash(novaSenha);
        await _repository.UpdateAsync(usuario);
        return true;
    }
}
