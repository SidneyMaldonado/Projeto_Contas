using Contas_Db.Repository;

namespace Contas_Core.UseCase.Credor;

public class InativarCredorUseCase
{
    private readonly IRepository<Contas_Db.Model.Credor> _repository;

    public InativarCredorUseCase(IRepository<Contas_Db.Model.Credor> repository)
    {
        _repository = repository;
    }

    public Task ExecuteAsync(int id) => _repository.SoftDeleteAsync(id);
}
