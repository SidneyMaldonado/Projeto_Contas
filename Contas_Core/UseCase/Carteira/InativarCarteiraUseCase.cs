using Contas_Db.Repository.Interface;

namespace Contas_Core.UseCase.Carteira;

public class InativarCarteiraUseCase
{
    private readonly IRepository<Contas_Db.Model.Carteira> _repository;

    public InativarCarteiraUseCase(IRepository<Contas_Db.Model.Carteira> repository)
    {
        _repository = repository;
    }

    public Task ExecuteAsync(int id) => _repository.SoftDeleteAsync(id);
}
