using Contas_Db.Repository;

namespace Contas_Biz.UseCase.Conta;

public class AdicionarContaUseCase
{
    private readonly IRepository<Contas_Db.Model.Conta> _repository;

    public AdicionarContaUseCase(IRepository<Contas_Db.Model.Conta> repository)
    {
        _repository = repository;
    }

    public Task ExecuteAsync(Contas_Db.Model.Conta entity) => _repository.AddAsync(entity);
}
