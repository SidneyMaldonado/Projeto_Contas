using Contas_Db.Repository;

namespace Contas_Core.UseCase.Conta;

public class InativarContaUseCase
{
    private readonly IRepository<Contas_Db.Model.Conta> _repository;

    public InativarContaUseCase(IRepository<Contas_Db.Model.Conta> repository)
    {
        _repository = repository;
    }

    public Task ExecuteAsync(int id) => _repository.SoftDeleteAsync(id);
}
