using Contas_Db.Repository.Interface;

namespace Contas_Core.UseCase.Usuario;

public class ObterTodosUsuarioUseCase
{
    private readonly IRepository<Contas_Db.Model.Usuario> _repository;

    public ObterTodosUsuarioUseCase(IRepository<Contas_Db.Model.Usuario> repository)
    {
        _repository = repository;
    }

    public Task<IEnumerable<Contas_Db.Model.Usuario>> ExecuteAsync() => _repository.GetAllAsync();
}
