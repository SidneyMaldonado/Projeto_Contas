using Contas_Db.Repository;

namespace Contas_Core.UseCase.Usuario;

public class InativarUsuarioUseCase
{
    private readonly IRepository<Contas_Db.Model.Usuario> _repository;

    public InativarUsuarioUseCase(IRepository<Contas_Db.Model.Usuario> repository)
    {
        _repository = repository;
    }

    public Task ExecuteAsync(int id) => _repository.SoftDeleteAsync(id);
}
