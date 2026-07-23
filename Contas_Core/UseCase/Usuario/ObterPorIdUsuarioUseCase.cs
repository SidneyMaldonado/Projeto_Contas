using Contas_Db.Repository.Interface;

namespace Contas_Core.UseCase.Usuario;

public class ObterPorIdUsuarioUseCase
{
    private readonly IRepository<Contas_Db.Model.Usuario> _repository;

    public ObterPorIdUsuarioUseCase(IRepository<Contas_Db.Model.Usuario> repository)
    {
        _repository = repository;
    }

    public Task<Contas_Db.Model.Usuario?> ExecuteAsync(int id) => _repository.GetByIdAsync(id);
}
