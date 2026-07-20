using Contas_Core.Security;
using Contas_Db.Repository;

namespace Contas_Core.UseCase.Usuario;

public class AdicionarUsuarioUseCase
{
    private readonly IRepository<Contas_Db.Model.Usuario> _repository;

    public AdicionarUsuarioUseCase(IRepository<Contas_Db.Model.Usuario> repository)
    {
        _repository = repository;
    }

    public Task ExecuteAsync(Contas_Db.Model.Usuario entity)
    {
        entity.Senha = PasswordHasher.Hash(entity.Senha);
        return _repository.AddAsync(entity);
    }
}
