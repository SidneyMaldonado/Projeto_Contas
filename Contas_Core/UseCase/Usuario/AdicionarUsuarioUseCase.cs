using Contas_Core.Biz;
using Contas_Core.Security;
using Contas_Db.Repository;

namespace Contas_Core.UseCase.Usuario;

public class AdicionarUsuarioUseCase
{
    private readonly IUsuarioRepository _repository;

    public AdicionarUsuarioUseCase(IUsuarioRepository repository)
    {
        _repository = repository;
    }

    public async Task ExecuteAsync(Contas_Db.Model.Usuario entity)
    {
        if (!await new AdicionarUsuarioBiz(_repository).IsValidAsync(entity))
            throw new ArgumentException("Usuário inválido: verifique nome, e-mail, senha ou se o e-mail já está cadastrado.");

        entity.Senha = PasswordHasher.Hash(entity.Senha);
        await _repository.AddAsync(entity);
    }
}
