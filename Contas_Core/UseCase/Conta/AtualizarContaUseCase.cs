using Contas_Db.Repository.Interface;

namespace Contas_Core.UseCase.Conta;

public class AtualizarContaUseCase
{
    private readonly IRepository<Contas_Db.Model.Conta> _repository;

    public AtualizarContaUseCase(IRepository<Contas_Db.Model.Conta> repository)
    {
        _repository = repository;
    }

    public Task ExecuteAsync(Contas_Db.Model.Conta entity) => _repository.UpdateAsync(entity);
}
