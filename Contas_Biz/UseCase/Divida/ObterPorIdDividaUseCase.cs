using Contas_Db.Repository;

namespace Contas_Biz.UseCase.Divida;

public class ObterPorIdDividaUseCase
{
    private readonly IRepository<Contas_Db.Model.Divida> _repository;

    public ObterPorIdDividaUseCase(IRepository<Contas_Db.Model.Divida> repository)
    {
        _repository = repository;
    }

    public Task<Contas_Db.Model.Divida?> ExecuteAsync(int id) => _repository.GetByIdAsync(id);
}
