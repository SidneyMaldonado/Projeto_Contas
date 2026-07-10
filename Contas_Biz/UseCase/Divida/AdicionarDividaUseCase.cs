using Contas_Db.Repository;

namespace Contas_Biz.UseCase.Divida;

public class AdicionarDividaUseCase
{
    private readonly IRepository<Contas_Db.Model.Divida> _repository;

    public AdicionarDividaUseCase(IRepository<Contas_Db.Model.Divida> repository)
    {
        _repository = repository;
    }

    public Task ExecuteAsync(Contas_Db.Model.Divida entity) => _repository.AddAsync(entity);
}
