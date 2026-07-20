using Contas_Db.Repository;

namespace Contas_Core.UseCase.Credor;

public class ObterPorIdCredorUseCase
{
    private readonly IRepository<Contas_Db.Model.Credor> _repository;

    public ObterPorIdCredorUseCase(IRepository<Contas_Db.Model.Credor> repository)
    {
        _repository = repository;
    }

    public Task<Contas_Db.Model.Credor?> ExecuteAsync(int id) => _repository.GetByIdAsync(id);
}
