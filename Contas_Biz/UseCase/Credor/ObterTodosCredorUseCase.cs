using Contas_Db.Repository;

namespace Contas_Biz.UseCase.Credor;

public class ObterTodosCredorUseCase
{
    private readonly IRepository<Contas_Db.Model.Credor> _repository;

    public ObterTodosCredorUseCase(IRepository<Contas_Db.Model.Credor> repository)
    {
        _repository = repository;
    }

    public Task<IEnumerable<Contas_Db.Model.Credor>> ExecuteAsync() => _repository.GetAllAsync();
}
