using Contas_Db.Repository.Interface;

namespace Contas_Core.UseCase.Credor;

public class ObterTodosCredorUseCase
{
    private readonly IRepository<Contas_Db.Model.Credor> _repository;

    public ObterTodosCredorUseCase(IRepository<Contas_Db.Model.Credor> repository)
    {
        _repository = repository;
    }

    public Task<IEnumerable<Contas_Db.Model.Credor>> ExecuteAsync() => _repository.GetAllAsync();
}
