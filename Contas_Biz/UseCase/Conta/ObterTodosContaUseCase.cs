using Contas_Db.Repository;

namespace Contas_Biz.UseCase.Conta;

public class ObterTodosContaUseCase
{
    private readonly IRepository<Contas_Db.Model.Conta> _repository;

    public ObterTodosContaUseCase(IRepository<Contas_Db.Model.Conta> repository)
    {
        _repository = repository;
    }

    public Task<IEnumerable<Contas_Db.Model.Conta>> ExecuteAsync() => _repository.GetAllAsync();
}
