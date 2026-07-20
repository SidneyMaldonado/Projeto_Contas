using Contas_Db.Repository;

namespace Contas_Core.UseCase.Conta;

public class ObterPorIdContaUseCase
{
    private readonly IRepository<Contas_Db.Model.Conta> _repository;

    public ObterPorIdContaUseCase(IRepository<Contas_Db.Model.Conta> repository)
    {
        _repository = repository;
    }

    public Task<Contas_Db.Model.Conta?> ExecuteAsync(int id) => _repository.GetByIdAsync(id);
}
