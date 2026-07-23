using Contas_Db.Repository.Interface;

namespace Contas_Core.UseCase.Usuario;

public class AtualizarUsuarioUseCase
{
    private readonly IRepository<Contas_Db.Model.Usuario> _repository;

    public AtualizarUsuarioUseCase(IRepository<Contas_Db.Model.Usuario> repository)
    {
        _repository = repository;
    }

    // Não hasheia Senha: troca de senha é feita via AlterarSenhaUsuarioUseCase.
    public Task ExecuteAsync(Contas_Db.Model.Usuario entity) => _repository.UpdateAsync(entity);
}
