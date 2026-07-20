using Contas_Db.Repository;

namespace Contas_Core.UseCase.Usuario;

public class ExcluirUsuarioUseCase
{
    private readonly IRepository<Contas_Db.Model.Usuario> _repository;

    public ExcluirUsuarioUseCase(IRepository<Contas_Db.Model.Usuario> repository)
    {
        _repository = repository;
    }

    public Task ExecuteAsync(int id) => _repository.DeleteAsync(id);
}
