using Contas_Db.Repository;

namespace Contas_Biz.UseCase.Conta;

public class ExcluirContaUseCase
{
    private readonly IRepository<Contas_Db.Model.Conta> _repository;

    public ExcluirContaUseCase(IRepository<Contas_Db.Model.Conta> repository)
    {
        _repository = repository;
    }

    public Task ExecuteAsync(int id) => _repository.DeleteAsync(id);
}
