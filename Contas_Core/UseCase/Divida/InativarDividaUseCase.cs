using Contas_Db.Repository.Interface;

namespace Contas_Core.UseCase.Divida;

public class InativarDividaUseCase
{
    private readonly IRepository<Contas_Db.Model.Divida> _repository;

    public InativarDividaUseCase(IRepository<Contas_Db.Model.Divida> repository)
    {
        _repository = repository;
    }

    public Task ExecuteAsync(int id) => _repository.SoftDeleteAsync(id);
}
