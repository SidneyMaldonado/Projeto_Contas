using Contas_Db.Repository;

namespace Contas_Biz.UseCase.Divida;

public class ExcluirDividaUseCase
{
    private readonly IRepository<Contas_Db.Model.Divida> _repository;

    public ExcluirDividaUseCase(IRepository<Contas_Db.Model.Divida> repository)
    {
        _repository = repository;
    }

    public Task ExecuteAsync(int id) => _repository.DeleteAsync(id);
}
